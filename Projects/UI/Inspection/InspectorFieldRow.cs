namespace Sachssoft.Engine.Controls.Inspection;

/// <summary>Lays out a property with its label above the editor when horizontal space is limited.</summary>
internal sealed class InspectorFieldRow : ContainerControl
{
    private const float LabelWidth = 112, MenuWidth = 26, Gap = 8, LabelGap = 4;
    private readonly TextBlock _label;
    private readonly Control _editor;
    private readonly InspectorPropertyMenu _menu;
    private bool _stacked;

    internal InspectorFieldRow(string title, Control editor, InspectorPropertyMenu menu)
    {
        _label = new TextBlock
        {
            Text = title, ToolTip = title, WrapText = true,
            VerticalAlignment = ControlAlignment.Center
        };
        _editor = editor;
        _menu = menu;
        Children.Add(_label);
        Children.Add(editor);
        Children.Add(menu);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Control";

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _stacked = available.Width < 420;
        float trailing = _menu.IsVisible ? MenuWidth + Gap : 0;
        float heading = _stacked ? 0 : LabelWidth + Gap;
        _label.Measure(context, new Size2(_stacked ? available.Width : LabelWidth, float.PositiveInfinity));
        _menu.Measure(context, new Size2(MenuWidth, available.Height));
        _editor.Measure(context, new Size2(MathF.Max(0, available.Width - heading - trailing), available.Height));
        float editorHeight = MathF.Max(_editor.DesiredSize.Height, _menu.DesiredSize.Height);
        float height = _stacked ? _label.DesiredSize.Height + LabelGap + editorHeight
            : MathF.Max(_label.DesiredSize.Height, editorHeight);
        float width = float.IsFinite(available.Width) ? available.Width
            : heading + trailing + _editor.DesiredSize.Width;
        return new Size2(width, height);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float labelHeight = _stacked ? MathF.Min(_label.DesiredSize.Height, bounds.Height) : bounds.Height;
        _label.Arrange(new Bounds2(bounds.X, bounds.Y,
            _stacked ? bounds.Width : MathF.Min(LabelWidth, bounds.Width), labelHeight), true);
        float heading = _stacked ? 0 : MathF.Min(LabelWidth + Gap, bounds.Width);
        float top = _stacked ? MathF.Min(labelHeight + LabelGap, bounds.Height) : 0;
        float menuWidth = _menu.IsVisible ? MathF.Min(MenuWidth, MathF.Max(0, bounds.Width - heading)) : 0;
        float trailing = menuWidth > 0 ? menuWidth + Gap : 0;
        _editor.Arrange(new Bounds2(bounds.X + heading, bounds.Y + top,
            MathF.Max(0, bounds.Width - heading - trailing), MathF.Max(0, bounds.Height - top)), true);
        _menu.Arrange(new Bounds2(bounds.Right - menuWidth, bounds.Y + top,
            menuWidth, MathF.Max(0, bounds.Height - top)), true);
    }
}
