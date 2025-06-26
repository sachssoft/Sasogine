using sachssoft.Sasogine.Utils;
using System;

public sealed class SystemRandomGenerator : IRandomGenerator
{
    public int Minimum { get; init; }
    public int Maximum { get; init; }
    public int Seed { get; init; }

    public int Generate()
    {
        var _random = new Random(Seed);
        return _random.Next(Minimum, Maximum + 1);
    }
}
