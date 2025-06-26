using System;
using System.Linq;

namespace sachssoft.Sasogine.Utils;

public static class RandomUtil
{
    public static int Generate(int minimum, int maximum)
    {
        return Generate<SystemRandomGenerator>(minimum, maximum);
    }

    public static int Generate(int minimum, int maximum, int seed)
    {
        return Generate<SystemRandomGenerator>(minimum, maximum, seed);
    }

    public static int Generate<TGenerator>(int minimum, int maximum)
        where TGenerator : class, IRandomGenerator, new()
    {
        return Generate<TGenerator>(minimum, maximum, GenerateSeed());
    }

    public static int Generate<TGenerator>(int minimum, int maximum, int seed)
        where TGenerator : class, IRandomGenerator, new()
    {
        if (minimum > maximum)
            throw new ArgumentException("Minimum must not be greater than maximum.");

        var generator = new TGenerator
        {
            Minimum = minimum,
            Maximum = maximum,
            Seed = seed
        };

        return generator.Generate();
    }

    public static int GeneratePeaked<TGenerator>(int minimum, int maximum, int peak)
        where TGenerator : class, IPeakedRandomGenerator, new()
    {
        return GeneratePeaked<TGenerator>(minimum, maximum, peak, GenerateSeed());
    }

    public static int GeneratePeaked<TGenerator>(int minimum, int maximum, int peak, int seed)
        where TGenerator : class, IPeakedRandomGenerator, new()
    {
        if (minimum > maximum)
            throw new ArgumentException("Minimum must not be greater than maximum.");

        if (peak < minimum || peak > maximum)
            throw new ArgumentOutOfRangeException(nameof(peak), "Peak must be within the range of minimum and maximum.");

        var generator = new TGenerator
        {
            Minimum = minimum,
            Maximum = maximum,
            Seed = seed
        };

        if (generator is IPeakedRandomGenerator peakable)
        {
            // Mit Record/Init-Only wird ein neues Objekt benötigt
            generator = (TGenerator)(object)new TGenerator
            {
                Minimum = minimum,
                Maximum = maximum,
                Seed = seed,
                Peak = peak
            };
        }

        return generator.Generate();
    }

    private static int GenerateSeed()
    {
        return Guid.NewGuid().GetHashCode(); //.ToByteArray().Sum(b => b);
    }
}
