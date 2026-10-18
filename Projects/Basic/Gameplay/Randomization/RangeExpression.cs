using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>Draws a float between constant or recursively evaluated bounds.</summary>
/// <remarks>The minimum is evaluated before the maximum, once each, followed by the draw.</remarks>
public sealed class RangeExpression : IRandomExpression<float>
{
    /// <summary>Creates a range with finite constant bounds; the maximum is exclusive.</summary>
    public RangeExpression(float minimum, float maximum)
        : this(new ConstantExpression<float>(minimum), new ConstantExpression<float>(maximum))
    {
        RandomMath.ValidateRange(minimum, maximum);
    }
    /// <summary>Creates a range whose bounds are evaluated for each draw.</summary>
    public RangeExpression(IRandomExpression<float> minimum, IRandomExpression<float> maximum)
    {
        ArgumentNullException.ThrowIfNull(minimum);
        ArgumentNullException.ThrowIfNull(maximum);
        Minimum = minimum;
        Maximum = maximum;
    }
    /// <summary>Gets the minimum expression.</summary>
    public IRandomExpression<float> Minimum { get; }
    /// <summary>Gets the maximum expression.</summary>
    public IRandomExpression<float> Maximum { get; }
    /// <inheritdoc />
    public float GetValue(IRandomGenerator random)
    {
        ArgumentNullException.ThrowIfNull(random);
        float minimum = Minimum.GetValue(random);
        float maximum = Maximum.GetValue(random);
        return random.NextSingle(minimum, maximum);
    }
}
