using System;
using System.Collections.Generic;

namespace Discord_Bot
{
  public class Output
  {
    private readonly string _s;
    public bool FixedWidth { get; set; }

    public Output(string s)
    {
      _s = s;
    }

    public Output(params string[] s) : this(string.Join(Environment.NewLine, s)) { }

    public Output(IEnumerable<string> s) : this(string.Join(Environment.NewLine, s)) { }

    public override string ToString() => 
      string.IsNullOrWhiteSpace(_s) ? null : FixedWidth ? $"```{_s}```" : _s;
  }
}
