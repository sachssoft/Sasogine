using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Returns normalized Euclidean distance to the nearest seeded cellular feature in 2D.</summary>
/// <remarks>One feature is placed per cell. The radius-two neighborhood guarantees nearest-feature coverage in 1D..3D.</remarks>
public sealed class WorleyNoise2 : INoise2
{
    /// <summary>Creates immutable cellular noise from a 64-bit seed.</summary>
    public WorleyNoise2(ulong seed) { Seed = seed; }
    /// <summary>Gets the feature seed.</summary>
    public ulong Seed { get; }
    /// <inheritdoc />
    public float GetValue(float x, float y)
    {
        NoiseMath.ValidateCoordinate(x, nameof(x));
        NoiseMath.ValidateCoordinate(y, nameof(y));
        int cellX = (int)MathF.Floor(x);
        double offsetX = x - cellX;
        int cellY = (int)MathF.Floor(y);
        double offsetY = y - cellY;
        double nearestSquared = double.PositiveInfinity;
        for (int dX = -2; dX <= 2; dX++)
            for (int dY = -2; dY <= 2; dY++)
        {
            ulong hash = RandomHash.WideLattice(Seed, (long)cellX + dX, (long)cellY + dY, 0L);
            double distanceX = dX + RandomHash.Feature(hash, 0) - offsetX;
            double distanceY = dY + RandomHash.Feature(hash, 1) - offsetY;
            nearestSquared = Math.Min(nearestSquared, distanceX * distanceX + distanceY * distanceY);
        }
        return NoiseMath.Clamp01((float)(Math.Sqrt(nearestSquared) * 0.7071067811865475d));
    }
}
