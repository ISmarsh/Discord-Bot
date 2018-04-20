using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using static Discord.MentionUtils;

namespace Discord_Bot
{
    public abstract class Base
    {
        protected static readonly Random Random = new Random();

        protected abstract string Prefix { get; }
        private Regex PrefixRegex => new Regex($"^{Prefix} ?");

        protected DiscordSocketClient Client;
        protected static List<(CommandAttribute Command, Func<Command, string> Delegate)> Handlers;

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
                    && method.ReturnParameter?.ParameterType == typeof(string)
                    && parameters.Length == 1 && parameters[0].ParameterType == typeof(Command)
                    select method
                );
            }

            Handlers = methods.SelectMany(m => m.GetCustomAttributes<CommandAttribute>()
                .Select(c => (Command: c, Delegate: (Func<Command, string>) m.CreateDelegate(typeof(Func<Command, string>))))
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

            await message.Channel.SendMessageAsync(
                Handlers.Select(h =>
                {
                    var match = h.Command.Pattern.Match(commandText);

                    return match.Success ? h.Delegate(new Command(message, match)) : null;
                }).FirstOrDefault(s => s != null) ??
                $"I'm afraid I don't understand, {MentionUser(message.Author.Id)}."
            );
        }

        [Command("help", "help", "Display all commands.")]
        public static string Help(Command command) => string.Join(Environment.NewLine, Handlers
            .Select(x => $@"""{x.Command.Hint}"" - {x.Command.Description}")
        );

        [Command("ping!?", "ping", "Test the bot's resposiveness.")]
        public static string Ping(Command command) => $"{command.MentionAuthor} Pong!";

        [Command("roll(?<num> \\d+)?", "roll (#)?", "Roll between 1 and a number, defaulting to 20.")]
        public static string Roll(Command command)
        {
            var num = command["num"].Success ? int.Parse(command["num"].Value) : 20;

            return $"{command.MentionAuthor}, you rolled a {Random.Next(num) + 1} (out of a possible {num}).";
        }

        [Command(
            "should (?:I|we|(?<other>\\S+))(?<verb> .*?)?: ?(?<options>.+?(?:,?(?: or)? .+?)*)\\??",
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
    }
}
