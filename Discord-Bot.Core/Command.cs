using System;
using System.Text.RegularExpressions;

namespace Discord_Bot
{
  public class Command
  {
    public Command(CommandAttribute attribute, Func<Input, Output> func)
    {
      Pattern = attribute.Pattern;
      Hint = attribute.Hint;
      Description = attribute.Description;
      Func = func;
    }

    public Regex Pattern { get; }
    public string Hint { get; }
    public string Description { get; }
    public Func<Input, Output> Func { get; }
  }
}
