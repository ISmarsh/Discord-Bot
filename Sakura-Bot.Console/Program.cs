using Discord.WebSocket;
using Discord_Bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Discord.MentionUtils;

namespace Sakura_Bot
{
  /// <summary>
  /// A custom bot I wrote for a friend from my first server.
  /// </summary>
  public class Program : Discord_Bot.Base
  {
    protected override string Name => "Sakura_Bot";

    public Program() : base(prefix: ";")
    {
      StandardTimeZones = StandardTimeZones.Concat(new[] { "Mountain Time", "GMT", "AUS Eastern" }).ToArray();
    }

    public static void Main(string[] args) => Task.WaitAll(new Program().RunAsync());

    protected override async Task UserJoined(SocketGuildUser user)
    {
      var welcome = user.Guild.TextChannels.FirstOrDefault(c => c.Name == "welcome");

      if (welcome == null) return;

      await welcome.SendMessageAsync(GetUserJoinedMessage(user.Id));
    }

    private string GetUserJoinedMessage(ulong userId)
    {
      return string.Join(Environment.NewLine,
        $"Welcome {MentionUser(userId)} to StickerHub!",
        $"Are you a sticker collector or vendor?",
        $"(Some verification may be required for vendors.)"
      );
    }

    [Command("test ?join", "", "")]
    public string TestJoin(Command command)
    {
      if (command.Message.Author.ToString() == "Smarshian#4242")
        return GetUserJoinedMessage(command.Message.Author.Id);

      return null;
    }

    protected override async Task UserLeft(SocketGuildUser user)
    {
      var general = user.Guild.TextChannels.FirstOrDefault(c => c.Name == "general");

      if (general == null) return;

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
    public string Crying(Command command) =>
      string.Format(CryingReplies[Random.Next(CryingReplies.Length)], command.MentionAuthor);
    private static readonly string[] CryingReplies = new List<IEnumerable<string>>
    {
      Enumerable.Repeat("Cheer up master!", 48),
      Enumerable.Repeat("Genki desu ka?", 48),
      Enumerable.Repeat("stop crying you piece of shit", 4),
    }.SelectMany(e => e).ToArray();

    [Command(@"gacha", "gacha", "")]
    public string Gacha(Command command) => $"{command.MentionAuthor} **{GachaReplies[Random.Next(GachaReplies.Length)]}!**";
    private readonly string[] GachaReplies = new List<IEnumerable<string>>
    {
      Enumerable.Repeat("Merlin", 100),
      Enumerable.Repeat("Nitocris", 100),
      Enumerable.Repeat("Jeanne", 100),
      Enumerable.Repeat("Jeanne Alter", 100),
      Enumerable.Repeat("Semiramis", 100),
      Enumerable.Repeat("Scathach", 100),
      Enumerable.Repeat("Frankensetein", 100),
      Enumerable.Repeat("Musashi", 42),
      Enumerable.Repeat("Gilgamesh", 42),
      Enumerable.Repeat("Artoria Alter", 42),
      Enumerable.Repeat("Astolfo", 42),
      Enumerable.Repeat("Tamamo no mae", 42),
      Enumerable.Repeat("Scathach (beach)", 42),
      Enumerable.Repeat("Kiyohime", 42),
    }.SelectMany(e => e).ToArray();

    [Command(@"badda\s*", "badda", "")]
    public string Badda(Command command) => $"{command.MentionAuthor} badda WHAT";

    [Command("badda ?bing!?", "baddabing", "")]
    public string BaddaBing(Command command) => $"{command.MentionAuthor} badda BOOM!";

    [Command("badda ?boom!?", "baddaboom", "")]
    public string BaddaBoom(Command command) => $"{command.MentionAuthor} do it in the right order you klutz";

    [Command("honk!?", "honk", "")]
    public string Honk(Command command) => $"{command.MentionAuthor} HEY IM WALKIN' OVA' HERE!";

    [Command("source", "source", "The source for the profile picture.")]
    public string Source(Command command) => $"{command.MentionAuthor} https://www.pixiv.net/artworks/79053274";
  }
}
