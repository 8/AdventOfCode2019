using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace AdventOfCode2019
{
  public class Day6
  {
    static (string Parent, string Child) ParseOrbit(string s)
      => s.Split(')') switch { var parts => (parts[0], parts[1]) };

    static IEnumerable<(string Parent, string Child)> ParseOrbits(string s)
    {
      string line;
      using (var sr = new StringReader(s))
        while ((line = sr.ReadLine()) != null)
          yield return ParseOrbit(line);
    }

    static Dictionary<string, string[]> GetNodes(IEnumerable<(string Parent, string Child)> items)
      => items.GroupBy(o => o.Parent)
      .ToDictionary(g => g.Key, g => g.Select(o => o.Child).ToArray());

    static int CountDescendants(Dictionary<string, string[]> items, string parent, int level = 0)
    {
      if (!items.TryGetValue(parent, out string[] children))
        return level;
      else
        return level + children.Sum(c => CountDescendants(items, c, level + 1));
    }

    static int CountDescendants(string input)
      => ParseOrbits(input) 
        switch { var o => GetNodes(o) 
          switch { var items => CountDescendants(items, "COM", 0) } };

    [Theory,
      InlineData(@"COM)B
B)C
C)D
D)E
E)F
B)G
G)H
D)I
E)J
J)K
K)L", 42),
      InlineData("COM)B", 1),
      InlineData("COM)B\nB)A", 3),
      InlineData("COM)A\nA)B\nB)C", 6),
      InlineData("COM)B\nCOM)A", 2),
      InlineData("COM)E\nE)F\nE)J\nJ)K\nK)L", 4 + 3 + 2 + 1 + 2)
      ]
    public void Day6_CountDescendents(string input, int expectedCount)
    {
      var result = CountDescendants(input);
      Console.WriteLine(result);
      result.Should().Be(expectedCount);
    }

    [Theory,
      InlineData("B)C", "B", "C"),
      InlineData("COM)B", "COM", "B"),
      InlineData("K)L", "K", "L"),
      ]
    public void Day6_ParseOrbit(string s, string expectedParent, string expectedChild)
    {
      var result = ParseOrbit(s);

      result.Parent.Should().Be(expectedParent);
      result.Child.Should().Be(expectedChild);
    }

    static string GetInput()
      => File.ReadAllText("Day6.Data.txt");

    [Fact]
    public void Day6_Part1()
    {
      string input = GetInput();

      int result = CountDescendants(input);

      Console.WriteLine(result);
    }

  }
}
