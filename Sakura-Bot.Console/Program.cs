using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord_Bot;
using static Discord.MentionUtils;

namespace Sakura_Bot
{
    /// <summary>
    /// A custom bot I wrote for a friend from my first server.
    /// </summary>
    public class Program : Discord_Bot.Base
    {
        static Program()
        {
            StandardTimeZones = StandardTimeZones.Concat(new[] { "GMT", "Taipei", "AUS Eastern"}).ToArray();
        }

        protected override string Prefix { get; } = ";";

        public static void Main(string[] args) => Task.WaitAll(new Program().RunAsync());

        protected override async Task UserJoined(SocketGuildUser user)
        {
            var mods = user.Guild.Roles.Single(r => r.Name == "Moderators");
            var general = user.Guild.TextChannels.FirstOrDefault(c => c.Name == "general");

            await general.SendMessageAsync(string.Join(Environment.NewLine,
                $"Welcome {MentionUser(user.Id)} to StickerHub!",
                $"Be sure to ping the {MentionRole(mods.Id)} to pick your role!",
                "(Some verification may be required.)"
            ));
        }

        protected override async Task UserLeft(SocketGuildUser user)
        {
            var general = user.Guild.TextChannels.FirstOrDefault(c => c.Name == "general");

            await general.SendMessageAsync($"Thanks for stopping by {user.Nickname ?? user.Username}!");
        }

        protected override string NotFoundMessage(SocketMessage message) =>
            $"Fuggedaboutit, {MentionUser(message.Author.Id)}.";

        [Command(@"badda\s*", "badda", "")]
        public static string Badda(Command command) => $"{command.MentionAuthor} badda WHAT";

        [Command("badda ?bing!?", "baddabing", "")]
        public static string BaddaBing(Command command) => $"{command.MentionAuthor} badda BOOM!";

        [Command("badda ?boom!?", "baddaboom", "")]
        public static string BaddaBoom(Command command) => $"{command.MentionAuthor} do it in the right order you klutz";

        [Command("honk!?", "honk", "")]
        public static string Honk(Command command) => $"{command.MentionAuthor} HEY IM WAHLKIN' OVA' HERE!";
    }
}
