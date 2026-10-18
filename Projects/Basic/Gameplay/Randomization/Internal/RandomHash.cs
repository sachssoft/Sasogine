namespace Sachssoft.Engine.Gameplay.Randomization.Internal;

/// <summary>Provides stable integer mixing shared by generators and lattice noise.</summary>
internal static class RandomHash
{
    internal const ulong Increment = 0x9E3779B97F4A7C15UL;

    // SplitMix64 finalizer, based on Sebastiano Vigna's public-domain reference:
    // https://prng.di.unimi.it/splitmix64.c
    internal static ulong Mix(ulong value)
    {
        unchecked
        {
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }

    internal static ulong Lattice(ulong seed, int x, int y)
    {
        unchecked
        {
            ulong first = Mix(seed ^ ((ulong)(uint)x + Increment));
            return Mix(first ^ ((ulong)(uint)y + 0xD1B54A32D192ED03UL));
        }
    }

    internal static ulong Lattice(ulong seed, int x)
    {
        unchecked { return Mix(seed ^ ((ulong)(uint)x + Increment)); }
    }

    internal static ulong Lattice(ulong seed, int x, int y, int z)
    {
        unchecked { return Mix(Lattice(seed, x, y) ^ ((ulong)(uint)z + 0x94D049BB133111EBUL)); }
    }

    internal static ulong WideLattice(ulong seed, long x, long y, long z)
    {
        unchecked
        {
            ulong value = Mix(seed ^ ((ulong)x + Increment));
            value = Mix(value ^ ((ulong)y + 0xD1B54A32D192ED03UL));
            return Mix(value ^ ((ulong)z + 0x94D049BB133111EBUL));
        }
    }

    internal static double Feature(ulong hash, int axis)
    {
        unchecked
        {
            ulong value = Mix(hash + (ulong)axis * Increment);
            return (value >> 11) * (1d / 9_007_199_254_740_992d);
        }
    }
}
