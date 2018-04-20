using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using static Discord.MentionUtils;

namespace Sakura_Bot
{
    /// <summary>
    /// A custom bot I wrote for a friend from my first server.
    /// </summary>
    public class Program : Discord_Bot.Base
    {
        protected override string Prefix { get; } = ">";

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

            await general.SendMessageAsync($"Thanks for stopping by {MentionUser(user.Id)}!");
        }
    }
}
