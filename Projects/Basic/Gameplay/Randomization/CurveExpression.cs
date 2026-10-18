using Sachssoft.Engine.Gameplay.Curves;
using Sachssoft.Engine.Gameplay.Randomization.Internal;
using System;

namespace Sachssoft.Engine.Gameplay.Randomization;

/// <summary>Shapes a normalized expression result with an existing curve.</summary>
public sealed class CurveExpression : IRandomExpression<float>
{
    /// <summary>Creates a curve expression; input and output must remain normalized.</summary>
    public CurveExpression(IRandomExpression<float> source, CurveBase curve, bool clampOutput = true)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(curve);
        Source = source;
        Curve = curve;
        ClampOutput = clampOutput;
    }
    /// <summary>Gets the normalized source expression.</summary>
    public IRandomExpression<float> Source { get; }
    /// <summary>Gets the shaping curve.</summary>
    public CurveBase Curve { get; }
    /// <summary>Gets whether finite curve overshoot is clamped instead of rejected.</summary>
    public bool ClampOutput { get; }
    /// <inheritdoc />
    public float GetValue(IRandomGenerator random)
    {
        ArgumentNullException.ThrowIfNull(random);
        return RandomMath.Shape(Source.GetValue(random), Curve, ClampOutput);
    }
}
