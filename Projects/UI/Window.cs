using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.ComponentModel;
using WindowSizing = Sachssoft.Engine.Controls.SizeToContent;
using ClosePlacement = Sachssoft.Engine.Controls.CloseButtonPlacement;

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
    private Size2 _titleSize;
    private bool AutoWidth => SizeToContent is WindowSizing.Width or WindowSizing.WidthAndHeight;
    private bool AutoHeight => SizeToContent is WindowSizing.Height or WindowSizing.WidthAndHeight;
    private bool HasCloseButton => ShowCloseButton && CanClose && TitlebarHeight > 0;
    /// <inheritdoc/>
    protected override float? LayoutWidth => AutoWidth ? null : base.LayoutWidth;
    /// <inheritdoc/>
    protected override float? LayoutHeight => AutoHeight ? null : base.LayoutHeight;
    /// <summary>Gets the visible close-button hit area in workspace coordinates, or an empty rectangle when hidden.</summary>
    public Bounds2 CloseButtonBounds => HasCloseButton ? new Bounds2(CloseButtonPlacement == ClosePlacement.Left ? Bounds.X : Bounds.Right - MathF.Min(32, Bounds.Width), Bounds.Y, MathF.Min(32, Bounds.Width), MathF.Min(TitlebarHeight, Bounds.Height)) : Bounds2.Zero;

    private StyleProperty<WindowSizing> _styleSizeToContent = new(WindowSizing.Manual);
    private static readonly PropertyChangedEventArgs SizeToContentArgs = new(nameof(SizeToContent));
    /// <summary>Gets or sets the dimensions fitted to content. Manual preserves configured dimensions; automatic axes ignore Width or Height without changing their stored values.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The sizing mode is unsupported.</exception>
    public WindowSizing SizeToContent
    {
        get => _styleSizeToContent.Value;
        set
        {
            VerifyTreeMutation();
            if (value is not (WindowSizing.Manual or WindowSizing.Width or WindowSizing.Height or WindowSizing.WidthAndHeight))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleSizeToContent.SetLocal(value))
                NotifyPropertyChanged(SizeToContentArgs);
        }
    }

    private StyleProperty<ClosePlacement> _styleCloseButtonPlacement = new(ClosePlacement.Right);
    private static readonly PropertyChangedEventArgs CloseButtonPlacementArgs = new(nameof(CloseButtonPlacement));
    /// <summary>Gets or sets the title-bar edge for the close button. The default is Right.</summary>
    /// <exception cref = "ArgumentOutOfRangeException">The placement is not Left or Right.</exception>
    public ClosePlacement CloseButtonPlacement
    {
        get => _styleCloseButtonPlacement.Value;
        set
        {
            VerifyTreeMutation();
            if (value is not (ClosePlacement.Left or ClosePlacement.Right))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_styleCloseButtonPlacement.SetLocal(value))
                NotifyPropertyChanged(CloseButtonPlacementArgs);
        }
    }

    private StyleProperty<bool> _styleShowCloseButton = new(true);
    private static readonly PropertyChangedEventArgs ShowCloseButtonArgs = new(nameof(ShowCloseButton));
    /// <summary>Gets or sets whether to display the close button. CanClose must also be true; hiding the button does not block programmatic closing.</summary>
    public bool ShowCloseButton
    {
        get => _styleShowCloseButton.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleShowCloseButton.SetLocal(value))
                NotifyPropertyChanged(ShowCloseButtonArgs);
        }
    }

    private StyleProperty<bool> _styleIsCloseButtonEnabled = new(true);
    private static readonly PropertyChangedEventArgs IsCloseButtonEnabledArgs = new(nameof(IsCloseButtonEnabled));
    /// <summary>Gets or sets whether the visible close button accepts pointer input. False keeps a disabled button visible and does not block programmatic closing.</summary>
    public bool IsCloseButtonEnabled
    {
        get => _styleIsCloseButtonEnabled.Value;
        set
        {
            VerifyTreeMutation();
            if (_styleIsCloseButtonEnabled.SetLocal(value))
                NotifyPropertyChanged(IsCloseButtonEnabledArgs);
        }
    }

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
    /// Initializes a new instance of the <see cref = "Window"/> class.
    /// </summary>
    public Window()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Window";
    /// <summary>
    /// Gets or sets the text displayed in the window's title bar.
    /// </summary>
    public string Title { get => ConfigTitle; set => ConfigTitle = value; }
    /// <summary>
    /// Gets or sets the window position in workspace coordinates.
    /// A null value allows the workspace to determine the position.
    /// </summary>
    public Vector2? Position { get => ConfigPosition; set => ConfigPosition = value; }
    /// <summary>
    /// Gets or sets whether the window can be resized using its resize grip.
    /// </summary>
    public bool CanResize { get => ConfigCanResize; set => ConfigCanResize = value; }
    /// <summary>
    /// Gets or sets whether the window can be moved by dragging its title bar.
    /// </summary>
    public bool CanMove { get => ConfigCanMove; set => ConfigCanMove = value; }
    /// <summary>
    /// Gets or sets whether the window displays an interactive close button.
    /// </summary>
    public bool CanClose { get => ConfigCanClose; set => ConfigCanClose = value; }
    /// <summary>
    /// Gets or sets the height of the title bar in pixels.
    /// </summary>
    public float TitlebarHeight { get => ConfigTitlebarHeight; set => ConfigTitlebarHeight = value; }

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
    internal override Bounds2 ContentBounds => new(Bounds.X + Padding.Left, Bounds.Y + TitlebarHeight + Padding.Top, float.Max(0, Bounds.Width - Padding.Horizontal), float.Max(0, Bounds.Height - TitlebarHeight - Padding.Vertical));

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!float.IsFinite(TitlebarHeight) || TitlebarHeight < 0 || Position is Vector2 p && (!float.IsFinite(p.X) || !float.IsFinite(p.Y)))
        {
            throw new InvalidOperationException("Invalid window geometry.");
        }

        ArgumentNullException.ThrowIfNull(Title);
        if (SizeToContent is not (WindowSizing.Manual or WindowSizing.Width or WindowSizing.Height or WindowSizing.WidthAndHeight) || CloseButtonPlacement is not (ClosePlacement.Left or ClosePlacement.Right))
            throw new InvalidOperationException("Invalid window content sizing or close-button placement.");
        _titleSize = GetFont(context).Measure(Title);
        float height = AutoHeight ? float.PositiveInfinity : MathF.Max(0, available.Height - TitlebarHeight);
        Size2 body = base.MeasureOverride(context, new Size2(AutoWidth ? float.PositiveInfinity : available.Width, height));
        // Reflow wrapping content when its natural width exceeds the workspace constraint.
        bool constrainedWidth = AutoWidth && body.Width > available.Width;
        if (constrainedWidth)
            body = base.MeasureOverride(context, new Size2(available.Width, height));
        float titleWidth = _titleSize.Width + (HasCloseButton ? 32 : 0) + 24 - Padding.Horizontal;
        return new Size2(AutoWidth ? MathF.Max(constrainedWidth ? available.Width : body.Width, MathF.Max(0, titleWidth)) : MathF.Max(160, body.Width), AutoHeight ? body.Height + TitlebarHeight : MathF.Max(80, body.Height + TitlebarHeight));
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
            if (HasCloseButton && Contains(CloseButtonBounds, p.X, p.Y))
            {
                if (IsCloseButtonEnabled)
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
                Position = new Vector2(float.Clamp(_original.X + delta.X, 0, float.Max(0, (Workspace?.Size.Width ?? float.MaxValue) - Bounds.Width)), float.Clamp(_original.Y + delta.Y, 0, float.Max(0, (Workspace?.Size.Height ?? float.MaxValue) - Bounds.Height)));
            }
            else
            {
                SizeToContent = WindowSizing.Manual;
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
            context.FillRectangle(new Bounds2(0, 0, Bounds.Width, TitlebarHeight), new Color(36, 46, 62));
        }

        var font = GetFont(context);
        float titleLeft = HasCloseButton && CloseButtonPlacement == ClosePlacement.Left ? 40 : 12;
        float titleRight = HasCloseButton && CloseButtonPlacement == ClosePlacement.Right ? Bounds.Width - 36 : Bounds.Width - 12;
        using (context.PushClipScope(new Bounds2(titleLeft, 0, MathF.Max(0, titleRight - titleLeft), TitlebarHeight)))
            context.DrawText(font, Title, new Vector2(titleLeft, (TitlebarHeight - _titleSize.Height) / 2), ConfigTextColor);
        if (HasCloseButton)
        {
            Bounds2 hit = CloseButtonBounds;
            float size = MathF.Min(24, MathF.Min(hit.Width, hit.Height));
            var close = new Bounds2(hit.X - Bounds.X + (hit.Width - size) / 2, (hit.Height - size) / 2, size, size);
            bool enabled = IsCloseButtonEnabled && IsAvailable();
            bool hover = enabled && Workspace is not null && Contains(hit, Workspace.PointerPosition.X, Workspace.PointerPosition.Y);
            string state = !enabled ? "Disabled" : hover ? "Hover" : "Normal";
            context.DrawSkinPart(this, "CloseButton", close, state);
            Color color = enabled ? TextColor : DisabledTextColor;
            Color glyphTint = enabled ? Color.White : Color.White * .4f;
            if (!context.DrawSkinPart(this, "CloseGlyph", close, state, glyphTint))
                context.DrawText(font, "×", new Vector2(close.X + 4, close.Y), color);
        }

        if (CanResize)
        {
            context.DrawSkinPart(this, "ResizeGrip", new Bounds2(Bounds.Width - 20, Bounds.Height - 20, 20, 20));
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
                case nameof(SizeToContent):
                    if (_styleSizeToContent.Apply(Converters.Convert<WindowSizing>("SizeToContent", entry.Value)))
                        NotifyPropertyChanged(SizeToContentArgs);
                    break;
                case nameof(CloseButtonPlacement):
                    if (_styleCloseButtonPlacement.Apply(Converters.Convert<ClosePlacement>("CloseButtonPlacement", entry.Value)))
                        NotifyPropertyChanged(CloseButtonPlacementArgs);
                    break;
                case nameof(ShowCloseButton):
                    if (_styleShowCloseButton.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(ShowCloseButtonArgs);
                    break;
                case nameof(IsCloseButtonEnabled):
                    if (_styleIsCloseButtonEnabled.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsCloseButtonEnabledArgs);
                    break;
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
            nameof(SizeToContent) => _styleSizeToContent.HasLocalValue,
            nameof(CloseButtonPlacement) => _styleCloseButtonPlacement.HasLocalValue,
            nameof(ShowCloseButton) => _styleShowCloseButton.HasLocalValue,
            nameof(IsCloseButtonEnabled) => _styleIsCloseButtonEnabled.HasLocalValue,
            nameof(Title) => _styleTitle.HasLocalValue,
            nameof(Position) => _stylePosition.HasLocalValue,
            nameof(CanResize) => _styleCanResize.HasLocalValue,
            nameof(CanMove) => _styleCanMove.HasLocalValue,
            nameof(CanClose) => _styleCanClose.HasLocalValue,
            nameof(TitlebarHeight) => _styleTitlebarHeight.HasLocalValue,
            _ => base.IsStyleLocal(property)};
    }

    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(SizeToContent):
                if (_styleSizeToContent.ClearLocal())
                    NotifyPropertyChanged(SizeToContentArgs);
                return;
            case nameof(CloseButtonPlacement):
                if (_styleCloseButtonPlacement.ClearLocal())
                    NotifyPropertyChanged(CloseButtonPlacementArgs);
                return;
            case nameof(ShowCloseButton):
                if (_styleShowCloseButton.ClearLocal())
                    NotifyPropertyChanged(ShowCloseButtonArgs);
                return;
            case nameof(IsCloseButtonEnabled):
                if (_styleIsCloseButtonEnabled.ClearLocal())
                    NotifyPropertyChanged(IsCloseButtonEnabledArgs);
                return;
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
        if (_styleSizeToContent.BeginStyle(WindowSizing.Manual))
            NotifyPropertyChanged(SizeToContentArgs);
        if (_styleCloseButtonPlacement.BeginStyle(ClosePlacement.Right))
            NotifyPropertyChanged(CloseButtonPlacementArgs);
        if (_styleShowCloseButton.BeginStyle(true))
            NotifyPropertyChanged(ShowCloseButtonArgs);
        if (_styleIsCloseButtonEnabled.BeginStyle(true))
            NotifyPropertyChanged(IsCloseButtonEnabledArgs);
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
        if (_styleSizeToContent.CommitStyle())
            NotifyPropertyChanged(SizeToContentArgs);
        if (_styleCloseButtonPlacement.CommitStyle())
            NotifyPropertyChanged(CloseButtonPlacementArgs);
        if (_styleShowCloseButton.CommitStyle())
            NotifyPropertyChanged(ShowCloseButtonArgs);
        if (_styleIsCloseButtonEnabled.CommitStyle())
            NotifyPropertyChanged(IsCloseButtonEnabledArgs);
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
