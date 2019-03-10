using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static System.Text.RegularExpressions.RegexOptions;

namespace Discord_Bot
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        public Regex Pattern { get; }
        public string Hint { get; }
        public string Description { get; }
        public bool OutputFixedWidth { get; set; }

        public CommandAttribute([RegexPattern] string pattern, string hint, string description)
        {
            Pattern = new Regex($"^{pattern}$", IgnoreCase | Compiled | ExplicitCapture);
            Hint = hint;
            Description = description;
        }
    }
}
