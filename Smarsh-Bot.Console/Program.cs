using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using static System.Text.RegularExpressions.RegexOptions;

namespace Smarsh_Bot
{
    public class Program
    {
        private static readonly Regex PrefixRegex = new Regex("^> ?");
        private static readonly string[] StandardTimeZones = { "Pacific Time", "Central Time", "Eastern Time" };
        private static readonly ReadOnlyCollection<TimeZoneInfo> SystemTimeZones = TimeZoneInfo.GetSystemTimeZones();
        private static readonly Random Random = new Random();

        private DiscordSocketClient _client;

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private static Task Log(LogMessage msg) => Task.Factory.StartNew(() => Console.WriteLine(msg.ToString()));

        private async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            await _client.LoginAsync(TokenType.Bot, "MzQyNDQwNzI4ODEyODQ3MTA0.DGejaw.vIBWmZW7ipc_wY1t6xgt0vPJ9Lc");
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static readonly Dictionary<string, Func<Command, string>> Handlers = new Dictionary<string,Func<Command,string>>
        {
            { $@"""help"" - display all commands.", Help },
            { $@"""ping"" - Test the bot's resposiveness.", Ping },
            { $@"""roll (number)?"" - Roll between 1 and a number, defaulting to 20.", Roll },
            { $@"""should (I|we|...) (verb)?: <1>, ...,( or)? <N>"" - Randomly choose one of many options.", Choose },
            { $@"""time(zones)?( (number) (hours|minutes) from)? now (with <1>, ..., <N>)?"" - Display time in different time zones.", TimeZones },
            { @"""what time is it( in <1>, ..., <N>)?"" - Display the current times either in America or a specific list of time zones.", WhatTime }
        };

        private async Task MessageReceived(SocketMessage message)
        {
            var prefixMatch = PrefixRegex.Match(message.Content);

            if (prefixMatch.Success == false) return;

            var command = new Command(
                new string(message.Content.Skip(prefixMatch.Value.Length).ToArray()),
                GetGuildUser(message.Channel.Id, message.Author.Id)?.Nickname ?? message.Author.Username
            );

            await message.Channel.SendMessageAsync(
                Handlers.Select(h => h.Value(command)).SkipWhile(r => r == null).FirstOrDefault() ??
                $"I'm afraid I don't understand, {command.Author}."
            );
        }

        private SocketGuildUser GetGuildUser(ulong channelId, ulong userId) => _client
            .Guilds.Select(g => g
                .Channels.SingleOrDefault(c => c.Id == channelId)?
                .Users.SingleOrDefault(u => u.Id == userId)
            ).SkipWhile(u => u == null).FirstOrDefault()
        ;

        private static string Help(Command command) => 
            Regex.IsMatch(command.Text, "help", IgnoreCase) == false ? null : 
                string.Join(Environment.NewLine, Handlers.Select(p => p.Key));

        private static string Ping(Command command) => 
            Regex.IsMatch(command.Text, "ping", IgnoreCase) ? "Pong!" : null;

        private static string Roll(Command command)
        {
            var match = Regex.Match(command.Text, "^roll (?<num>\\d+)?", IgnoreCase);

            if (match.Success == false) return null;

            var numMatch = match.Groups["num"];
            var num = numMatch.Success ? int.Parse(numMatch.Value) : 20;

            return $"{command.Author}, you rolled a {Random.Next(num) + 1} (out of a possible {num}).";
        }

        private static string Choose(Command command)
        {
            var match = Regex.Match(command.Text,
                "^should (?:we|I|(?<other>\\S+))(?<verb> .*?)?: ?(?<options>.+?(?:,?(?: or)? .+?)*)\\??$",
                IgnoreCase
            );

            if (match.Success == false) return null;

            var otherMatch = match.Groups["other"];
            var verbMatch = match.Groups["verb"];
            var optionsMatch = match.Groups["options"];

            if (optionsMatch.Success == false) return null;

            var options = optionsMatch.Value
                .Split(new[] {",", " or "}, StringSplitOptions.RemoveEmptyEntries);

            options = options.Except(options.Where(string.IsNullOrWhiteSpace)).ToArray();

            //await message.Channel.SendMessageAsync($"Choosing from {options.Length} options...");

            //Thread.Sleep(Random.Next(1000, 5000));

            var subject = otherMatch.Success ? otherMatch.Value : "you";

            var verb = verbMatch.Success ? verbMatch.Value.Trim() : "";

            var choice = options[Random.Next(options.Length)].Trim();

            return $"I think that {subject} should {(verb == "" ? "" : verb + " ")}{choice}.".Replace(" my", " your");
        }

        private static string TimeZones(Command command)
        {
            var match = Regex.Match(command.Text, 
                "^time(zones)?(?<offset> (?<c>\\d+) (?<period>hours?|minutes?) from)? now(?: with (?<ex>.+))?", 
                IgnoreCase
            );

            if (match.Success == false) return null;

            var now = DateTime.UtcNow;

            if (match.Groups["offset"].Success)
            {
                var c = int.Parse(match.Groups["c"].Value);
                var period = match.Groups["period"].Value;

                if (Regex.Match(period, "hours?").Success)
                    now = now.AddHours(c);
                else if (Regex.Match(period, "minutes?").Success)
                    now = now.AddMinutes(c);
            }

            var extraMatch = match.Groups["ex"];
            var extraZones = extraMatch.Success ? extraMatch.Value.Split(',').ToList() : Enumerable.Empty<string>();

            var messages = new List<string>();
            foreach (var timeZoneInfo in StandardTimeZones
                .Union(extraZones).Select(s => s.Trim())
                .ToDictionary(s => s, s => SystemTimeZones.FirstOrDefault(z => 1 == 0
                ||  Regex.IsMatch(z.Id, s, IgnoreCase)
                ||  Regex.IsMatch(z.StandardName, s, IgnoreCase)
                ||  Regex.IsMatch(z.DisplayName, s, IgnoreCase)
                )))
            {
                string label;
                string display;
                if (timeZoneInfo.Value == null)
                {
                    label = timeZoneInfo.Key;
                    display = "Not Found";
                }
                else
                {
                    var offset = timeZoneInfo.Value.BaseUtcOffset;

                    if (timeZoneInfo.Value.IsDaylightSavingTime(now))
                    {
                        offset = offset.Add(TimeSpan.FromHours(1));
                        label = timeZoneInfo.Value.DaylightName;
                    }
                    else
                    {
                        label = timeZoneInfo.Value.StandardName;
                    }

                    display = now.Add(offset).ToString("h:mm:ss tt");
                }

                messages.Add($"{label}: {display}");
            }

            return string.Join(Environment.NewLine, messages);
        }

        private static string WhatTime(Command command)
        {
            var match = Regex.Match(command.Text, 
                "^what time is it(?<specific> in (?<ex>.+))?", 
                IgnoreCase
            );

            if (match.Success == false) return null;

            var now = DateTime.UtcNow;
            
            IEnumerable<string> timeZones;

            if (match.Groups["specific"].Success)
            {
                timeZones = match.Groups["ex"]
                    .Value.Split(',').Select(s => s.Trim()).ToList();
            }
            else
            {
                timeZones = StandardTimeZones;
            }

            var messages = new List<string>();
            foreach (var timeZoneInfo in timeZones
                .ToDictionary(s => s, s => SystemTimeZones.FirstOrDefault(z => 1 == 0
                ||  Regex.IsMatch(z.Id, s, IgnoreCase)
                ||  Regex.IsMatch(z.StandardName, s, IgnoreCase)
                ||  Regex.IsMatch(z.DisplayName, s, IgnoreCase)
                )))
            {
                string label;
                string display;
                if (timeZoneInfo.Value == null)
                {
                    label = timeZoneInfo.Key;
                    display = "Not Found";
                }
                else
                {
                    var offset = timeZoneInfo.Value.BaseUtcOffset;

                    if (timeZoneInfo.Value.IsDaylightSavingTime(now))
                    {
                        offset = offset.Add(TimeSpan.FromHours(1));
                        label = timeZoneInfo.Value.DaylightName;
                    }
                    else
                    {
                        label = timeZoneInfo.Value.StandardName;
                    }

                    display = now.Add(offset).ToString("h:mm:ss tt");
                }

                messages.Add($"{label}: {display}");
            }

            return string.Join(Environment.NewLine, messages);
        }

        private class Command
        {
            public string Text { get; }
            public string Author { get; }

            public Command(string text, string author)
            {
                Text = text;
                Author = author;
            }
        }
    }
}
