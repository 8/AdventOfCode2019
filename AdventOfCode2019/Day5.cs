﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AdventOfCode2019
{
  public class Day5
  {
    enum OpCodes
    {
      Add = 1,
      Multiply = 2,
      Write = 3,
      Print = 4,
      End = 99,
    }

    static int[] StateWith(int[] state, Index index, int value)
    {
      var s = state.ToArray();
      s[index] = value;
      return s;
    }

    static int GetAddress(int[] state, int i)
      => state[i];

    static int GetValue(int[] state, int i)
      => state[GetAddress(state, i)];

    static (int[] State, Index Index) AddMultiply(int[] state, Index i, OpCodes op)
    {
      var v1 = GetValue(state, i.Value + 1);
      var v2 = GetValue(state, i.Value + 2);
      var writeAddress = GetAddress(state, i.Value + 3);

      var result = op switch
      {
        OpCodes.Add => v1 + v2,
        OpCodes.Multiply => v1 * v2,
        _ => throw new InvalidOperationException($"unknown OpCode '{op}'"),
      };

      return (State: StateWith(state, writeAddress, result), Index: new Index(i.Value + 4));
    }

    static (int[] State, Index index) Print(int[] state, Index i, Action<int> printCallback)
    {
      var param = GetValue(state, i.Value + 1);
      printCallback(param);
      return (state, i.Value + 2);
    }

    static (int[] State, Index index) Read(int[] state, Index i, Func<int> readInputCallback)
    {
      var param = GetAddress(state, i.Value + 1);
      var value = readInputCallback();
      return (StateWith(state, new Index(param), value), i.Value + 2);
    }

    static (int[] State, Index Index) Step(int[] state, Index i, Action<int> printCallback, Func<int> readInputCallback)
    {
      OpCodes op = (OpCodes)state[i];

      return op switch
      {
        OpCodes.End => (state, Index.End),
        OpCodes o when o == OpCodes.Add || o == OpCodes.Multiply => AddMultiply(state, i, o),
        OpCodes.Print => Print(state, i, printCallback),
        OpCodes.Write => Read(state, i, readInputCallback),
        _ => throw new InvalidOperationException($"unknown OpCode '{op}'"),
      };
    }

    static int[] Run(int[] program, Action<int> printCallback, Func<int> readInputCallback)
    {
      //List<int[]> states = new List<int[]>();
      var state = program.ToArray();
      Index i = 0;

      while (true)
      {
        //states.Add(state);
        (state, i) = Step(state, i, printCallback, readInputCallback);

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
      builder.Append($"{state[state.Length - 1]}");
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
    public void Day5_Run(int[] program, int[] resultState)
    {
      Console.WriteLine(FormatState(program));

      /* act */
      var state = Run(program, n => Console.WriteLine(n), () => 0);

      Console.WriteLine(FormatState(state));

      /* assert */
      state.Should().BeEquivalentTo(resultState);
    }

    /* extended for Day5 */

    [Fact]
    public void Day5_Run_Print()
    {
      var program = new int[] { 4, 2, 99};
      var output = new List<int>();
      var state = Run(program, n => output.Add(n), () => 0);

      output.Count.Should().Be(1);
      output[0].Should().Be(99);
    }

    [Fact]
    public void Day5_Run_ReadInput_Print()
    {
      /* arrange */
      var program = new int[] { 3, 0, 4, 0, 99 };
      var output = new List<int>();

      /* act */
      var state = Run(program, n => output.Add(n), () => 33);

      /* assert */
      output.Count.Should().Be(1);
      output[0].Should().Be(33);
    }
 
  }
}