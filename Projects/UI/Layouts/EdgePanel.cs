namespace Sachssoft.Engine.Controls.Layouts;
/// <summary>Stacks two ordered groups against opposite edges, leaving unused space between them.</summary>
/// <remarks>Horizontal orientation uses left/right edges; vertical orientation uses top/bottom edges.
/// Children added without Put belong to Start. End children retain their insertion order.
/// Under constrained space, the Start group has priority and remaining slots are clipped without overlap.</remarks>
public class EdgePanel : StackPanel
{
    private readonly Dictionary<Control, PanelEdge> _edges = new(ReferenceEqualityComparer.Instance);
    /// <summary>Creates a horizontal panel with zero spacing.</summary>
    public EdgePanel() : base(StackOrientation.Horizontal)
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "EdgePanel";

    /// <summary>Adds an unowned control or moves an existing child to an edge group without duplicating it.</summary>
    /// <param name = "control">The control to place. Existing ownership by this panel is retained.</param>
    /// <param name = "edge">The target edge group, defaulting to Start.</param>
    /// <exception cref = "ArgumentNullException">The control is null.</exception>
    /// <exception cref = "ArgumentOutOfRangeException">The edge is unsupported.</exception>
    /// <exception cref = "InvalidOperationException">The control belongs to another owner or would introduce a cycle.</exception>
    public void Put(Control control, PanelEdge edge = PanelEdge.Start)
    {
        ArgumentNullException.ThrowIfNull(control);
        VerifyTreeMutation();
        if (edge is not (PanelEdge.Start or PanelEdge.End))
            throw new ArgumentOutOfRangeException(nameof(edge));
        if (!ReferenceEquals(control.Parent, this))
            base.Add(control);
        if (GetEdge(control) == edge)
            return;
        if (edge == PanelEdge.Start)
            _edges.Remove(control);
        else
            _edges[control] = edge;
        InvalidateMeasure();
    }

    /// <summary>Gets the assigned edge, or Start when the control has no explicit assignment.</summary>
    /// <param name = "control">The control whose assignment is queried.</param>
    /// <returns>The orientation-relative edge group.</returns>
    /// <exception cref = "ArgumentNullException">The control is null.</exception>
    public PanelEdge GetEdge(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        return _edges.TryGetValue(control, out PanelEdge edge) ? edge : PanelEdge.Start;
    }

    /// <inheritdoc/>
    public override void Remove(Control child)
    {
        ArgumentNullException.ThrowIfNull(child);
        VerifyTreeMutation();
        _edges.Remove(child);
        base.Remove(child);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
        base.Clear();
        _edges.Clear();
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        bool vertical = Orientation == StackOrientation.Vertical;
        float length = vertical ? bounds.Height : bounds.Width;
        float startExtent = 0, endExtent = 0;
        int startCount = 0, endCount = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
                continue;
            float extent = vertical ? child.DesiredSize.Height : child.DesiredSize.Width;
            if (GetEdge(child) == PanelEdge.Start)
            {
                if (startCount++ > 0)
                    startExtent += Spacing;
                startExtent += extent;
            }
            else
            {
                if (endCount++ > 0)
                    endExtent += Spacing;
                endExtent += extent;
            }
        }

        float gap = startCount > 0 && endCount > 0 ? Spacing : 0;
        float end = MathF.Max(MathF.Min(length, startExtent + gap), length - endExtent);
        float startLimit = MathF.Max(0, end - gap);
        float start = 0;
        foreach (Control child in Children)
        {
            if (!child.IsVisible)
            {
                child.Arrange(Bounds2.Zero);
                continue;
            }

            bool atStart = GetEdge(child) == PanelEdge.Start;
            float cursor = atStart ? start : end;
            float limit = atStart ? startLimit : length;
            float extent = MathF.Min(vertical ? child.DesiredSize.Height : child.DesiredSize.Width, MathF.Max(0, limit - cursor));
            child.Arrange(vertical ? new Bounds2(bounds.X, bounds.Y + MathF.Min(cursor, length), bounds.Width, extent) : new Bounds2(bounds.X + MathF.Min(cursor, length), bounds.Y, extent, bounds.Height), true);
            if (atStart)
                start += extent + Spacing;
            else
                end += extent + Spacing;
        }
    }
}
