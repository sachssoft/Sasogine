
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>
/// Represents a movable and resizable window hosted by a workspace overlay.
/// </summary>
/// <remarks>
/// Supports title bar interaction, pointer-based moving and resizing,
/// optional closing, and style-configurable window properties.
/// </remarks>
public class Window : ContentControl
{
    private bool _moving;
    private bool _resizing;

    private Vector2 _start;
    private Vector2 _original;
    private Size2 _startSize;

    private StyleProperty<string> _styleTitle = new(string.Empty);
    private StyleProperty<Vector2?> _stylePosition = new(null);
    private StyleProperty<bool> _styleCanResize = new(true);
    private StyleProperty<bool> _styleCanMove = new(true);
    private StyleProperty<bool> _styleCanClose = new(true);
    private StyleProperty<float> _styleTitlebarHeight = new(36f);

    private static readonly PropertyChangedEventArgs TitleChangedArgs = new(nameof(Title));
    private static readonly PropertyChangedEventArgs PositionChangedArgs = new(nameof(Position));
    private static readonly PropertyChangedEventArgs CanResizeChangedArgs = new(nameof(CanResize));
    private static readonly PropertyChangedEventArgs CanMoveChangedArgs = new(nameof(CanMove));
    private static readonly PropertyChangedEventArgs CanCloseChangedArgs = new(nameof(CanClose));
    private static readonly PropertyChangedEventArgs TitlebarHeightChangedArgs = new(nameof(TitlebarHeight));

    /// <summary>
    /// Initializes a new instance of the <see cref="Window"/> class.
    /// </summary>
    public Window()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Window";

    /// <summary>
    /// Gets or sets the text displayed in the window's title bar.
    /// </summary>
    public string Title
    {
        get => ConfigTitle;
        set => ConfigTitle = value;
    }

    /// <summary>
    /// Gets or sets the window position in workspace coordinates.
    /// A null value allows the workspace to determine the position.
    /// </summary>
    public Vector2? Position
    {
        get => ConfigPosition;
        set => ConfigPosition = value;
    }

    /// <summary>
    /// Gets or sets whether the window can be resized using its resize grip.
    /// </summary>
    public bool CanResize
    {
        get => ConfigCanResize;
        set => ConfigCanResize = value;
    }

    /// <summary>
    /// Gets or sets whether the window can be moved by dragging its title bar.
    /// </summary>
    public bool CanMove
    {
        get => ConfigCanMove;
        set => ConfigCanMove = value;
    }

    /// <summary>
    /// Gets or sets whether the window displays an interactive close button.
    /// </summary>
    public bool CanClose
    {
        get => ConfigCanClose;
        set => ConfigCanClose = value;
    }

    /// <summary>
    /// Gets or sets the height of the title bar in pixels.
    /// </summary>
    public float TitlebarHeight
    {
        get => ConfigTitlebarHeight;
        set => ConfigTitlebarHeight = value;
    }

    /// <summary>
    /// Occurs after the window has been closed by its hosting workspace.
    /// </summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Requests that the hosting workspace close this window.
    /// </summary>
    /// <remarks>
    /// The removal is performed through the workspace mutation mechanism.
    /// Calling this method has no effect when the window is not attached
    /// to a workspace.
    /// </remarks>
    public virtual void Close()
    {
        Workspace? root = Workspace;

        if (root is not null)
            root.Mutate(() => root.CloseOverlay(this));
    }

    /// <summary>
    /// Notifies listeners that the window has been closed.
    /// </summary>
    internal virtual void NotifyClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets or sets the effective title used by the window.
    /// </summary>
    internal string ConfigTitle
    {
        get => _styleTitle.Value;
        set
        {
            if (_styleTitle.SetLocal(value))
                NotifyPropertyChanged(TitleChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets the effective workspace position of the window.
    /// </summary>
    internal Vector2? ConfigPosition
    {
        get => _stylePosition.Value;
        set
        {
            if (_stylePosition.SetLocal(value))
                NotifyPropertyChanged(PositionChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets the effective resize permission.
    /// </summary>
    internal bool ConfigCanResize
    {
        get => _styleCanResize.Value;
        set
        {
            if (_styleCanResize.SetLocal(value))
                NotifyPropertyChanged(CanResizeChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets the effective movement permission.
    /// </summary>
    internal bool ConfigCanMove
    {
        get => _styleCanMove.Value;
        set
        {
            if (_styleCanMove.SetLocal(value))
                NotifyPropertyChanged(CanMoveChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets the effective close permission.
    /// </summary>
    internal bool ConfigCanClose
    {
        get => _styleCanClose.Value;
        set
        {
            if (_styleCanClose.SetLocal(value))
                NotifyPropertyChanged(CanCloseChangedArgs);
        }
    }

    /// <summary>
    /// Gets or sets the effective title bar height.
    /// </summary>
    internal float ConfigTitlebarHeight
    {
        get => _styleTitlebarHeight.Value;
        set
        {
            if (_styleTitlebarHeight.SetLocal(value))
                NotifyPropertyChanged(TitlebarHeightChangedArgs);
        }
    }

    /// <inheritdoc/>
    internal override Bounds2 ContentBounds => new(
        Bounds.X + Padding.Left,
        Bounds.Y + TitlebarHeight + Padding.Top,
        float.Max(0, Bounds.Width - Padding.Horizontal),
        float.Max(0, Bounds.Height - TitlebarHeight - Padding.Vertical));

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!float.IsFinite(TitlebarHeight) || TitlebarHeight < 0 ||
            Position is Vector2 p && (!float.IsFinite(p.X) || !float.IsFinite(p.Y)))
        {
            throw new InvalidOperationException("Invalid window geometry.");
        }

        Size2 body = base.MeasureOverride(
            context,
            new Size2(available.Width, float.Max(0, available.Height - TitlebarHeight)));

        return new Size2(
            float.Max(160, body.Width),
            float.Max(80, body.Height + TitlebarHeight));
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        Vector2 p = e.Position;

        float x = p.X - Bounds.X;
        float y = p.Y - Bounds.Y;

        if (e.Type == ControlInputEventType.PointerDown && e.Button == MouseButton.Left)
        {
            if (CanClose && x >= Bounds.Width - 32 && y < TitlebarHeight)
            {
                Close();
                args.Handled = true;
                return;
            }

            if (CanResize && x >= Bounds.Width - 14 && y >= Bounds.Height - 14)
                _resizing = true;
            else if (CanMove && y < TitlebarHeight)
                _moving = true;
            else
                return;

            _start = p;
            _original = new Vector2(Bounds.X, Bounds.Y);
            _startSize = Bounds.Size;

            context.CapturePointer(this);
            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.PointerMove && (_moving || _resizing))
        {
            Vector2 delta = p - _start;

            if (_moving)
            {
                Position = new Vector2(
                    float.Clamp(_original.X + delta.X, 0, float.Max(0, (Workspace?.Size.Width ?? float.MaxValue) - Bounds.Width)),
                    float.Clamp(_original.Y + delta.Y, 0, float.Max(0, (Workspace?.Size.Height ?? float.MaxValue) - Bounds.Height)));
            }
            else
            {
                Width = float.Max(MinWidth, _startSize.Width + delta.X);
                Height = float.Max(MinHeight, _startSize.Height + delta.Y);
                Position = _original;
            }

            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.PointerUp && e.Button == MouseButton.Left && (_moving || _resizing))
        {
            _moving = false;
            _resizing = false;

            context.ReleasePointer(this);
            args.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        base.OnInteractionCancelled();

        _moving = false;
        _resizing = false;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);

        if (!context.DrawSkinPart(this, "Titlebar", new Bounds2(0, 0, Bounds.Width, TitlebarHeight)))
        {
            context.FillRectangle(
                new Bounds2(0, 0, Bounds.Width, TitlebarHeight),
                new Color(36, 46, 62));
        }

        var font = GetFont(context);

        using (context.PushClipScope(new Bounds2(10, 0, float.Max(0, Bounds.Width - 48), TitlebarHeight)))
        {
            context.DrawText(
                font,
                Title,
                new Vector2(12, (TitlebarHeight - font.Measure(Title).Height) / 2),
                ConfigTextColor);
        }

        if (CanClose)
        {
            var close = new Bounds2(
                Bounds.Width - 30,
                (TitlebarHeight - 24) / 2,
                24,
                24);

            bool hover = Workspace is not null &&
                Contains(
                    new Bounds2(Bounds.X + close.X, Bounds.Y + close.Y, 24, 24),
                    Workspace.PointerPosition.X,
                    Workspace.PointerPosition.Y);

            context.DrawSkinPart(this, "CloseButton", close, hover ? "Hover" : "Normal");

            if (!context.DrawSkinPart(this, "CloseGlyph", close))
                context.DrawText(font, "×", new Vector2(close.X + 4, close.Y), Color.White);
        }

        if (CanResize)
        {
            context.DrawSkinPart(
                this,
                "ResizeGrip",
                new Bounds2(Bounds.Width - 20, Bounds.Height - 20, 20, 20));
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);

        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(Title):
                    if (_styleTitle.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(TitleChangedArgs);
                    break;

                case nameof(Position):
                    if (_stylePosition.Apply(Converters.Convert<Vector2?>("NullableVector2", entry.Value)))
                        NotifyPropertyChanged(PositionChangedArgs);
                    break;

                case nameof(CanResize):
                    if (_styleCanResize.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(CanResizeChangedArgs);
                    break;

                case nameof(CanMove):
                    if (_styleCanMove.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(CanMoveChangedArgs);
                    break;

                case nameof(CanClose):
                    if (_styleCanClose.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(CanCloseChangedArgs);
                    break;

                case nameof(TitlebarHeight):
                    if (_styleTitlebarHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(TitlebarHeightChangedArgs);
                    break;
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property)
    {
        return property switch
        {
            nameof(Title) => _styleTitle.HasLocalValue,
            nameof(Position) => _stylePosition.HasLocalValue,
            nameof(CanResize) => _styleCanResize.HasLocalValue,
            nameof(CanMove) => _styleCanMove.HasLocalValue,
            nameof(CanClose) => _styleCanClose.HasLocalValue,
            nameof(TitlebarHeight) => _styleTitlebarHeight.HasLocalValue,
            _ => base.IsStyleLocal(property)
        };
    }

    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(Title):
                if (_styleTitle.ClearLocal())
                    NotifyPropertyChanged(TitleChangedArgs);
                return;

            case nameof(Position):
                if (_stylePosition.ClearLocal())
                    NotifyPropertyChanged(PositionChangedArgs);
                return;

            case nameof(CanResize):
                if (_styleCanResize.ClearLocal())
                    NotifyPropertyChanged(CanResizeChangedArgs);
                return;

            case nameof(CanMove):
                if (_styleCanMove.ClearLocal())
                    NotifyPropertyChanged(CanMoveChangedArgs);
                return;

            case nameof(CanClose):
                if (_styleCanClose.ClearLocal())
                    NotifyPropertyChanged(CanCloseChangedArgs);
                return;

            case nameof(TitlebarHeight):
                if (_styleTitlebarHeight.ClearLocal())
                    NotifyPropertyChanged(TitlebarHeightChangedArgs);
                return;

            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();

        if (_styleTitle.BeginStyle(string.Empty))
            NotifyPropertyChanged(TitleChangedArgs);

        if (_stylePosition.BeginStyle(null))
            NotifyPropertyChanged(PositionChangedArgs);

        if (_styleCanResize.BeginStyle(true))
            NotifyPropertyChanged(CanResizeChangedArgs);

        if (_styleCanMove.BeginStyle(true))
            NotifyPropertyChanged(CanMoveChangedArgs);

        if (_styleCanClose.BeginStyle(true))
            NotifyPropertyChanged(CanCloseChangedArgs);

        if (_styleTitlebarHeight.BeginStyle(36f))
            NotifyPropertyChanged(TitlebarHeightChangedArgs);
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();

        if (_styleTitle.CommitStyle())
            NotifyPropertyChanged(TitleChangedArgs);

        if (_stylePosition.CommitStyle())
            NotifyPropertyChanged(PositionChangedArgs);

        if (_styleCanResize.CommitStyle())
            NotifyPropertyChanged(CanResizeChangedArgs);

        if (_styleCanMove.CommitStyle())
            NotifyPropertyChanged(CanMoveChangedArgs);

        if (_styleCanClose.CommitStyle())
            NotifyPropertyChanged(CanCloseChangedArgs);

        if (_styleTitlebarHeight.CommitStyle())
            NotifyPropertyChanged(TitlebarHeightChangedArgs);
    }
}
