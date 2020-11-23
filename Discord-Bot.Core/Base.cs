using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Discord.MentionUtils;
using static System.Text.RegularExpressions.RegexOptions;

namespace Discord_Bot
{
  public abstract class Base
  {
    private static readonly Type InputType = typeof(Input);
    private static readonly Type OutputType = typeof(Output);

    private readonly string _prefix;
    private readonly Regex _commandRegex;
    private readonly Regex _reactRegex;
    private readonly IConfiguration _configuration;
    private readonly List<Command> _commands;

    protected Base(string prefix)
    {
      _commandRegex = new Regex($"^{_prefix = prefix}(?!{prefix}) ?");
      _reactRegex = new Regex($"(^|\n){prefix}{prefix} (?<reactions>.+?([, ]{{1,2}}.+?)*)$", Multiline);

      _configuration = new ConfigurationBuilder()
        .AddUserSecrets(Assembly.GetCallingAssembly())
        .AddEnvironmentVariables()
        .Build();

      _commands = GetType().GetMethods().SelectMany(GetHandlers).ToList();
    }

    private IEnumerable<Command> GetHandlers(MethodInfo method)
    {
      var parameters = method.GetParameters();
      if (parameters.Length != 1 || parameters[0].ParameterType != InputType) yield break;

      var returnType = method.ReturnParameter?.ParameterType;
      if (returnType == null || returnType != OutputType) yield break;

      var inputExpression = Expression.Parameter(typeof(Input), "input");
      var func = Expression.Lambda<Func<Input, Output>>(
        Expression.Call(Expression.Constant(this), method, inputExpression),
        inputExpression
      ).Compile();

      foreach (var attribute in method.GetCustomAttributes<CommandAttribute>())
      {
        yield return new Command(attribute, func);
      }
    }

    protected abstract string Name { get; }
    protected readonly DiscordSocketClient Client = new DiscordSocketClient();
    protected string[] StandardTimeZones { get; set; } = { "Pacific Time", "Central Time", "Eastern Time" };
    protected readonly ReadOnlyCollection<TimeZoneInfo> SystemTimeZones = TimeZoneInfo.GetSystemTimeZones();
    protected readonly Random Random = new Random();

    protected async Task RunAsync()
    {
      Client.Log += Log;
      Client.MessageReceived += MessageReceived;
      Client.UserJoined += UserJoined;
      Client.UserLeft += UserLeft;

      await Client.LoginAsync(TokenType.Bot, _configuration[$"Discord:Token:{Name}"]);
      await Client.StartAsync();

      await Task.Delay(-1);
    }

    private Task Log(LogMessage msg) => Task.Factory.StartNew(() => Console.WriteLine(msg.ToString()));

    private async Task MessageReceived(SocketMessage message)
    {
      var commandMatch = _commandRegex.Match(message.Content);
      var reactMatch = _reactRegex.Match(message.Content);

      IMessage responseMessage = null;
      if (commandMatch.Success)
      {
        var commandText = message.Content;

        if (reactMatch.Success)
        {
          commandText = commandText.Substring(0, reactMatch.Index);
        }

        commandText = commandText.Substring(commandMatch.Value.Length).Trim();

        Output output = null;
        foreach (var command in _commands)
        {
          var match = command.Pattern.Match(commandText);

          if (match.Success == false) continue;

          output = command.Func(new Input(message, match));

          if (output != null) break;
        }

        if (output == null)
        {
          await message.Channel.SendMessageAsync(NotFoundMessage(message));
        }
        else
        {
          if (output.HasMessage)
          {
            responseMessage = await message.Channel.SendMessageAsync(output.Message);

            if (output.Reactions?.Length > 0)
            {
              await AddReactions(responseMessage, output.Reactions);
            }
          }

          if (output.DeleteInput)
          {
            try
            {
              await message.DeleteAsync();
            }
            catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
            {
              //Whoops, not allowed to do that.
            }
            catch (HttpException e) when (e.HttpCode == HttpStatusCode.NotFound)
            {
              //Message was probably deleted.
            }
          }
        }
      }
      
      if (reactMatch.Success)
      {
        var reactions = reactMatch.Groups["reactions"].Value.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);

        await AddReactions(responseMessage ?? message, reactions);
      }
    }

    protected static async Task AddReactions(IMessage message, IEnumerable<string> reactions)
    {
      foreach (var reaction in reactions)
      {
        try
        {
          await message.AddReactionAsync(
            Emote.TryParse(reaction, out var emote) ? (IEmote) emote : new Emoji(reaction)
          );
        }
        catch (HttpException e) when (e.HttpCode == HttpStatusCode.BadRequest)
        {
          //Bad input; just ignore and move on.
#if DEBUG
          await message.Channel.SendMessageAsync($"Invalid reaction: {reaction}");
#endif
        }
        catch (HttpException e) when (e.HttpCode == HttpStatusCode.NotFound)
        {
          //Message was probably deleted.
        }
      }
    }

    protected virtual Task UserJoined(SocketGuildUser user) => Task.CompletedTask;
    protected virtual Task UserLeft(SocketGuildUser socketGuildUser) => Task.CompletedTask;

    protected virtual string NotFoundMessage(SocketMessage message) =>
        $"I'm afraid I don't understand, {MentionUser(message.Author.Id)}.";

    [Command("help", "help", "Display all commands.")]
    public Output Help(Input input) => new Output(_commands
      .Where(x => x.Description?.Length > 0).Select(x => $@"`{_prefix}{x.Hint}` - {x.Description}")
    );

    [Command("ping!?", "ping", "Test the bot's responsiveness.")]
    public Output Ping(Input input) => new Output($"{input.MentionAuthor} Pong!");

    [Command("react (?<reactions>.+?([, ]{{1,2}}.+?)*)")]
    public Output React(Input input)
    {
      var reactions = input["reactions"].Value.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);

      var getMessageTask = input.Message.Channel.GetMessagesAsync(input.Message, Direction.Before, 1).FlattenAsync();
      getMessageTask.Wait();
      var targetMessage = getMessageTask.Result.SingleOrDefault();

      if (targetMessage != null)
      {
        Task.WaitAll(AddReactions(targetMessage, reactions));
      }

      return new Output { DeleteInput = true };
    }

    [Command("roll(?<num> [1-9]\\d*)?", "roll (#)?", "Roll between 1 and a number, defaulting to 20.")]
    public Output Roll(Input input)
    {
      var num = input["num"].Success ? int.Parse(input["num"].Value) : 20;

      var result = Random.Next(num) + 1;
      var display = result.ToString("N0") + (result == 69 ? " (nice)" : "");

      return new Output($"{input.MentionAuthor}, you rolled a {display} out of a possible {num:N0}.");
    }

    [Command(
      "should (I|we|(?<other>\\S+))(?<verb> .*?)?: ?(?<options>.+?(,?( or)? .+?)*)\\??",
      "should (I|we|...) (verb)?: <1>, ...,( or)? <N>", "Randomly choose one of many options."
    )]
    public Output Choose(Input input)
    {
      var optionsMatch = input["options"];

      if (optionsMatch.Success == false) return null;

      var options = optionsMatch.Value
          .Split(new[] { ",", " or " }, StringSplitOptions.RemoveEmptyEntries);

      options = options.Except(options.Where(string.IsNullOrWhiteSpace)).ToArray();

      var subject = input["other"].Success ? input["other"].Value : "you";

      var verb = input["verb"].Success ? $"{input["verb"].Value.Trim()} " : "";

      var choice = options[Random.Next(options.Length)].Trim();

      return new Output($"I think that {subject} should {verb}{choice}.".Replace(" my", " your"));
    }

    #region Time Commands/Utilities
    [Command(
      "times?((?<offset> (?<c>\\d+) (?<period>months?|days?|hours?|minutes?))+ from now)?( with (?<ex>.+))?",
      "times?(( (number) (months|days|hours|minutes))+ from now)? (with <1>, ..., <N>)?",
      "Display time in different time zones."
    )]
    public Output TimeZones(Input input)
    {
      var baseTime = DateTime.UtcNow;
      var offset = input["offset"];

      if (offset.Success)
      {
        for (var i = 0; i < offset.Captures.Count; i++)
        {
          var c = int.Parse(input["c"].Captures[i].Value);
          var period = input["period"].Captures[i].Value.ToLower();

          if (Regex.Match(period, "months?").Success)
            baseTime = baseTime.AddMonths(c);
          else if (Regex.Match(period, "days?").Success)
            baseTime += TimeSpan.FromDays(c);
          else if (Regex.Match(period, "hours?").Success)
            baseTime += TimeSpan.FromHours(c);
          else if (Regex.Match(period, "minutes?").Success)
            baseTime += TimeSpan.FromMinutes(c);
        }
      }

      var extraMatch = input["ex"];
      var extraZones = extraMatch.Success ? extraMatch.Value.Split(',').ToList() : Enumerable.Empty<string>();
      var timeZones = StandardTimeZones.Union(extraZones).Select(s => s.Trim());

      return new Output(GetTimes(timeZones, baseTime)) { FixedWidth = true };
    }

    [Command(
      "what time is it(?<specific> in (?<ex>.+))?", "what time is it( in <1>, ..., <N>)?",
      "Display the current times either in America or a specific list of time zones."
    )]
    public Output WhatTime(Input input)
    {
      IEnumerable<string> timeZones;
      if (input["specific"].Success)
      {
        timeZones = input["ex"].Value.Split(',').Select(s => s.Trim()).ToList();
      }
      else
      {
        timeZones = StandardTimeZones;
      }

      return new Output(GetTimes(timeZones, DateTime.UtcNow)) { FixedWidth = true };
    }

    private IEnumerable<string> GetTimes(
        IEnumerable<string> timeZones, DateTime baseTime)
    {
      var displayDate = baseTime.ToUniversalTime() > DateTime.UtcNow.AddDays(1);

      var times = new Dictionary<string, DateTime?>();

      foreach (var timeZoneInfo in timeZones.ToDictionary(s => s, GetTimeZoneInfo))
      {
        string label;
        DateTime? dateTime;
        if (timeZoneInfo.Value == null)
        {
          label = timeZoneInfo.Key;
          dateTime = null;
        }
        else
        {
          var offset = timeZoneInfo.Value.BaseUtcOffset;

          if (timeZoneInfo.Value.IsDaylightSavingTime(baseTime))
          {
            offset += TimeSpan.FromHours(1);
            label = timeZoneInfo.Value.DaylightName;
          }
          else
          {
            label = timeZoneInfo.Value.StandardName;
          }

          dateTime = baseTime.Add(offset);
        }

        if (times.ContainsKey(label)) continue;

        times.Add(label, dateTime);
      }

      var max = times.Max(p => p.Key.Length);

      var format = "hh:mm tt";

      if (displayDate) format = "MM/dd/yy " + format + " (ddd)";

      return times.OrderBy(p => p.Value).Select(p =>
        $"{p.Key.PadRight(max)} : {p.Value?.ToString(format) ?? "Not Found".PadLeft(format.Length)}"
      );
    }

    private TimeZoneInfo GetTimeZoneInfo(string s)
    {
      return SystemTimeZones.FirstOrDefault(z => 1 == 0
      || Regex.IsMatch(z.Id, s, IgnoreCase | Compiled)
      || Regex.IsMatch(z.StandardName, s, IgnoreCase | Compiled)
      || Regex.IsMatch(z.DisplayName, s, IgnoreCase | Compiled)
      );
    }

    #endregion
  }
}
