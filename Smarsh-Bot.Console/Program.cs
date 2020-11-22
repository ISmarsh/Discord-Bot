using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Smarsh_Bot
{
  /// <summary>
  /// This is the first bot I wrote, for the first server I participated in heavily.
  /// All of the commands were simple enough and generally useful, so I moved them to my base class.
  /// </summary>
  public class Program : Discord_Bot.Base
  {
    protected override string Name => "Smarsh_Bot";

    public Program() : base(prefix: ";") { }

    public static void Main(string[] args) => Task.WaitAll(new Program().RunAsync());
  }
}
