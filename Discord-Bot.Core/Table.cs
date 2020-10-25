using System;
using System.Collections.Immutable;

namespace Discord_Bot
{
  public class Table<T>
  {
    private readonly Random _random = new Random();
    private readonly ImmutableList<T> _rows;

    public Table(params T[] rows)
    {
      _rows = rows.ToImmutableList();
    }

    public Table(params (Range range, T row)[] rows)
    {
      var totalLength = rows[^1].range.End.Value;
      var array = new T[totalLength];

      foreach (var (range, row) in rows)
      {
        var (offset, length) = range.GetOffsetAndLength(totalLength);

        for (var i = offset - 1; i < offset + length; i++)
        {
          array[i] = row;
        }
      }

      _rows = array.ToImmutableList();
    }

    public T this[int index] => _rows[index];

    public T Roll(out int roll) => this[roll = _random.Next(_rows.Count)];
    public T Roll() => this.Roll(out _);
  }
}
