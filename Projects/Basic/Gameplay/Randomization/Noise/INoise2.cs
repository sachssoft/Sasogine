namespace Sachssoft.Engine.Gameplay.Randomization.Noise;

/// <summary>Provides normalized, coordinate-dependent noise in two dimensions.</summary>
/// <remarks>
/// Results are in [0, 1]. Implementations in this module accept finite coordinates
/// in [int.MinValue, int.MaxValue). Sample fractional coordinates for smooth detail.
/// Implementations must return the same result for the same coordinates and state.
/// </remarks>
public interface INoise2
{
    /// <summary>Samples the noise at the specified coordinates.</summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    /// <returns>A normalized value in [0, 1].</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// A coordinate is not finite or lies outside the supported range.
    /// </exception>
    float GetValue(float x, float y);
}
