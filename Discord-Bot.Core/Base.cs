using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
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
    private readonly Regex _prefixRegex;
    private readonly IConfiguration _configuration;
    private readonly List<Command> _commands;

    protected Base(string prefix)
    {
      _prefixRegex = new Regex($"^{_prefix = prefix} ?");

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
      var prefixMatch = _prefixRegex.Match(message.Content);

      if (prefixMatch.Success == false) return;

      var commandText = message.Content.Substring(prefixMatch.Value.Length).Trim();

      var response = _commands.Select(c =>
      {
        var match = c.Pattern.Match(commandText);

        if (match.Success == false) return null;

        return c.Func(new Input(message, match))?.ToString();
      }).FirstOrDefault(s => s != null);

      await message.Channel.SendMessageAsync(response ?? NotFoundMessage(message));
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
