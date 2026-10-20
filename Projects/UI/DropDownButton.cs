namespace Sachssoft.Engine.Controls;
/// <summary>A content button that opens an owned popup menu without executing a primary command.</summary>
public class DropDownButton : ButtonBase
{
    private ContextMenu? _menu;
    private static readonly System.ComponentModel.PropertyChangedEventArgs MenuArgs = new(nameof(Menu));
    /// <summary>Gets or sets the popup menu. It must not already belong to another owner.</summary>
    public ContextMenu? Menu { get => _menu; set => SetNotify(ref _menu, value, MenuArgs); }
    /// <summary>Gets whether this button opened a popup during the current update.</summary>
    public bool WasOpened { get; private set; }
    /// <inheritdoc/>
    protected override string DefaultStyleName => "DropDownButton";

    /// <summary>Opens the menu below this button.</summary>
    public void OpenMenu()
    {
        if (!WasOpened && IsAvailable() && Workspace is Workspace root && Menu is ContextMenu menu && menu.Workspace is null)
        {
            root.ShowPopup(menu, new Bounds2(Bounds.X, Bounds.Y + Bounds.Height, Bounds.Width, 0), this);
            WasOpened = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnClick()
    {
        RaiseClicked();
        OpenMenu();
    }

    internal override void BeginFrame()
    {
        base.BeginFrame();
        WasOpened = false;
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        if (args.Input.Type == ControlInputEventType.KeyDown && args.Input.Key is Microsoft.Xna.Framework.Input.Keys.F4 or Microsoft.Xna.Framework.Input.Keys.Down)
        {
            OpenMenu();
            args.Handled = true;
            return;
        }

        base.OnInput(context, args);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        var s = base.MeasureOverride(context, available);
        return new Size2(s.Width + 24, MathF.Max(20, s.Height));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b) => base.ArrangeOverride(new Bounds2(b.X, b.Y, MathF.Max(0, b.Width - 24), b.Height));
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        float x = Bounds.Width - 16, y = Bounds.Height / 2;
        context.DrawLine(new(x - 4, y - 2), new(x, y + 2), TextColor);
        context.DrawLine(new(x, y + 2), new(x + 4, y - 2), TextColor);
    }
}
