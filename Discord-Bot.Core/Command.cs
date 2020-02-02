using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace Discord_Bot
{
    public class Command
    {
        public SocketMessage Message { get; }
        public Match Match { get; }

        public Command(SocketMessage message, Match match)
        {
            Message = message;
            Match = match;
        }

        public Group this[string name] => Match.Groups[name];

        public string MentionAuthor => MentionUtils.MentionUser(Message.Author.Id);
    }
}
