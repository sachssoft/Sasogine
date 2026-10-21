using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
/// <summary>Counts parent layout work after text metric changes.</summary>
internal sealed class TextLayoutProbePanel : Panel
{
    internal int Measurements, Arrangements;
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    { Measurements++; return base.MeasureOverride(context, available); }
    protected override void ArrangeOverride(Bounds2 bounds)
    { Arrangements++; base.ArrangeOverride(bounds); }
}
