
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Represents a button that opens an associated popup menu
/// without executing a primary command.
/// </summary>
/// <remarks>
/// <para>
/// The menu opens below the button when activated by pointer,
/// keyboard, or programmatic invocation.
/// </para>
/// <para>
/// The <see cref="ButtonBase.Clicked"/> event is raised before
/// the menu is opened through normal button activation.
/// </para>
/// <para>
/// The associated <see cref="Menu"/> must not already belong
/// to another workspace or popup owner.
/// </para>
/// </remarks>
public class DropDownButton : ButtonBase
{
    private ContextMenu? _menu;

    private static readonly PropertyChangedEventArgs MenuArgs = new(nameof(Menu));

    /// <summary>
    /// Gets or sets the context menu displayed when the button is activated.
    /// </summary>
    /// <remarks>
    /// The menu must not already be attached to a workspace
    /// when <see cref="OpenMenu"/> is called.
    /// </remarks>
    public ContextMenu? Menu
    {
        get => _menu;
        set => SetNotify(ref _menu, value, MenuArgs);
    }

    /// <summary>
    /// Gets whether this button opened its menu during the current update frame.
    /// </summary>
    /// <remarks>
    /// This value is reset at the beginning of each frame.
    /// </remarks>
    public bool WasOpened { get; private set; }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "DropDownButton";

    /// <summary>
    /// Opens the associated menu directly below the button.
    /// </summary>
    /// <remarks>
    /// Does nothing if the button is unavailable, the menu is missing,
    /// the menu is already attached to a workspace, or the menu
    /// has already been opened during the current frame.
    /// </remarks>
    public void OpenMenu()
    {
        if (WasOpened || !IsAvailable() || Workspace is not Workspace root ||
            Menu is not ContextMenu menu || menu.Workspace is not null)
            return;

        var anchor = new Bounds2(Bounds.X, Bounds.Y + Bounds.Height, Bounds.Width, 0);

        root.ShowPopup(menu, anchor, this);
        WasOpened = true;
    }

    /// <inheritdoc/>
    protected override void OnClick()
    {
        RaiseClicked();
        OpenMenu();
    }

    /// <inheritdoc/>
    internal override void BeginFrame()
    {
        base.BeginFrame();
        WasOpened = false;
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        if (args.Input.Type == ControlInputEventType.KeyDown &&
            args.Input.Key is Keys.F4 or Keys.Down)
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
        Size2 size = base.MeasureOverride(context, available);
        return new Size2(size.Width + 24, float.Max(20, size.Height));
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        var contentBounds = new Bounds2(
            bounds.X, bounds.Y,
            float.Max(0, bounds.Width - 24),
            bounds.Height);

        base.ArrangeOverride(contentBounds);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);

        float x = Bounds.Width - 16;
        float y = Bounds.Height / 2;

        context.DrawLine(new(x - 4, y - 2), new(x, y + 2), TextColor);
        context.DrawLine(new(x, y + 2), new(x + 4, y - 2), TextColor);
    }
}
