using Discord;
using Discord.WebSocket;

namespace Discord_Bot
{
    public class Command
    {
        public string Text { get; }
        public SocketMessage Message { get; }

        public Command(string prefix, SocketMessage message)
        {
            Text = message.Content.Substring(prefix.Length).Trim();
            Message = message;
        }

        public string MentionAuthor => MentionUtils.MentionUser(Message.Author.Id);
    }
}
