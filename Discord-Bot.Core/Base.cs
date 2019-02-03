using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using static System.Text.RegularExpressions.RegexOptions;
using static Discord.MentionUtils;

namespace Discord_Bot
{
    public abstract class Base
    {
        protected static string[] StandardTimeZones = { "Pacific Time", "Central Time", "Eastern Time" };
        protected static readonly ReadOnlyCollection<TimeZoneInfo> SystemTimeZones = TimeZoneInfo.GetSystemTimeZones();
        protected static readonly Random Random = new Random();

        protected abstract string Prefix { get; }
        private Regex PrefixRegex => new Regex($"^{Prefix} ?");

        protected DiscordSocketClient Client;
        protected static List<(CommandAttribute Command, Delegate Delegate, Type ReturnType)> Handlers;

        protected Base()
        {
            var type = GetType();
            var types = new List<Type>();
            do
            {
                types.Add(type);
            } while ((type = type.BaseType) != null);
            types.Reverse();

            var methods = new List<MethodInfo>();
            foreach (var t in types)
            {
                methods.AddRange(
                    from method in t.GetMethods()
                    let parameters = method.GetParameters()
                    where 1==1
                    && method.IsStatic 
                    && new[]{ typeof(string), typeof(string[]) }.Contains(method.ReturnParameter?.ParameterType)
                    && parameters.Length == 1 && parameters[0].ParameterType == typeof(Command)
                    select method
                );
            }

            Handlers = methods.SelectMany(m => m.GetCustomAttributes<CommandAttribute>()
                .Select(c =>
                {
                    Delegate @delegate;
                    var returnType = m.ReturnParameter.ParameterType;

                    if (returnType == typeof(string[]))
                    {
                        @delegate = m.CreateDelegate(typeof(Func<Command, string[]>));
                    }
                    else
                    {
                        @delegate = m.CreateDelegate(typeof(Func<Command, string>));
                    }

                    return (Command: c, Delegate: @delegate, ReturnType: returnType);
                })
            ).ToList();
        }

        protected async Task RunAsync()
        {
            Client = new DiscordSocketClient();

            Client.Log += Log;
            Client.MessageReceived += MessageReceived;
            Client.UserJoined += UserJoined;
            Client.UserLeft += UserLeft;

            await Client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["Token"]);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg) => Task.Factory.StartNew(() => Console.WriteLine(msg.ToString()));

        protected virtual Task UserJoined(SocketGuildUser user) => Task.CompletedTask;
        protected virtual Task UserLeft(SocketGuildUser socketGuildUser) => Task.CompletedTask;

        private async Task MessageReceived(SocketMessage message)
        {
            var prefixMatch = PrefixRegex.Match(message.Content);

            if (prefixMatch.Success == false) return;

            var commandText = message.Content.Substring(prefixMatch.Value.Length).Trim();

            var responses = Handlers.Select(h =>
            {
                var match = h.Command.Pattern.Match(commandText);

                if (match.Success == false) return null;

                var command = new Command(message, match);

                var messagess = h.ReturnType == typeof(string[])
                    ? ((Func<Command, string[]>) h.Delegate)(command)
                    : new[] {((Func<Command, string>) h.Delegate)(command)};

                return messagess.Where(m => m != null)
                    .Select(m => h.Command.OutputFixedWidth ? $"```{m}```" : m);

            }).FirstOrDefault(s => s != null);

            foreach (var response in responses ?? new [] { NotFoundMessage(message) })
            {
                await message.Channel.SendMessageAsync(response);
            }
        }

        protected virtual string NotFoundMessage(SocketMessage message) => 
            $"I'm afraid I don't understand, {MentionUser(message.Author.Id)}.";

        [Command("help", "help", "Display all commands.")]
        public static string Help(Command command) => string.Join(Environment.NewLine, Handlers
            .Where(x => x.Command.Description.Length > 0)
            .Select(x => $@"""{x.Command.Hint}"" - {x.Command.Description}")
        );

        [Command("ping!?", "ping", "Test the bot's resposiveness.")]
        public static string Ping(Command command) => $"{command.MentionAuthor} Pong!";

        [Command("roll(?<num> [1-9]\\d*)?", "roll (#)?", "Roll between 1 and a number, defaulting to 20.")]
        public static string Roll(Command command)
        {
            var num = command["num"].Success ? int.Parse(command["num"].Value) : 20;

            var result = Random.Next(num) + 1;
            var display = result.ToString("N0") + (result == 69 ? " (nice)" : "");

            return $"{command.MentionAuthor}, you rolled a {display} out of a possible {num:N0}.";
        }

        [Command(
            "should (I|we|(?<other>\\S+))(?<verb> .*?)?: ?(?<options>.+?(,?( or)? .+?)*)\\??",
            "should (I|we|...) (verb)?: <1>, ...,( or)? <N>", "Randomly choose one of many options."
        )]
        public static string Choose(Command command)
        {
            var optionsMatch = command["options"];

            if (optionsMatch.Success == false) return null;

            var options = optionsMatch.Value
                .Split(new[] { ",", " or " }, StringSplitOptions.RemoveEmptyEntries);

            options = options.Except(options.Where(string.IsNullOrWhiteSpace)).ToArray();

            //await message.Channel.SendMessageAsync($"Choosing from {options.Length} options...");

            //Thread.Sleep(Random.Next(1000, 5000));

            var subject = command["other"].Success ? command["other"].Value : "you";

            var verb = command["verb"].Success ? $"{command["verb"].Value.Trim()} " : "";

            var choice = options[Random.Next(options.Length)].Trim();

            return $"I think that {subject} should {verb}{choice}.".Replace(" my", " your");
        }

        #region Time Commands/Utilities
        [Command(
            "time(zones)?((?<offset> (?<c>\\d+) (?<period>months?|days?|hours?|minutes?))+ from now)?( with (?<ex>.+))?",
            "time(zones)?(( (number) (months|days|hours|minutes))+ from now)? (with <1>, ..., <N>)?",
            "Display time in different time zones.",
            OutputFixedWidth = true
        )]
        public static string TimeZones(Command command)
        {
            var baseTime = DateTime.UtcNow;

            if (command["offset"].Success)
            {
                for (var i = 0; i < command["offset"].Captures.Count; i++)
                {
                    var c = int.Parse(command["c"].Captures[i].Value);
                    var period = command["period"].Captures[i].Value.ToLower();

                    if (Regex.Match(period, "months?").Success)
                        baseTime = baseTime.AddMonths(c);
                    else if (Regex.Match(period, "days?").Success)
                        baseTime += TimeSpan.FromDays(c);
                    else if (Regex.Match(period, "hours?").Success)
                        baseTime += TimeSpan.FromHours(c);
                    else if (Regex.Match(period, "minutes?").Success)
                        baseTime += TimeSpan.FromMinutes(c);
                }
            }

            var extraMatch = command["ex"];
            var extraZones = extraMatch.Success ? extraMatch.Value.Split(',').ToList() : Enumerable.Empty<string>();
            var timeZones = StandardTimeZones.Union(extraZones).Select(s => s.Trim());

            return GetTimes(timeZones, baseTime, displayDate: baseTime > DateTime.UtcNow.AddDays(1));
        }

        [Command(
            "what time is it(?<specific> in (?<ex>.+))?", "what time is it( in <1>, ..., <N>)?",
            "Display the current times either in America or a specific list of time zones.",
            OutputFixedWidth = true
        )]
        public static string WhatTime(Command command)
        {
            IEnumerable<string> timeZones;
            if (command["specific"].Success)
            {
                timeZones = command["ex"].Value.Split(',').Select(s => s.Trim()).ToList();
            }
            else
            {
                timeZones = StandardTimeZones;
            }

            return GetTimes(timeZones, DateTime.UtcNow);
        }

        private static string GetTimes(
            IEnumerable<string> timeZones, DateTime baseTime, bool displayDate = false)
        {
            var times = new Dictionary<string, DateTime?>();

            foreach (var timeZoneInfo in timeZones
                .ToDictionary(s => s, s => SystemTimeZones.FirstOrDefault(z => 1 == 0
                || Regex.IsMatch(z.Id, s, IgnoreCase | Compiled)
                || Regex.IsMatch(z.StandardName, s, IgnoreCase | Compiled)
                || Regex.IsMatch(z.DisplayName, s, IgnoreCase | Compiled)
                )))
            {
                string label;
                DateTime? dateTime;
                if (timeZoneInfo.Value == null)
                {
                    label = timeZoneInfo.Key;
                    dateTime = null;
                }
                else
                {
                    var offset = timeZoneInfo.Value.BaseUtcOffset;

                    if (timeZoneInfo.Value.IsDaylightSavingTime(baseTime))
                    {
                        offset += TimeSpan.FromHours(1);
                        label = timeZoneInfo.Value.DaylightName;
                    }
                    else
                    {
                        label = timeZoneInfo.Value.StandardName;
                    }

                    dateTime = baseTime.Add(offset);
                }

                if (times.ContainsKey(label)) continue;

                times.Add(label, dateTime);
            }

            var max = times.Max(p => p.Key.Length);

            var format = "hh:mm tt";

            if (displayDate) format = "MM/dd/yy " + format + " (ddd)";

            return string.Join(Environment.NewLine, times.OrderBy(p => p.Value).Select(p =>
                $"{p.Key.PadRight(max)} : {p.Value?.ToString(format) ?? "Not Found".PadLeft(format.Length)}"));
        }
        #endregion
    }
}
