using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace AdventOfCode2019
{
  public class Day4
  {
    /* it is a 6-digit number */
    bool IsItA6DigitNumber(int number)
      => number.ToString().Length == 6;

    [Theory,
      InlineData(111111, true),
      InlineData(1, false),
      InlineData(1111112, false),
      ]
    public void Day4_IsItA6DigitNumber(int number, bool expectedResult)
    {
      var result = IsItA6DigitNumber(number);

      result.Should().Be(expectedResult);
    }

    IEnumerable<int> GetNumbers(int start, int end)
    {
      for (int i = start; i <= end; i++)
        yield return i;
    }

    /* value within the range */
    IEnumerable<int> GetNumbers()
      => GetNumbers(124075, 580769);

    /* two adjacent digits are same */
    bool AreTwoAdjacentDigitsTheSame(int number)
    {
      string s = number.ToString();

      bool same = false;
      for (int i = 1; i < s.Length; i++)
      {
        if (s[i] == s[i - 1])
        {
          same = true;
          break;
        }
      }

      return same;
    }

    [Theory,
      InlineData(1, false),
      InlineData(22, true),
      InlineData(123456, false),
      InlineData(1233, true),
      InlineData(123455555, true)
      ]
    public void Day4_AreTwoAdjacentDigitsTheSame(int number, bool expectedResult)
    {
      var result = AreTwoAdjacentDigitsTheSame(number);

      result.Should().Be(expectedResult);
    }

    /* going from left to right, the digits never decrease, they only increase or stay the same */
    bool DoTheDigitsNeverDecrease(int number)
    {
      int[] digits = number.ToString().Select(c => int.Parse(new string(c, 1))).ToArray();

      bool neverDecrease = true;
      for (int i = 0; i < digits.Length; i++)
        if (i > 0 && digits[i-1] > digits[i])
        {
          neverDecrease = false;
          break;
        }

      return neverDecrease;
    }

    [Theory,
      InlineData(1, true),
      InlineData(21, false),
      InlineData(11, true),
      InlineData(12345678, true),
      InlineData(1234467, true),
      InlineData(12345467, false),
      ]
    public void Day4_DoTheDigitsNeverDecrease(int number, bool expectedResult)
    {
      var result = DoTheDigitsNeverDecrease(number);

      result.Should().Be(expectedResult);
    }

    /* how many different passwords meet the criteria? */
    IEnumerable<int> GetValidNumbers()
      => GetNumbers().Where(n => IsItA6DigitNumber(n) && AreTwoAdjacentDigitsTheSame(n) && DoTheDigitsNeverDecrease(n));

    [Fact]
    public void Day4_Count_of_Valid_Numbers()
    {
      var result = GetValidNumbers().Count();

      Console.WriteLine($"result: {result}");
    }

    /* PART 2 */

    /* two adjacent digits are same */
    bool Are2AdjacentDigitsTheSameButNotPartOfALargerGroup(int number)
    {
      string s = number.ToString();

      bool same = false;
      for (int i = 1; i < s.Length; i++)
      {
        if (s[i] == s[i - 1] /* are two adjacent? */
            && (i == s.Length -1 || s[i] != s[i+1] /* and either at the end or the following is different */)
            && (i < 2 || s[i] != s[i-2])           /* and either at the start of the previous is different */
            )
        {
          same = true;
          break;
        }
      }

      return same;
    }

    [Theory,
      InlineData(1, false),
      InlineData(22, true),
      InlineData(123456, false),
      InlineData(112233, true),
      InlineData(123444, false),
      InlineData(111122, true),
      ]
    public void Are2AdjacentDigitsTheSameButNotPartOfALargerGroup_Test(int number, bool expectedResult)
    {
      var result = Are2AdjacentDigitsTheSameButNotPartOfALargerGroup(number);

      result.Should().Be(expectedResult);
    }

    IEnumerable<int> GetValidNumbersPart2()
      => GetNumbers().Where(n => IsItA6DigitNumber(n) && Are2AdjacentDigitsTheSameButNotPartOfALargerGroup(n) && DoTheDigitsNeverDecrease(n));

    [Fact]
    public void Day4_Part2()
    {
      var result = GetValidNumbersPart2().Count();

      Console.WriteLine($"result: {result}");
    }
  }
}
