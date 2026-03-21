using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Graphics.Colors;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls;

public partial class Widget : ElementBase, ITransformable, IPresentationHostElement, ICloneable
{
    // Eigenschaften ohne Tag (kein Style-Bezug)
    private PopupBase? _popup;

    private bool _isLoaded = false;
    private Vector2? _startPos;
    private Point _startLeftTop;
    private Desktop _desktop;

    private Point _lastMeasureSize;
    private Point _lastMeasureAvailableSize;


    #region Events

    public event EventHandler? EffectiveEnabledChanged;
    public event EventHandler? Preparing;
    public event EventHandler? Loaded;
    public event EventHandler? Unloaded;
    public event EventHandler? ParentChanged;

    #endregion

    public Widget()
    {
        IsVisible = true;
        IsEnabled = true;
        DragHandle = this;

        //Debug.WriteLine(GetType());

        Children.CollectionChanged += ChildrenOnCollectionChanged;
    }

    #region Direct Properties 

    public bool IsLoaded => _isLoaded;

    public bool IsDraggable => DragDirection != Controls.DragDirection.None;

    public object? Tooltip { get; set; }

    public Widget TooltipPresenter { get; }

    protected PopupBase? Popup
    {
        get => _popup;
        set
        {
            if (_popup == value)
                return;

            OnPropertyChanging();

            if (_popup != null)
            {
                // Remove old owner link
                if (ReferenceEquals(_popup.Owner, this))
                    _popup.Owner = null;
            }

            if (value != null)
            {
                // Validate: a Popup can only be owned by one control at a time
                if (value.Owner != null && !ReferenceEquals(value.Owner, this))
                {
                    throw new InvalidOperationException(
                        $"The specified popup instance is already assigned to another control of type '{value.Owner.GetType().Name}'. " +
                        $"A popup can only be associated with one widget at a time."
                    );
                }

                value.Owner = this;
            }

            _popup = value;
            OnPropertyChanged();
        }
    }

    protected virtual Point PopupPosition
    {
        get => ToGlobal(new Point(0, Bounds.Height));
    }

    #endregion

    public Widget DragHandle { get; set; }

    /// <summary>
    /// Determines whether the widget had been placed on Desktop
    /// </summary>
    public bool IsPlaced
    {
        get
        {
            return Desktop != null;
        }
    }

    [AllowNull]
    public virtual Desktop Desktop
    {
        get
        {
            return _desktop;
        }

        internal set
        {
            if (_desktop != null && value == null)
            {
                if (_desktop.FocusedKeyboardWidget == this)
                {
                    _desktop.FocusedKeyboardWidget = null;
                }

                if (_desktop.Tooltip != null && _desktop.Tooltip.Tag == this)
                {
                    _desktop.HideTooltip();
                }
            }

            LocalMousePosition = null;
            LocalTouchPosition = null;

            _desktop = value;

            if (_desktop != null)
            {
                InvalidateMeasure();
            }

            SubscribeOnTouchMoved(IsPlaced && IsDraggable);

            foreach (var child in LayoutChildren)
            {
                child.Desktop = value;
            }
            OnPlacedChanged();
        }
    }

    #region Desktop Events



    #endregion

    //#region Modal

    //public bool IsModal { get; set; }

    //public IBrush? ModalBackground { get; set; }

    //#endregion

    public Widget? Parent
    {
        get => _parent;
        internal set
        {
            if (SetAndNotify(ref _parent, value))
            {
                OnParentChanged(EventArgs.Empty);
            }
        }
    }

    protected virtual void OnParentChanged(EventArgs e)
    {
        ParentChanged?.Invoke(this, e);
    }

    public bool IsKeyboardFocused
    {
        get
        {
            return _isKeyboardFocused;
        }

        internal set
        {
            if (value == _isKeyboardFocused)
            {
                return;
            }

            _isKeyboardFocused = value;
            KeyboardFocusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool _isEffectiveEnabled;

    public bool IsEffectiveEnabled => _isEffectiveEnabled;
    protected virtual bool ComputeEffectiveEnabled()
    {
        return IsEnabled.Value && (Parent?.IsEffectiveEnabled ?? true);
    }

    public void UpdateEffectiveEnabled()
    {
        bool newEffective = ComputeEffectiveEnabled();

        if (_isEffectiveEnabled != newEffective)
        {
            _isEffectiveEnabled = newEffective;
            OnIsEffectiveEnabledChanged(EventArgs.Empty);

            foreach (var child in LayoutChildren)
                child.UpdateEffectiveEnabled();
        }
    }

    protected virtual void OnIsEffectiveEnabledChanged(EventArgs e)
    {
        EffectiveEnabledChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual bool UseOverBackground => IsMouseInside;

    public void BringToFront()
    {
        var widgets = Parent != null ? Parent.Children : Desktop.Widgets;

        if (widgets[widgets.Count - 1] == this) return;

        widgets.Remove(this);
        widgets.Add(this);
    }

    public void BringToBack()
    {
        var widgets = Parent != null ? Parent.Children : Desktop.Widgets;

        if (widgets[widgets.Count - 1] == this) return;

        widgets.Remove(this);
        widgets.Insert(0, this);
    }

    protected void FireKeyDown(Keys k)
    {
        KeyDown?.Invoke(this, new(k));
    }

    public virtual void OnKeyDown(Keys k)
    {
        FireKeyDown(k);
    }

    public virtual void OnKeyUp(Keys k)
    {
        KeyUp?.Invoke(this, new Behaviors.GenericEventArgs<Keys>(k));
    }

    //public virtual void OnKeyPressed(Keys key)
    //{
    //    KeyPressed?.Invoke(this, key);
    //}

    public virtual void OnChar(char c)
    {
        Char?.Invoke(this, new Behaviors.GenericEventArgs<char>(c));
    }

    protected virtual void OnPlacedChanged()
    {
        PlacedChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnIsVisibleChanged(EventArgs e)
    {
        base.OnIsVisibleChanged(e);

        LocalMousePosition = null;
        LocalTouchPosition = null;

        InvalidateMeasure();
        VisibleChanged?.Invoke(this, EventArgs.Empty);

        // Fokus freigeben, falls Widget nicht mehr sichtbar ist
        if (!IsVisible && Desktop?.FocusedKeyboardWidget == this)
        {
            Desktop.FocusedKeyboardWidget = null;
        }
    }

    public virtual void OnKeyboardFocusLost()
    {
        IsKeyboardFocused = false;
        KeyboardFocusLost?.Invoke(this, EventArgs.Empty);
    }

    public virtual void OnKeyboardFocusGot()
    {
        IsKeyboardFocused = true;
        KeyboardFocusGot?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveFromParent()
    {
        if (Parent == null)
        {
            return;
        }

        Parent.Children.Remove(this);
    }

    public void RemoveFromDesktop()
    {
        Desktop.Widgets.Remove(this);
    }

    private void FireLocationChanged()
    {
        LocationChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FireSizeChanged()
    {
        SizeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetKeyboardFocus()
    {
        Desktop.FocusedKeyboardWidget = this;
    }

    public void RequestKeyboardFocus()
    {
        if (!AcceptsKeyboardFocus) return; // verhindert Fokusänderung bei Capture

        SetKeyboardFocus();
    }

    private void SubscribeOnTouchMoved(bool subscribe)
    {
        if (Parent != null)
        {
            Parent.TouchMoved -= DesktopOnTouchMoved;
            Parent.TouchUp -= DesktopTouchUp;
        }
        else if (Desktop != null)
        {
            Desktop.TouchMoved -= DesktopOnTouchMoved;
            Desktop.TouchUp -= DesktopTouchUp;
        }

        if (subscribe)
        {
            if (Parent != null)
            {
                Parent.TouchMoved += DesktopOnTouchMoved;
                Parent.TouchUp += DesktopTouchUp;
            }
            else if (Desktop != null)
            {
                Desktop.TouchMoved += DesktopOnTouchMoved;
                Desktop.TouchUp += DesktopTouchUp;
            }
        }
    }

    private void DesktopOnTouchMoved(object? sender, EventArgs args)
    {
        if (_startPos == null || !IsDraggable || Desktop == null)
        {
            return;
        }

        var parent = Parent != null ? (ITransformable)Parent : Desktop;
        var newPos = parent.ToLocal(
            new Vector2(
                Desktop.TouchPosition.GetValueOrDefault().X,
                Desktop.TouchPosition.GetValueOrDefault().Y
            )
        );
        var delta = newPos - _startPos.Value;

        var newLeft = Left;
        var newTop = Top;
        if (DragDirection.Value.HasFlag(Controls.DragDirection.Horizontal))
        {
            newLeft = _startLeftTop.X + (int)delta.X;
        }

        if (DragDirection.Value.HasFlag(Controls.DragDirection.Vertical))
        {
            newTop = _startLeftTop.Y + (int)delta.Y;
        }

        var parentBounds = Parent != null ? Parent.Bounds : Desktop.InternalBounds;
        if (newLeft < 0)
        {
            newLeft = 0;
        }

        if (newLeft + Bounds.Width > parentBounds.Width)
        {
            newLeft = parentBounds.Width - Bounds.Width;
        }

        if (newTop < 0)
        {
            newTop = 0;
        }

        if (newTop + Bounds.Height > parentBounds.Height)
        {
            newTop = parentBounds.Height - Bounds.Height;
        }

        Left = newLeft;
        Top = newTop;
    }

    protected virtual void OnTouchMoveCaptured(Point mousePos)
    {
        if (!IsDraggable || _startPos == null) return;

        var parent = Parent != null ? (ITransformable)Parent : Desktop;
        var newPos = parent.ToLocal(mousePos.ToVector2());
        var delta = newPos - _startPos.Value;

        if (DragDirection.Value.HasFlag(Controls.DragDirection.Horizontal))
            Left = _startLeftTop.X + (int)delta.X;

        if (DragDirection.Value.HasFlag(Controls.DragDirection.Vertical))
            Top = _startLeftTop.Y + (int)delta.Y;

        // Clamping
        var parentBounds = Parent?.Bounds ?? Desktop.InternalBounds;
        Left = Math.Clamp(Left, 0, parentBounds.Width - Bounds.Width);
        Top = Math.Clamp(Top, 0, parentBounds.Height - Bounds.Height);
    }

    internal protected virtual void OnMouseCaptureLost()
    {
    }

    internal protected virtual void OnMouseCaptured()
    {
    }

    private void DesktopTouchUp(object? sender, EventArgs args)
    {
        _startPos = null;
    }

    // Beispiel für HitTest
    public virtual Widget? HitTest(Point p)
    {
        if (Desktop == null || !IsVisible || !ContainsGlobalPoint(p) || !IsHitTestVisible)
        {
            return null;
        }

        Widget? result = null;
        for (var i = _layoutChildren.Count - 1; i >= 0; i--)
        {
            var child = _layoutChildren[i];
            result = child.HitTest(p);
            if (result != null) break;
        }

        var localPos = ToLocal(p);
        if (result == null && !InputFallsThrough(localPos))
        {
            result = this;
        }

        return result;
    }

    IPresentationHost IPresentationHostElement.Host => _desktop;

    public virtual bool InputFallsThrough(Point localPos) => false;

    internal void Initialize()
    {
        Preparing?.Invoke(this, EventArgs.Empty);
        EnsureLoadStyle();
        OnLoaded();
    }

    internal void Uninitialize()
    {
        OnUnloaded();
    }

    protected virtual void OnLoaded()
    {
        Loaded?.Invoke(this, EventArgs.Empty);
        _isLoaded = true;
    }

    protected virtual void OnUnloaded()
    {
        _isLoaded = false;
        Unloaded?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnIsEnabledChanged(EventArgs e)
    {
        base.OnIsEnabledChanged(e); 
        UpdateEffectiveEnabled();
    }
}