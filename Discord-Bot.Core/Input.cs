using Discord;
using Discord.WebSocket;
using System.Text.RegularExpressions;

namespace Discord_Bot
{
  public class Input
  {
    public SocketMessage Message { get; }
    public Match Match { get; }

    public Input(SocketMessage message, Match match)
    {
      Message = message;
      Match = match;
    }

    public Group this[string name] => Match.Groups[name];

    public string MentionAuthor => MentionUtils.MentionUser(Message.Author.Id);
  }
}
