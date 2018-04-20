using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using static System.Text.RegularExpressions.RegexOptions;

namespace Discord_Bot
{
    public abstract class Program
    {
        protected const RegexOptions RegexOptions = IgnoreCase | Compiled;
        protected static readonly Random Random = new Random();

        protected abstract string Prefix { get; }
        private Regex PrefixRegex => new Regex($"^{Prefix} ?");

        protected abstract Dictionary<string, Func<Command, string>> GetHandlers();

        protected DiscordSocketClient Client;
        protected Dictionary<string, Func<Command, string>> Handlers;

        protected async Task RunAsync()
        {
            Client = new DiscordSocketClient();
            Handlers = GetHandlers();

            Client.Log += Log;
            Client.MessageReceived += MessageReceived;

            await Client.LoginAsync(TokenType.Bot, ConfigurationManager.AppSettings["Token"]);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg) => Task.Factory.StartNew(() => Console.WriteLine(msg.ToString()));

        private async Task MessageReceived(SocketMessage message)
        {
            var prefixMatch = PrefixRegex.Match(message.Content);

            if (prefixMatch.Success == false) return;

            var command = new Command(
                message.Content.Substring(prefixMatch.Value.Length),
                GetGuildUser(message.Channel.Id, message.Author.Id)?.Nickname ?? message.Author.Username
            );

            await message.Channel.SendMessageAsync(
                Handlers.Select(h => h.Value(command)).SkipWhile(r => r == null).FirstOrDefault() ??
                $"I'm afraid I don't understand, {command.Author}."
            );
        }

        private SocketGuildUser GetGuildUser(ulong channelId, ulong userId) => Client
            .Guilds.Select(g => g
                .Channels.SingleOrDefault(c => c.Id == channelId)?
                .Users.SingleOrDefault(u => u.Id == userId)
            ).SkipWhile(u => u == null).FirstOrDefault()
        ;

        protected string Help(Command command) =>
            Regex.IsMatch(command.Text, "help", RegexOptions) == false ? null :
                string.Join(Environment.NewLine, Handlers.Select(p => p.Key));

        protected static string Ping(Command command) =>
            Regex.IsMatch(command.Text, "ping", RegexOptions) ? "Pong!" : null;

        protected static string Roll(Command command)
        {
            var match = Regex.Match(command.Text, "^roll (?<num>\\d+)?", RegexOptions);

            if (match.Success == false) return null;

            var numMatch = match.Groups["num"];
            var num = numMatch.Success ? int.Parse(numMatch.Value) : 20;

            return $"{command.Author}, you rolled a {Random.Next(num) + 1} (out of a possible {num}).";
        }

        protected static string Choose(Command command)
        {
            var match = Regex.Match(command.Text,
                "^should (?:we|I|(?<other>\\S+))(?<verb> .*?)?: ?(?<options>.+?(?:,?(?: or)? .+?)*)\\??$",
                RegexOptions
            );

            if (match.Success == false) return null;

            var otherMatch = match.Groups["other"];
            var verbMatch = match.Groups["verb"];
            var optionsMatch = match.Groups["options"];

            if (optionsMatch.Success == false) return null;

            var options = optionsMatch.Value
                .Split(new[] { ",", " or " }, StringSplitOptions.RemoveEmptyEntries);

            options = options.Except(options.Where(string.IsNullOrWhiteSpace)).ToArray();

            //await message.Channel.SendMessageAsync($"Choosing from {options.Length} options...");

            //Thread.Sleep(Random.Next(1000, 5000));

            var subject = otherMatch.Success ? otherMatch.Value : "you";

            var verb = verbMatch.Success ? verbMatch.Value.Trim() : "";

            var choice = options[Random.Next(options.Length)].Trim();

            return $"I think that {subject} should {(verb == "" ? "" : verb + " ")}{choice}.".Replace(" my", " your");
        }
    }
}
