using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord_Bot;

namespace Smarsh_Bot
{
    /// <summary>
    /// This is the first bot I wrote, for the first server I participated in heavily.
    /// We all lived in different time zones, hence the time-based commands I've left here.
    /// The rest of the commands were fairly simple and generally useful, so I moved them to my base class.
    /// </summary>
    public class Program : Discord_Bot.Base
    {
        protected static readonly string[] StandardTimeZones = { "Pacific Time", "Central Time", "Eastern Time" };
        protected static readonly ReadOnlyCollection<TimeZoneInfo> SystemTimeZones = TimeZoneInfo.GetSystemTimeZones();

        protected override string Prefix => ">";
        protected override Dictionary<string, Func<Command, string>> GetHandlers() => new Dictionary<string, Func<Command, string>>
        {
            { @"""help"" - display all commands.", Help },
            { @"""ping"" - Test the bot's resposiveness.", Ping },
            { @"""roll (number)?"" - Roll between 1 and a number, defaulting to 20.", Roll },
            { @"""should (I|we|...) (verb)?: <1>, ...,( or)? <N>"" - Randomly choose one of many options.", Choose },
            { @"""time(zones)?( (number) (hours|minutes) from)? now (with <1>, ..., <N>)?"" - Display time in different time zones.", TimeZones },
            { @"""what time is it( in <1>, ..., <N>)?"" - Display the current times either in America or a specific list of time zones.", WhatTime }
        };

        public static void Main(string[] args) => Task.WaitAll(new Program().RunAsync());

        protected static string TimeZones(Command command)
        {
            var match = Regex.Match(command.Text, 
                "^time(zones)?(?<offset> (?<c>\\d+) (?<period>hours?|minutes?) from)? now(?: with (?<ex>.+))?", 
                RegexOptions
            );

            if (match.Success == false) return null;

            var now = DateTime.UtcNow;

            if (match.Groups["offset"].Success)
            {
                var c = int.Parse(match.Groups["c"].Value);
                var period = match.Groups["period"].Value;

                if (Regex.Match(period, "hours?").Success)
                    now = now.AddHours(c);
                else if (Regex.Match(period, "minutes?").Success)
                    now = now.AddMinutes(c);
            }

            var extraMatch = match.Groups["ex"];
            var extraZones = extraMatch.Success ? extraMatch.Value.Split(',').ToList() : Enumerable.Empty<string>();

            var messages = new List<string>();
            foreach (var timeZoneInfo in StandardTimeZones
                .Union(extraZones).Select(s => s.Trim())
                .ToDictionary(s => s, s => SystemTimeZones.FirstOrDefault(z => 1 == 0
                ||  Regex.IsMatch(z.Id, s, RegexOptions)
                ||  Regex.IsMatch(z.StandardName, s, RegexOptions)
                ||  Regex.IsMatch(z.DisplayName, s, RegexOptions)
                )))
            {
                string label;
                string display;
                if (timeZoneInfo.Value == null)
                {
                    label = timeZoneInfo.Key;
                    display = "Not Found";
                }
                else
                {
                    var offset = timeZoneInfo.Value.BaseUtcOffset;

                    if (timeZoneInfo.Value.IsDaylightSavingTime(now))
                    {
                        offset = offset.Add(TimeSpan.FromHours(1));
                        label = timeZoneInfo.Value.DaylightName;
                    }
                    else
                    {
                        label = timeZoneInfo.Value.StandardName;
                    }

                    display = now.Add(offset).ToString("h:mm:ss tt");
                }

                messages.Add($"{label}: {display}");
            }

            return string.Join(Environment.NewLine, messages);
        }

        protected static string WhatTime(Command command)
        {
            var match = Regex.Match(command.Text, 
                "^what time is it(?<specific> in (?<ex>.+))?", 
                RegexOptions
            );

            if (match.Success == false) return null;

            var now = DateTime.UtcNow;
            
            IEnumerable<string> timeZones;

            if (match.Groups["specific"].Success)
            {
                timeZones = match.Groups["ex"]
                    .Value.Split(',').Select(s => s.Trim()).ToList();
            }
            else
            {
                timeZones = StandardTimeZones;
            }

            var messages = new List<string>();
            foreach (var timeZoneInfo in timeZones
                .ToDictionary(s => s, s => SystemTimeZones.FirstOrDefault(z => 1 == 0
                ||  Regex.IsMatch(z.Id, s, RegexOptions)
                ||  Regex.IsMatch(z.StandardName, s, RegexOptions)
                ||  Regex.IsMatch(z.DisplayName, s, RegexOptions)
                )))
            {
                string label;
                string display;
                if (timeZoneInfo.Value == null)
                {
                    label = timeZoneInfo.Key;
                    display = "Not Found";
                }
                else
                {
                    var offset = timeZoneInfo.Value.BaseUtcOffset;

                    if (timeZoneInfo.Value.IsDaylightSavingTime(now))
                    {
                        offset = offset.Add(TimeSpan.FromHours(1));
                        label = timeZoneInfo.Value.DaylightName;
                    }
                    else
                    {
                        label = timeZoneInfo.Value.StandardName;
                    }

                    display = now.Add(offset).ToString("h:mm:ss tt");
                }

                messages.Add($"{label}: {display}");
            }

            return string.Join(Environment.NewLine, messages);
        }
    }
}
