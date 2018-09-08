using System;
using System.Collections.Generic;
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
            StandardTimeZones = StandardTimeZones.Concat(new[] { "GMT", "AUS Eastern"}).ToArray();
        }

        protected override string Prefix { get; } = ";";

        public static void Main(string[] args) => Task.WaitAll(new Program().RunAsync());

        protected override async Task UserJoined(SocketGuildUser user)
        {
            var mods = user.Guild.Roles.Single(r => r.Name == "Moderators");
            var general = user.Guild.TextChannels.FirstOrDefault(c => c.Name == "welcome");

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
            string.Format(NotFoundMessages[Random.Next(NotFoundMessages.Length)], MentionUser(message.Author.Id));
        private static readonly string[] NotFoundMessages = new List<IEnumerable<string>>
        {
            Enumerable.Repeat("Could you repeat that, {0}?", 32),
            Enumerable.Repeat("I ask of you, are you my master?", 32),
            Enumerable.Repeat("Go home goshujin-sama, you're drunk", 32),
            Enumerable.Repeat("Never show your stupid self in public, {0}.", 4)
        }.SelectMany(e => e).ToArray();

        //NOTE: this only makes sense when the prefix is ';'
        [Command(@"[-_];", "", "")]
        public static string Crying(Command command) =>
            string.Format(CryingReplies[Random.Next(CryingReplies.Length)], command.MentionAuthor);
        private static readonly string[] CryingReplies = new List<IEnumerable<string>>
        {
            Enumerable.Repeat("Cheer up master!", 48),
            Enumerable.Repeat("Genki desu ka?", 48),
            Enumerable.Repeat("stop crying you piece of shit", 4),
        }.SelectMany(e => e).ToArray();

        [Command(@"badda\s*", "badda", "")]
        public static string Badda(Command command) => $"{command.MentionAuthor} badda WHAT";

        [Command("badda ?bing!?", "baddabing", "")]
        public static string BaddaBing(Command command) => $"{command.MentionAuthor} badda BOOM!";

        [Command("badda ?boom!?", "baddaboom", "")]
        public static string BaddaBoom(Command command) => $"{command.MentionAuthor} do it in the right order you klutz";

        [Command("honk!?", "honk", "")]
        public static string Honk(Command command) => $"{command.MentionAuthor} HEY IM SKATIN' OVA' HERE!";

        [Command("source", "source", "The source for the profile picture.")]
        public static string Source(Command command) => $"{command.MentionAuthor} https://www.pixiv.net/member_illust.php?mode=medium&illust_id=68594526";
    }
}
