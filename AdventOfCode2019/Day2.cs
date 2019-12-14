using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AdventOfCode2019
{
  public class Day2
  {
    enum OpCodes
    {
      Add      = 1,
      Multiply = 2,
      End      = 99,
    }

    static int[] StateWith(int[] state, Index index, int value)
    {
      var s = state.ToArray();
      s[index] = value;
      return s;
    }

    static (int[] State, Index Index) Step(int[] state, Index i)
    {
      static int GetAddress(int[] state, int i)
        => state[i];

      static int GetValue(int[] state, int i)
        => state[GetAddress(state, i)];

      static (int[] State, Index Index) GetResult(int[] state, Index i, OpCodes op)
      {
        var v1 = GetValue(state, i.Value + 1);
        var v2 = GetValue(state, i.Value + 2);
        var outIndex = GetAddress(state, i.Value + 3);
        var result = op switch
        {
          OpCodes.Add => v1 + v2,
          OpCodes.Multiply => v1 * v2,
          _ => throw new InvalidOperationException($"unknown OpCode '{op}'"),
        };

        return (State: StateWith(state, outIndex, result), Index: new Index(i.Value + 4));
      }

      OpCodes op = (OpCodes)state[i];

      return op switch
      {
        OpCodes.End => (state, Index.End),
        var o => GetResult(state, i, o),
      };
    }

    int[] Run(int[] program)
    {
      //List<int[]> states = new List<int[]>();
      var state = program.ToArray();
      Index i = 0;

      while (true)
      {
        //states.Add(state);
        (state, i) = Step(state, i);

        if (i.Value == Index.End.Value)
          break;
      }

      return state;
    }

    string FormatState(int[] state)
    {
      var builder = new StringBuilder();
      for (int i = 0; i < state.Length - 1; i++)
        builder.Append($"{state[i]},");
      builder.Append($"{state[state.Length-1]}");
      return builder.ToString();
    }

    [Theory,
      InlineData(new int[] { 1, 9, 10, 3, 2, 3, 11, 0, 99, 30, 40, 50 },
                 new int[] { 3500, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50 }),
      InlineData(new int[] { 1, 0, 0, 0, 99 }, new int[] { 2, 0, 0, 0, 99 }),
      InlineData(new int[] { 2, 3, 0, 3, 99 }, new int[] { 2, 3, 0, 6, 99 }),
      InlineData(new int[] { 2, 4, 4, 5, 99, 0 }, new int[] { 2, 4, 4, 5, 99, 9801 }),
      InlineData(new int[] { 1, 1, 1, 4, 99, 5, 6, 0, 99 }, new int[] { 30, 1, 1, 4, 2, 5, 6, 0, 99 }),
      ]
    public void Day2_Run(int[] program, int[] resultState)
    {
      Console.WriteLine(FormatState(program));

      /* act */
      var state = Run(program);

      Console.WriteLine(FormatState(state));

      /* assert */
      state.Should().BeEquivalentTo(resultState);
    }

    static int[] GetInput()
      => new int[] { 1,0,0,3,1,1,2,3,1,3,4,3,1,5,0,3,2,10,1,19,1,6,19,23,1,23,13,27,2,6,27,31,1,5,31,35,2,10,35,39,1,6,39,43,1,13,43,47,2,47,6,51,1,51,5,55,1,55,6,59,2,59,10,63,1,63,6,67,2,67,10,71,1,71,9,75,2,75,10,79,1,79,5,83,2,10,83,87,1,87,6,91,2,9,91,95,1,95,5,99,1,5,99,103,1,103,10,107,1,9,107,111,1,6,111,115,1,115,5,119,1,10,119,123,2,6,123,127,2,127,6,131,1,131,2,135,1,10,135,0,99,2,0,14,0 };

    [Fact]
    public void Day2_Test()
    {
      var program = GetInput();
      program[1] = 12;
      program[2] = 2;

      var state = this.Run(program);

      Console.WriteLine(FormatState(state));
    }
  }
}
