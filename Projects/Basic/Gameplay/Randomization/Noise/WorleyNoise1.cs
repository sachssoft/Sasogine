using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Returns normalized Euclidean distance to the nearest seeded cellular feature in 1D.</summary>
/// <remarks>One feature is placed per cell. The radius-two neighborhood guarantees nearest-feature coverage in 1D..3D.</remarks>
public sealed class WorleyNoise1 : INoise1
{
    /// <summary>Creates immutable cellular noise from a 64-bit seed.</summary>
    public WorleyNoise1(ulong seed) { Seed = seed; }
    /// <summary>Gets the feature seed.</summary>
    public ulong Seed { get; }
    /// <inheritdoc />
    public float GetValue(float x)
    {
        NoiseMath.ValidateCoordinate(x, nameof(x));
        int cellX = (int)MathF.Floor(x);
        double offsetX = x - cellX;
        double nearestSquared = double.PositiveInfinity;
        for (int dX = -2; dX <= 2; dX++)
        {
            ulong hash = RandomHash.WideLattice(Seed, (long)cellX + dX, 0L, 0L);
            double distanceX = dX + RandomHash.Feature(hash, 0) - offsetX;
            nearestSquared = Math.Min(nearestSquared, distanceX * distanceX);
        }
        return NoiseMath.Clamp01((float)(Math.Sqrt(nearestSquared) * 1d));
    }
}
