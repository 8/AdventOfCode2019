using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace AdventOfCode2019
{
  public class Day1
  {
    static double GetFuel(double mass)
      => Math.Floor(mass / 3d) - 2;

    [Theory,
      InlineData(12, 2),
      InlineData(14, 2),
      InlineData(1969, 654),
      InlineData(100756, 33583)
      ]
    public void Day1_FuelForMass(double mass, double expectedFuel)
    {
      var fuel = GetFuel(mass);

      fuel.Should().Be(expectedFuel);
    }

    readonly double[] _masses = new double[]
      {
        75592
,56081
,141375
,103651
,132375
,90584
,94148
,85029
,95082
,148499
,108192
,97739
,60599
,140308
,125171
,129160
,143118
,98762
,103907
,115389
,127835
,57917
,72980
,88747
,86595
,130407
,116862
,84652
,112817
,136922
,51900
,76677
,146244
,121897
,99310
,136486
,84665
,117344
,88992
,83929
,74820
,56651
,74001
,88636
,51232
,57878
,114559
,58879
,145519
,83727
,111774
,146256
,123479
,86955
,64027
,59812
,59211
,85835
,58084
,113676
,119161
,106368
,137358
,85290
,81131
,124857
,51759
,82977
,138957
,146216
,147807
,72265
,60332
,136741
,110215
,89293
,148703
,73152
,93080
,140220
,68511
,77397
,51934
,100243
,92442
,135254
,98873
,51105
,118755
,79155
,89249
,137430
,142807
,86334
,117266
,149484
,89284
,63361
,52269
,111666
      };

    [Fact]
    public void Day1_Sum()
    {
      var sum = this._masses.Select(m => GetFuel(m)).Sum();

      Console.WriteLine($"sum of all masses: {sum}");
    }

    static double GetFuelRec(double mass, double acc)
    {
      var m = GetFuel(mass);
      if (m > 0)
        return GetFuelRec(m, acc + m);
      else
        return acc;
    }

    [Theory,
      InlineData(14, 2),
      InlineData(100756, 50346),
      ]
    public void Day1_FuelForMassRec(double mass, double expectedFuel)
    {
      var fuel = GetFuelRec(mass, 0);

      fuel.Should().Be(expectedFuel);
    }

    [Fact]
    public void Day1_SumRec()
    {
      var sum = this._masses.Select(m => GetFuelRec(m, 0)).Sum();

      Console.WriteLine($"sum of all masses taking into account recursive fuel: {sum}");
    }
  }
}
