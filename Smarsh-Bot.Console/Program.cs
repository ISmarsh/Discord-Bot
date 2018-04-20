using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord_Bot;
using static System.Text.RegularExpressions.RegexOptions;

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

        protected override string Prefix { get; } = ">";

        public static void Main(string[] args) => Task.WaitAll(new Program().RunAsync());

        [Command(
            "time(zones)?(?<offset> (?<c>\\d+) (?<period>hours?|minutes?) from)? now(?: with (?<ex>.+))?",
            "time(zones)?( (number) (hours|minutes) from)? now (with <1>, ..., <N>)?", 
            "Display time in different time zones."
        )]
        public static string TimeZones(Command command)
        {
            var baseTime = DateTime.UtcNow;

            if (command["offset"].Success)
            {
                var c = int.Parse(command["c"].Value);
                var period = command["period"].Value.ToLower();

                if (Regex.Match(period, "hours?").Success)
                    baseTime = baseTime.AddHours(c);
                else if (Regex.Match(period, "minutes?").Success)
                    baseTime = baseTime.AddMinutes(c);
            }

            var extraMatch = command["ex"];
            var extraZones = extraMatch.Success ? extraMatch.Value.Split(',').ToList() : Enumerable.Empty<string>();
            var timeZones = StandardTimeZones.Union(extraZones).Select(s => s.Trim());

            return string.Join(Environment.NewLine, GetTimes(timeZones, baseTime));
        }

        [Command(
            "what time is it(?<specific> in (?<ex>.+))?", "what time is it( in <1>, ..., <N>)?", 
            "Display the current times either in America or a specific list of time zones."
        )]
        public static string WhatTime(Command command)
        {
            IEnumerable<string> timeZones;
            if (command["specific"].Success)
            {
                timeZones = command["ex"].Value.Split(',').Select(s => s.Trim()).ToList();
            }
            else
            {
                timeZones = StandardTimeZones;
            }

            return string.Join(Environment.NewLine, GetTimes(timeZones, DateTime.UtcNow));
        }

        private static IEnumerable<string> GetTimes(IEnumerable<string> timeZones, DateTime baseTime)
        {
            foreach (var timeZoneInfo in timeZones
                .ToDictionary(s => s, s => SystemTimeZones.FirstOrDefault(z => 1 == 0
                || Regex.IsMatch(z.Id, s, IgnoreCase | Compiled)
                || Regex.IsMatch(z.StandardName, s, IgnoreCase | Compiled)
                || Regex.IsMatch(z.DisplayName, s, IgnoreCase | Compiled)
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

                    if (timeZoneInfo.Value.IsDaylightSavingTime(baseTime))
                    {
                        offset = offset.Add(TimeSpan.FromHours(1));
                        label = timeZoneInfo.Value.DaylightName;
                    }
                    else
                    {
                        label = timeZoneInfo.Value.StandardName;
                    }

                    display = baseTime.Add(offset).ToString("h:mm:ss tt");
                }

                yield return $"{label}: {display}";
            }
        }
    }
}
