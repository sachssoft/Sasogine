using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;

namespace Sachssoft.Engine.Controls;

/// <summary>A text-style button with commands, optional navigation requests and a visited state.</summary>
/// <remarks>
/// Assign Content just as for Button; scalar values use a cached TextBlock presenter.
/// Activate through pointer, keyboard, PerformClick or poll IsPressed after Workspace.Update.
/// URI targets are dispatched through NavigateRequested; the host chooses how to navigate.
/// The control never starts an external application automatically.
/// </remarks>
public class Hyperlink : ButtonBase
{
    /// <summary>Caches the HoverTextColor notification.</summary>
    private static readonly PropertyChangedEventArgs HoverTextColorArgs = new(nameof(HoverTextColor));

    /// <summary>Caches visited-state notifications.</summary>
    private static readonly PropertyChangedEventArgs IsVisitedArgs = new(nameof(IsVisited));

    /// <summary>Caches navigation target notifications.</summary>
    private static readonly PropertyChangedEventArgs NavigateUriArgs = new(nameof(NavigateUri));

    /// <summary>Caches the PressedTextColor notification.</summary>
    private static readonly PropertyChangedEventArgs PressedTextColorArgs = new(nameof(PressedTextColor));

    /// <summary>Caches the Underline notification.</summary>
    private static readonly PropertyChangedEventArgs UnderlineArgs = new(nameof(Underline));

    /// <summary>Caches the VisitedTextColor notification.</summary>
    private static readonly PropertyChangedEventArgs VisitedTextColorArgs = new(nameof(VisitedTextColor));

    /// <summary>Stores the styled HoverTextColor and its optional local override.</summary>
    private StyleProperty<Color> _hoverTextColor = new(new Color(126, 200, 255));

    /// <summary>Stores the styled PressedTextColor and its optional local override.</summary>
    private StyleProperty<Color> _pressedTextColor = new(new Color(60, 130, 210));

    /// <summary>Stores the styled VisitedTextColor and its optional local override.</summary>
    private StyleProperty<Color> _visitedTextColor = new(new Color(178, 147, 232));

    /// <summary>Stores the styled Underline and its optional local override.</summary>
    private StyleProperty<bool> _underline = new(true);

    /// <summary>Stores the optional absolute or relative navigation target.</summary>
    private Uri? _navigateUri;

    /// <summary>Stores whether an activation has occurred since the last reset or target change.</summary>
    private bool _isVisited;

    /// <summary>Occurs after Clicked, command execution and the visited-state transition when NavigateUri is set.</summary>
    /// <remarks>The handler reads NavigateUri and performs platform-specific or internal navigation.</remarks>
    public event EventHandler? NavigateRequested;

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(Hyperlink);

    /// <summary>Gets or sets whether the link has been activated. Changing the target resets this state.</summary>
    public bool IsVisited { get => _isVisited; set => SetNotify(ref _isVisited, value, IsVisitedArgs); }

    /// <summary>Gets or sets an optional absolute or relative URI; null provides command-only activation.</summary>
    /// <remarks>Changing the original URI text resets IsVisited. The host handles NavigateRequested.</remarks>
    public Uri? NavigateUri
    {
        get => _navigateUri;
        set
        {
            if (_navigateUri?.OriginalString == value?.OriginalString)
                return;
            _navigateUri = value;
            IsVisited = false;
            NotifyPropertyChanged(NavigateUriArgs);
        }
    }

    /// <summary>Gets the current presentation color without changing layout or control properties.</summary>
    internal Color EffectiveTextColor => !IsAvailable() ? DisabledTextColor : IsHeld ? PressedTextColor : IsPointerOver || IsFocused ? HoverTextColor : IsVisited ? VisitedTextColor : TextColor;

    /// <summary>Gets or sets the text color while the link is hovered or keyboard-focused.</summary>
    public Color HoverTextColor
    {
        get => _hoverTextColor.Value;
        set
        {
            if (_hoverTextColor.SetLocal(value))
                NotifyPropertyChanged(HoverTextColorArgs);
        }
    }

    /// <summary>Gets or sets the text color while activation is held.</summary>
    public Color PressedTextColor
    {
        get => _pressedTextColor.Value;
        set
        {
            if (_pressedTextColor.SetLocal(value))
                NotifyPropertyChanged(PressedTextColorArgs);
        }
    }

    /// <summary>Gets or sets whether generated text content is underlined. Defaults to true.</summary>
    public bool Underline
    {
        get => _underline.Value;
        set
        {
            if (_underline.SetLocal(value))
                NotifyPropertyChanged(UnderlineArgs);
        }
    }

    /// <summary>Gets or sets the text color after activation when the link is neither hovered nor held.</summary>
    public Color VisitedTextColor
    {
        get => _visitedTextColor.Value;
        set
        {
            if (_visitedTextColor.SetLocal(value))
                NotifyPropertyChanged(VisitedTextColorArgs);
        }
    }

    /// <inheritdoc/>
    protected override TextBlock CreateTextPresenter(string text) => new HyperlinkTextPresenter(this)
    {
        Text = text
    };

    /// <summary>Activates the command, records the visited state and requests navigation for the current target.</summary>
    protected override void OnClick()
    {
        if (!IsAvailable() || !CanExecute)
            return;
        base.OnClick();
        IsVisited = true;
        if (NavigateUri is not null)
            NavigateRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Leaves text rendering to the presenter without drawing a button background.</summary>
    /// <param name = "context">The current drawing context.</param>
    protected override void OnDraw(ControlDrawContext context)
    {
    }

    /// <summary>Keeps appearance and navigation changes from invalidating cached geometry.</summary>
    /// <param name = "notification">The effective property change.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName is nameof(IsVisited) or nameof(NavigateUri) or nameof(HoverTextColor) or nameof(PressedTextColor) or nameof(VisitedTextColor) or nameof(Underline))
        {
            InvalidateVisual();
            return;
        }
        base.OnPropertyChanged(notification);
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(HoverTextColor):
                    if (_hoverTextColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(HoverTextColorArgs);
                    break;
                case nameof(PressedTextColor):
                    if (_pressedTextColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(PressedTextColorArgs);
                    break;
                case nameof(Underline):
                    if (_underline.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(UnderlineArgs);
                    break;
                case nameof(VisitedTextColor):
                    if (_visitedTextColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(VisitedTextColorArgs);
                    break;
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property) => property switch
    {
        nameof(HoverTextColor) => _hoverTextColor.HasLocalValue,
        nameof(PressedTextColor) => _pressedTextColor.HasLocalValue,
        nameof(Underline) => _underline.HasLocalValue,
        nameof(VisitedTextColor) => _visitedTextColor.HasLocalValue,
        _ => base.IsStyleLocal(property)};

    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(HoverTextColor):
                if (_hoverTextColor.ClearLocal())
                    NotifyPropertyChanged(HoverTextColorArgs);
                break;
            case nameof(PressedTextColor):
                if (_pressedTextColor.ClearLocal())
                    NotifyPropertyChanged(PressedTextColorArgs);
                break;
            case nameof(Underline):
                if (_underline.ClearLocal())
                    NotifyPropertyChanged(UnderlineArgs);
                break;
            case nameof(VisitedTextColor):
                if (_visitedTextColor.ClearLocal())
                    NotifyPropertyChanged(VisitedTextColorArgs);
                break;
            default:
                base.ResetStyleProperty(property);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _hoverTextColor.BeginStyle(new Color(126, 200, 255));
        _pressedTextColor.BeginStyle(new Color(60, 130, 210));
        _underline.BeginStyle(true);
        _visitedTextColor.BeginStyle(new Color(178, 147, 232));
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_hoverTextColor.CommitStyle())
            NotifyPropertyChanged(HoverTextColorArgs);
        if (_pressedTextColor.CommitStyle())
            NotifyPropertyChanged(PressedTextColorArgs);
        if (_underline.CommitStyle())
            NotifyPropertyChanged(UnderlineArgs);
        if (_visitedTextColor.CommitStyle())
            NotifyPropertyChanged(VisitedTextColorArgs);
    }
}
