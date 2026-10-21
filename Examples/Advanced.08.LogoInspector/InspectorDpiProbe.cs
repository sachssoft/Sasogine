namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Counts actual layout measurements for DPI and resize cache checks.</summary>
internal sealed class InspectorDpiProbe : Control
{
    internal int Measures { get; private set; }
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    { Measures++; return new Size2(20, 20); }
}
