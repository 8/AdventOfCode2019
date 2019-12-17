using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AdventOfCode2019
{
  public class Day5
  {
    public enum OpCodes
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

    static int GetParameter(int[] state, Index i, int parameterIndex, Op op)
    {
      var mode = GetParameterMode(op, parameterIndex);

      return mode switch
      {
        ParameterMode.Address => GetAddress(state, i.Value + 1 + parameterIndex),
        ParameterMode.Value => GetValue(state, i.Value + 1 + parameterIndex),
        _ => throw new ArgumentOutOfRangeException(nameof(mode)),
      };
    }

    static (int[] State, Index Index) AddMultiply(int[] state, Index i, Op op)
    {
      var v1 = GetParameter(state, i, 0, op);
      var v2 = GetParameter(state, i, 1, op);
      var writeAddress = GetAddress(state, i.Value + 3);

      var result = op.Opcode switch
      {
        OpCodes.Add => v1 + v2,
        OpCodes.Multiply => v1 * v2,
        _ => throw new InvalidOperationException($"unknown OpCode '{op}'"),
      };

      return (State: StateWith(state, writeAddress, result), Index: new Index(i.Value + 4));
    }

    static (int[] State, Index index) Print(int[] state, Index i, Action<int> printCallback, Op op)
    {
      var param = GetParameter(state, i, 0, op);
      printCallback(param);
      return (state, i.Value + 2);
    }

    static (int[] State, Index index) Read(int[] state, Index i, Func<int> readInputCallback)
    {
      var param = GetAddress(state, i.Value + 1);
      var value = readInputCallback();
      return (StateWith(state, new Index(param), value), i.Value + 2);
    }

    public enum ParameterMode { Value = 0, Address = 1};

    class Op
    {
      public OpCodes Opcode { get; set; }
      public ParameterMode[] ParameterModes { get; set; }
    }

    static OpCodes GetOpCode(int n)
      => (OpCodes)int.Parse(n.ToString("00")[^2..]);

    [Theory,
      InlineData(1, OpCodes.Add),
      InlineData(101, OpCodes.Add),
      InlineData(123231202, OpCodes.Multiply),
      ]
    public void Day5_GetOpCode(int n, OpCodes expectedOpCode)
    {
      var result = GetOpCode(n);

      result.Should().Be(expectedOpCode);
    }

    static ParameterMode[] GetParameterModes(int n)
      => n.ToString("00")[..^2]
        .Select(c => (ParameterMode)int.Parse(new string(c, 1)))
        .Reverse()
        .ToArray();

    [Theory,
      InlineData(1002, new ParameterMode[] { ParameterMode.Address, ParameterMode.Value })]
    public void Day5_GetParameterModes(int n, ParameterMode[] expectedParameterModes)
    {
      /* act */
      var result = GetParameterModes(n);

      /* assert */
      result.Should().BeEquivalentTo(expectedParameterModes);
    }

    static Op GetOp(int n)
      => new Op
      {
        Opcode = GetOpCode(n),
        ParameterModes = GetParameterModes(n),
      };

    static ParameterMode GetParameterMode(Op op, int i)
      => op.ParameterModes.Length > i ? op.ParameterModes[i] : ParameterMode.Value;
   
    static (int[] State, Index Index) Step(int[] state, Index i, Action<int> printCallback, Func<int> readInputCallback)
    {
      var op = GetOp(state[i]);

      return op.Opcode switch
      {
        OpCodes.End => (state, Index.End),
        OpCodes.Add => AddMultiply(state, i, op),
        OpCodes.Multiply => AddMultiply(state, i, op),
        OpCodes.Print => Print(state, i, printCallback, op),
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

    [Fact]
    public void Day5_Run_Test()
    {
      var program = new int[] { 1101, 100, -1, 4, 0 };
      var state = Run(program, n => throw new Exception(), () => throw new Exception());
      Console.WriteLine(FormatState(state));
    }

    [Fact]
    public void Day5_Part1()
    {
      var program = new int[] { };

      StringBuilder stringBuilder = new StringBuilder();
      var state = Run(program, n => stringBuilder.Append(n), () => 1);

      Console.WriteLine(stringBuilder.ToString());
    }
 
  }
}
