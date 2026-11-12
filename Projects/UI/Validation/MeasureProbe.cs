using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;

internal sealed class MeasureProbe : Control
{
    internal int Measurements;
    internal int Arrangements;
    internal bool FailMeasure, FailArrange;
    internal Action? OnMeasure { get; set; }
    internal Action? OnArrange { get; set; }
    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        Arrangements++;
        if (FailArrange) throw new InvalidOperationException("Arrange failure");
        OnArrange?.Invoke();
    }
    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        Measurements++;
        if (FailMeasure) throw new InvalidOperationException("Measure failure");
        OnMeasure?.Invoke();
        return new(20, 20);
    }
}
