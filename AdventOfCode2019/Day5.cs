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
      JumpIfTrue = 5,
      JumpIfFalse = 6,
      LessThan = 7,
      Equals = 8,
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

    static (int[] State, Index index) JumpIfTrue(int[] state, Index i, Op op)
    {
      var test = GetParameter(state, i, 0, op);
      var ip = GetParameter(state, i, 1, op);
      return (State: state, test != 0 ? ip : i.Value + 3);
    }

    static (int[] State, Index index) JumpIfFalse(int[] state, Index i, Op op)
    {
      var test = GetParameter(state, i, 0, op);
      var ip = GetParameter(state, i, 1, op);
      return (State: state, test == 0 ? ip : i.Value + 3);
    }

    static (int[] State, Index index) LessThan(int[] state, Index i, Op op)
    {
      var v1 = GetParameter(state, i, 0, op);
      var v2 = GetParameter(state, i, 1, op);
      var address = GetAddress(state, i.Value + 3);
      return (State: StateWith(state, address, v1 < v2 ? 1 : 0), i.Value + 4);
    }

    static (int[] State, Index index) Equals(int[] state, Index i, Op op)
    {
      var v1 = GetParameter(state, i, 0, op);
      var v2 = GetParameter(state, i, 1, op);
      var v3 = GetAddress(state, i.Value + 3);
      return (State: StateWith(state, v3, v1 == v2 ? 1 : 0), i.Value + 4);
    }

    public enum ParameterMode { Value = 0, Address = 1 };

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
        OpCodes.JumpIfTrue => JumpIfTrue(state, i, op),
        OpCodes.JumpIfFalse => JumpIfFalse(state, i, op),
        OpCodes.LessThan => LessThan(state, i, op),
        OpCodes.Equals => Equals(state, i, op),
        _ => throw new InvalidOperationException($"unknown OpCode '{op.Opcode}'"),
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

    int[] GetInput() => new int[] { 3, 225, 1, 225, 6, 6, 1100, 1, 238, 225, 104, 0, 2, 218, 57, 224, 101, -3828, 224, 224, 4, 224, 102, 8, 223, 223, 1001, 224, 2, 224, 1, 223, 224, 223, 1102, 26, 25, 224, 1001, 224, -650, 224, 4, 224, 1002, 223, 8, 223, 101, 7, 224, 224, 1, 223, 224, 223, 1102, 44, 37, 225, 1102, 51, 26, 225, 1102, 70, 94, 225, 1002, 188, 7, 224, 1001, 224, -70, 224, 4, 224, 1002, 223, 8, 223, 1001, 224, 1, 224, 1, 223, 224, 223, 1101, 86, 70, 225, 1101, 80, 25, 224, 101, -105, 224, 224, 4, 224, 102, 8, 223, 223, 101, 1, 224, 224, 1, 224, 223, 223, 101, 6, 91, 224, 1001, 224, -92, 224, 4, 224, 102, 8, 223, 223, 101, 6, 224, 224, 1, 224, 223, 223, 1102, 61, 60, 225, 1001, 139, 81, 224, 101, -142, 224, 224, 4, 224, 102, 8, 223, 223, 101, 1, 224, 224, 1, 223, 224, 223, 102, 40, 65, 224, 1001, 224, -2800, 224, 4, 224, 1002, 223, 8, 223, 1001, 224, 3, 224, 1, 224, 223, 223, 1102, 72, 10, 225, 1101, 71, 21, 225, 1, 62, 192, 224, 1001, 224, -47, 224, 4, 224, 1002, 223, 8, 223, 101, 7, 224, 224, 1, 224, 223, 223, 1101, 76, 87, 225, 4, 223, 99, 0, 0, 0, 677, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1105, 0, 99999, 1105, 227, 247, 1105, 1, 99999, 1005, 227, 99999, 1005, 0, 256, 1105, 1, 99999, 1106, 227, 99999, 1106, 0, 265, 1105, 1, 99999, 1006, 0, 99999, 1006, 227, 274, 1105, 1, 99999, 1105, 1, 280, 1105, 1, 99999, 1, 225, 225, 225, 1101, 294, 0, 0, 105, 1, 0, 1105, 1, 99999, 1106, 0, 300, 1105, 1, 99999, 1, 225, 225, 225, 1101, 314, 0, 0, 106, 0, 0, 1105, 1, 99999, 108, 226, 677, 224, 102, 2, 223, 223, 1005, 224, 329, 1001, 223, 1, 223, 1107, 677, 226, 224, 102, 2, 223, 223, 1006, 224, 344, 1001, 223, 1, 223, 7, 226, 677, 224, 1002, 223, 2, 223, 1005, 224, 359, 101, 1, 223, 223, 1007, 226, 226, 224, 102, 2, 223, 223, 1005, 224, 374, 101, 1, 223, 223, 108, 677, 677, 224, 102, 2, 223, 223, 1006, 224, 389, 1001, 223, 1, 223, 107, 677, 226, 224, 102, 2, 223, 223, 1006, 224, 404, 101, 1, 223, 223, 1108, 677, 226, 224, 102, 2, 223, 223, 1006, 224, 419, 1001, 223, 1, 223, 1107, 677, 677, 224, 1002, 223, 2, 223, 1006, 224, 434, 101, 1, 223, 223, 1007, 677, 677, 224, 102, 2, 223, 223, 1006, 224, 449, 1001, 223, 1, 223, 1108, 226, 677, 224, 1002, 223, 2, 223, 1006, 224, 464, 101, 1, 223, 223, 7, 677, 226, 224, 102, 2, 223, 223, 1006, 224, 479, 101, 1, 223, 223, 1008, 226, 226, 224, 102, 2, 223, 223, 1006, 224, 494, 101, 1, 223, 223, 1008, 226, 677, 224, 1002, 223, 2, 223, 1005, 224, 509, 1001, 223, 1, 223, 1007, 677, 226, 224, 102, 2, 223, 223, 1005, 224, 524, 1001, 223, 1, 223, 8, 226, 226, 224, 102, 2, 223, 223, 1006, 224, 539, 101, 1, 223, 223, 1108, 226, 226, 224, 1002, 223, 2, 223, 1006, 224, 554, 101, 1, 223, 223, 107, 226, 226, 224, 1002, 223, 2, 223, 1005, 224, 569, 1001, 223, 1, 223, 7, 226, 226, 224, 102, 2, 223, 223, 1005, 224, 584, 101, 1, 223, 223, 1008, 677, 677, 224, 1002, 223, 2, 223, 1006, 224, 599, 1001, 223, 1, 223, 8, 226, 677, 224, 1002, 223, 2, 223, 1006, 224, 614, 1001, 223, 1, 223, 108, 226, 226, 224, 1002, 223, 2, 223, 1006, 224, 629, 101, 1, 223, 223, 107, 677, 677, 224, 102, 2, 223, 223, 1005, 224, 644, 1001, 223, 1, 223, 8, 677, 226, 224, 1002, 223, 2, 223, 1005, 224, 659, 1001, 223, 1, 223, 1107, 226, 677, 224, 102, 2, 223, 223, 1005, 224, 674, 1001, 223, 1, 223, 4, 223, 99, 226 };

    [Fact]
    public void Day5_Part1()
    {
      var program = GetInput();
      var builder = new StringBuilder();
      var state = Run(program, n => builder.Append($"Output: {n}\n"), () => 1);

      Console.WriteLine(builder.ToString());
    }

    [Fact]
    public void Day5_Part2()
    {
      var program = GetInput();
      var builder = new StringBuilder();
      var state = Run(program, n => builder.Append($"Output: {n}\n"), () => 5);

      Console.WriteLine(builder.ToString());
    }
 
  }
}
