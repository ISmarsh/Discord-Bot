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
        protected override string Prefix => ">";
        protected override Dictionary<string, Func<Command, string>> GetHandlers() => new Dictionary<string, Func<Command, string>>
        {
            { @"""help"" - display all commands.", Help },
            { @"""ping"" - Test the bot's resposiveness.", Ping },
            { @"""roll (number)?"" - Roll between 1 and a number, defaulting to 20.", Roll },
            { @"""should (I|we|...) (verb)?: <1>, ...,( or)? <N>"" - Randomly choose one of many options.", Choose },
            //{ @"",  },
        };

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
