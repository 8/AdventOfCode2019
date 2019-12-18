using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace AdventOfCode2019
{
  public class Day6
  {
    static (string Parent, string Child) ParseOrbit(string s)
      => s.Split(')') switch { var parts => (parts[0], parts[1]) };

    static IEnumerable<(string Parent, string Child)> GetOrbits(string s)
    {
      string line;
      using (var sr = new StringReader(s))
        while ((line = sr.ReadLine()) != null)
          yield return ParseOrbit(line);
    }

    static Dictionary<string, string[]> GetNodes(IEnumerable<(string Parent, string Child)> items)
      => items.GroupBy(o => o.Parent)
      .ToDictionary(g => g.Key, g => g.Select(o => o.Child).ToArray());

    static int GetCountDescendants(Dictionary<string, string[]> items, string parent, int level = 0)
    {
      if (!items.TryGetValue(parent, out string[] children))
        return level;
      else
        return level + children.Sum(c => GetCountDescendants(items, c, level + 1));
    }

    static int GetCountDescendants(string input)
      => GetOrbits(input) 
        switch { var o => GetNodes(o) 
          switch { var items => GetCountDescendants(items, "COM", 0) } };

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
      var result = GetCountDescendants(input);
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

      int result = GetCountDescendants(input);

      Console.WriteLine(result);
    }

    /* part 2 */

    int GetHopCount(string input, string n1, string n2)
    {
      var parent2Children = GetOrbits(input).GroupBy(o => o.Parent).ToDictionary(g => g.Key, g => g.Select(o => o.Child).ToArray());
      var child2Parent = GetOrbits(input).ToDictionary(o => o.Child, o => o.Parent);
      return Search(parent2Children, child2Parent, null, child2Parent[n1], n2, 0);
    }

    int Search(
      Dictionary<string, string[]> parent2Children,
      Dictionary<string, string> child2Parent,
      string sourceNode,
      string currentNode,
      string searchedNode,
      int distance
      )
    {
      string parent;
      if (!child2Parent.TryGetValue(currentNode, out parent))
        parent = null;

      /* did we find the target? */
      if (currentNode == child2Parent[searchedNode])
        return distance;

      /* if we didn't come from the parent, then look into the parent */
      if (parent != null && parent != sourceNode)
      {
        var result = Search(parent2Children, child2Parent, currentNode, parent, searchedNode, distance + 1);
        if (result != -1)
          return result;
      }

      if (parent2Children.TryGetValue(currentNode, out string[] children))
      {
        foreach (var child in children)
          if (child != sourceNode)
          {
            var result = Search(parent2Children, child2Parent, currentNode, child, searchedNode, distance + 1);
            if (result != -1)
              return result;
          }
      }

      return -1;
    }

    [Theory,
      InlineData("COM)B\nB)C\nC)D\nD)E\nE)F\nB)G\nG)H\nD)I\nE)J\nJ)K\nK)L\nK)YOU\nI)SAN", 4),
      InlineData("COM)YOU\nCOM)SAN", 0),
      InlineData("COM)YOU\nCOM)A\nA)SAN", 1),
      ]
    public void Day6_GetHops(string s, int expectedHopCount)
    {
      var orbits = GetOrbits(s);
      var result = GetHopCount(s, "YOU", "SAN");

      result.Should().Be(expectedHopCount);
    }

    [Fact]
    public void Day6_Part2()
    {
      var input = GetInput();
      var result = GetHopCount(input, "YOU", "SAN");
      Console.WriteLine(result);
    }
  }
}
