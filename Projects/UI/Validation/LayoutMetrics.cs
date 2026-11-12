#if LAYOUT_METRICS
using Sachssoft.Engine.Controls;

internal static class LayoutMetrics
{
    internal static readonly HashSet<Control> Affected = new(ReferenceEqualityComparer.Instance);
    internal static long MeasureCalls, Measures, ArrangeCalls, Arranges, Propagation, Passes, Prepared;

    internal static void Reset()
    {
        MeasureCalls = Measures = ArrangeCalls = Arranges = Propagation = Passes = Prepared = 0;
        Affected.Clear();
    }

    internal static void Visit(Control control)
    {
        Propagation++;
        Affected.Add(control);
    }
}
#endif
