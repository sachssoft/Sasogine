using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls;
/// <summary>A movable, resizable window hosted by a Workspace overlay.</summary>
public class Window : ContentControl
{
    private bool _moving, _resizing;
    private Vector2 _start, _original;
    private Size2 _startSize;
    /// <summary>Initializes a new window instance.</summary>
    public Window() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Window";
    /// <summary>Gets or sets title.</summary>
    public string Title { get => ConfigTitle; set => ConfigTitle = value; }
    /// <summary>Gets or sets position.</summary>
    public Vector2? Position { get => ConfigPosition; set => ConfigPosition = value; }
    /// <summary>Gets or sets can resize.</summary>
    public bool CanResize { get => ConfigCanResize; set => ConfigCanResize = value; }
    /// <summary>Gets or sets can move.</summary>
    public bool CanMove { get => ConfigCanMove; set => ConfigCanMove = value; }
    /// <summary>Gets or sets can close.</summary>
    public bool CanClose { get => ConfigCanClose; set => ConfigCanClose = value; }
    /// <summary>Gets or sets titlebar height.</summary>
    public float TitlebarHeight { get => ConfigTitlebarHeight; set => ConfigTitlebarHeight = value; }

    /// <summary>Occurs when closed.</summary>
    public event EventHandler? Closed;
    /// <summary>Performs close using the current control state.</summary>
    public virtual void Close()
    {
        Workspace? root = Workspace;
        if (root is not null)
            root.Mutate(() => root.CloseOverlay(this));
    }

    internal virtual void NotifyClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    internal override Bounds2 ContentBounds => new(Bounds.X + Padding.Left, Bounds.Y + TitlebarHeight + Padding.Top, MathF.Max(0, Bounds.Width - Padding.Horizontal), MathF.Max(0, Bounds.Height - TitlebarHeight - Padding.Vertical));

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!float.IsFinite(TitlebarHeight) || TitlebarHeight < 0 || Position is Vector2 p && (!float.IsFinite(p.X) || !float.IsFinite(p.Y)))
            throw new InvalidOperationException("Invalid window geometry.");
        Size2 body = base.MeasureOverride(context, new Size2(available.Width, MathF.Max(0, available.Height - TitlebarHeight)));
        return new Size2(MathF.Max(160, body.Width), MathF.Max(80, body.Height + TitlebarHeight));
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        Vector2 p = e.Position;
        float x = p.X - Bounds.X, y = p.Y - Bounds.Y;
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
                Position = new Vector2(Math.Clamp(_original.X + delta.X, 0, MathF.Max(0, (Workspace?.Size.Width ?? float.MaxValue) - Bounds.Width)), Math.Clamp(_original.Y + delta.Y, 0, MathF.Max(0, (Workspace?.Size.Height ?? float.MaxValue) - Bounds.Height)));
            else
            {
                Width = MathF.Max(MinWidth, _startSize.Width + delta.X);
                Height = MathF.Max(MinHeight, _startSize.Height + delta.Y);
                Position = _original;
            }

            args.Handled = true;
        }
        else if (e.Type == ControlInputEventType.PointerUp && e.Button == MouseButton.Left && (_moving || _resizing))
        {
            context.ReleasePointer(this);
            args.Handled = true;
        }
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        _moving = false;
        _resizing = false;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        if (!context.DrawSkinPart(this, "Titlebar", new Bounds2(0, 0, Bounds.Width, TitlebarHeight)))
            context.FillRectangle(new Bounds2(0, 0, Bounds.Width, TitlebarHeight), new Color(36, 46, 62));
        var font = ConfigFont ?? context.DefaultFont;
        using (context.PushClipScope(new Bounds2(10, 0, MathF.Max(0, Bounds.Width - 48), TitlebarHeight)))
            context.DrawText(font, Title, new Vector2(12, (TitlebarHeight - font.Measure(Title).Height) / 2), ConfigTextColor);
        if (CanClose)
        {
            var close = new Bounds2(Bounds.Width - 30, (TitlebarHeight - 24) / 2, 24, 24);
            bool hover = Workspace is not null && Contains(new Bounds2(Bounds.X + close.X, Bounds.Y + close.Y, 24, 24), Workspace.PointerPosition.X, Workspace.PointerPosition.Y);
            context.DrawSkinPart(this, "CloseButton", close, hover ? "Hover" : "Normal");
            if (!context.DrawSkinPart(this, "CloseGlyph", close))
                context.DrawText(font, "×", new Vector2(close.X + 4, close.Y), Color.White);
        }

        if (CanResize)
            context.DrawSkinPart(this, "ResizeGrip", new Bounds2(Bounds.Width - 20, Bounds.Height - 20, 20, 20));
    }

    private StyleProperty<string> _styleTitle = new(string.Empty);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TitleChangedArgs = new(nameof(Title));
    internal string ConfigTitle
    {
        get => _styleTitle.Value;
        set
        {
            if (_styleTitle.SetLocal(value))
                NotifyPropertyChanged(TitleChangedArgs);
        }
    }

    private StyleProperty<Vector2?> _stylePosition = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs PositionChangedArgs = new(nameof(Position));
    internal Vector2? ConfigPosition
    {
        get => _stylePosition.Value;
        set
        {
            if (_stylePosition.SetLocal(value))
                NotifyPropertyChanged(PositionChangedArgs);
        }
    }

    private StyleProperty<bool> _styleCanResize = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CanResizeChangedArgs = new(nameof(CanResize));
    internal bool ConfigCanResize
    {
        get => _styleCanResize.Value;
        set
        {
            if (_styleCanResize.SetLocal(value))
                NotifyPropertyChanged(CanResizeChangedArgs);
        }
    }

    private StyleProperty<bool> _styleCanMove = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CanMoveChangedArgs = new(nameof(CanMove));
    internal bool ConfigCanMove
    {
        get => _styleCanMove.Value;
        set
        {
            if (_styleCanMove.SetLocal(value))
                NotifyPropertyChanged(CanMoveChangedArgs);
        }
    }

    private StyleProperty<bool> _styleCanClose = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CanCloseChangedArgs = new(nameof(CanClose));
    internal bool ConfigCanClose
    {
        get => _styleCanClose.Value;
        set
        {
            if (_styleCanClose.SetLocal(value))
                NotifyPropertyChanged(CanCloseChangedArgs);
        }
    }

    private StyleProperty<float> _styleTitlebarHeight = new(36f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs TitlebarHeightChangedArgs = new(nameof(TitlebarHeight));
    internal float ConfigTitlebarHeight
    {
        get => _styleTitlebarHeight.Value;
        set
        {
            if (_styleTitlebarHeight.SetLocal(value))
                NotifyPropertyChanged(TitlebarHeightChangedArgs);
        }
    }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "Title":
                    if (_styleTitle.Apply(Converters.Convert<string>("String", entry.Value)))
                        NotifyPropertyChanged(TitleChangedArgs);
                    break;
                case "Position":
                    if (_stylePosition.Apply(Converters.Convert<Vector2?>("NullableVector2", entry.Value)))
                        NotifyPropertyChanged(PositionChangedArgs);
                    break;
                case "CanResize":
                    if (_styleCanResize.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(CanResizeChangedArgs);
                    break;
                case "CanMove":
                    if (_styleCanMove.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(CanMoveChangedArgs);
                    break;
                case "CanClose":
                    if (_styleCanClose.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(CanCloseChangedArgs);
                    break;
                case "TitlebarHeight":
                    if (_styleTitlebarHeight.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(TitlebarHeightChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "Title":
                return _styleTitle.HasLocalValue;
            case "Position":
                return _stylePosition.HasLocalValue;
            case "CanResize":
                return _styleCanResize.HasLocalValue;
            case "CanMove":
                return _styleCanMove.HasLocalValue;
            case "CanClose":
                return _styleCanClose.HasLocalValue;
            case "TitlebarHeight":
                return _styleTitlebarHeight.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Title":
                if (_styleTitle.ClearLocal())
                    NotifyPropertyChanged(TitleChangedArgs);
                return;
            case "Position":
                if (_stylePosition.ClearLocal())
                    NotifyPropertyChanged(PositionChangedArgs);
                return;
            case "CanResize":
                if (_styleCanResize.ClearLocal())
                    NotifyPropertyChanged(CanResizeChangedArgs);
                return;
            case "CanMove":
                if (_styleCanMove.ClearLocal())
                    NotifyPropertyChanged(CanMoveChangedArgs);
                return;
            case "CanClose":
                if (_styleCanClose.ClearLocal())
                    NotifyPropertyChanged(CanCloseChangedArgs);
                return;
            case "TitlebarHeight":
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

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
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
