using System;
using System.Collections.Generic;

namespace Discord_Bot
{
  public class Output
  {
    private readonly string _message;
    public bool HasMessage => string.IsNullOrWhiteSpace(_message) == false;
    public string Message => HasMessage ? FixedWidth ? $"```{_message}```" : _message : null;

    public bool FixedWidth { get; set; }
    public string[] Reactions { get; set; } = new string[0];
    public bool DeleteInput { get; set; }

    public Output(string s = null)
    {
      _message = s;
    }

    public Output(params string[] s) : this(string.Join(Environment.NewLine, s)) { }

    public Output(IEnumerable<string> s) : this(string.Join(Environment.NewLine, s)) { }
  }
}
