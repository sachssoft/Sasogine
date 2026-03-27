using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals.Controls;

namespace Sachssoft.Sasogine.Surface.Controls;

partial class Widget : IInputEventsProcessor
{
    private DateTime? _lastTouchDown;
    private DateTime? _lastMouseMovement;
    private Point _lastLocalTouchPosition;
    private Point? _localMousePosition;
    private Point? _localTouchPosition;

    #region Events

    public event EventHandler? PlacedChanged;
    public event EventHandler? VisibleChanged;
    public event EventHandler? IsEnabledChanged;

    public event EventHandler? LocationChanged;
    public event EventHandler? SizeChanged;
    public event EventHandler? ArrangeUpdated;

    public event EventHandler? MouseLeft;
    public event EventHandler? MouseEntered;
    public event EventHandler? MouseMoved;

    // Desktop-Bezogen
    public event EventHandler? HostMouseMoved;

    public event EventHandler? TouchLeft;
    public event EventHandler? TouchEntered;
    public event EventHandler? TouchMoved;
    public event EventHandler? TouchDown;
    public event EventHandler? TouchUp;
    public event EventHandler? TouchDoubleClick;

    // Desktop-Bezogen
    public event EventHandler? HostTouchMoved;
    public event EventHandler? HostTouchDown;
    public event EventHandler? HostTouchUp;

    public event EventHandler? KeyboardFocusChanged;
    public event EventHandler? KeyboardFocusGot;
    public event EventHandler? KeyboardFocusLost;

    public event EventHandler<GenericEventArgs<float>>? MouseWheelChanged;

    public event EventHandler<GenericEventArgs<Keys>>? KeyUp;
    public event EventHandler<GenericEventArgs<Keys>>? KeyDown;
    public event EventHandler<GenericEventArgs<char>>? Char;

    #endregion

    [Browsable(false)]
    [XmlIgnore]
    public bool IsMouseInside => _localMousePosition != null;

    [Browsable(false)]
    [XmlIgnore]
    public Point? LocalMousePosition
    {
        get => _localMousePosition;
        private set
        {
            if (value == _localMousePosition)
            {
                return;
            }

            var oldValue = _localMousePosition;
            _localMousePosition = value;

            if (Desktop == null)
            {
                return;
            }

            if (value != null && oldValue == null)
            {
                InputEventsManager.Queue(this, InputEventType.MouseEntered);
            }
            else if (value == null && oldValue != null)
            {
                InputEventsManager.Queue(this, InputEventType.MouseLeft);
            }
            else if (value != null && oldValue != null && value.Value != oldValue.Value)
            {
                InputEventsManager.Queue(this, InputEventType.MouseMoved);
            }
        }
    }

    [Browsable(false)]
    [XmlIgnore]
    public bool IsTouchInside => _localTouchPosition != null;

    [Browsable(false)]
    [XmlIgnore]
    public Point? LocalTouchPosition
    {
        get => _localTouchPosition;
        private set
        {
            if (value == _localTouchPosition)
            {
                return;
            }

            var oldValue = _localTouchPosition;
            _localTouchPosition = value;

            if (Desktop == null)
            {
                return;
            }

            if (value != null && oldValue == null)
            {
                if (Desktop.PreviousTouchPosition == null)
                {
                    // Touch Down Event
                    InputEventsManager.Queue(this, InputEventType.TouchDown);
                    ProcessDoubleClick(value.Value);
                }
                else
                {
                    // Touch Entered
                    InputEventsManager.Queue(this, InputEventType.TouchEntered);
                }
            }
            else if (value == null && oldValue != null)
            {
                if (Desktop.TouchPosition == null)
                {
                    InputEventsManager.Queue(this, InputEventType.TouchUp);
                }
                else
                {
                    InputEventsManager.Queue(this, InputEventType.TouchLeft);
                }
            }
            else if (value != null && oldValue != null && value.Value != oldValue.Value)
            {
                InputEventsManager.Queue(this, InputEventType.TouchMoved);
            }
        }
    }

    protected internal virtual bool AcceptsMouseWheel => false;

    private void ProcessDoubleClick(Point touchPos)
    {
        if (_lastTouchDown != null &&
            (DateTime.Now - _lastTouchDown.Value).TotalMilliseconds < UIEnvironment.DoubleClickIntervalInMs &&
            int.Abs(touchPos.X - _lastLocalTouchPosition.X) <= UIEnvironment.DoubleClickRadius &&
            int.Abs(touchPos.Y - _lastLocalTouchPosition.Y) <= UIEnvironment.DoubleClickRadius)
        {
            _lastTouchDown = null;
            InputEventsManager.Queue(this, InputEventType.TouchDoubleClick);
        }
        else
        {
            _lastTouchDown = DateTime.Now;
            _lastLocalTouchPosition = LocalTouchPosition.Value;
        }
    }

    protected internal virtual void ProcessInput(InputContext inputContext)
    {
        if (!IsVisible || Desktop == null)
        {
            return;
        }

        if (!inputContext.MouseOrTouchHandled)
        {
            var oldContainsMouse = inputContext.ParentContainsMouse;
            var oldContainsTouch = inputContext.ParentContainsTouch;

            if (!UIEnvironment.IsMobile)
            {
                if (inputContext.ParentContainsMouse)
                {
                    if (ContainsGlobalPoint(Desktop.MousePosition))
                    {
                        LocalMousePosition = ToLocal(Desktop.MousePosition);
                    }
                    else
                    {
                        LocalMousePosition = null;
                        inputContext.ParentContainsMouse = false;
                    }
                }
                else
                {
                    LocalMousePosition = null;
                }
            }

            if (Desktop.TouchPosition != null && inputContext.ParentContainsTouch)
            {
                if (ContainsGlobalPoint(Desktop.TouchPosition.Value))
                {
                    LocalTouchPosition = ToLocal(Desktop.TouchPosition.Value);
                }
                else
                {
                    LocalTouchPosition = null;
                    inputContext.ParentContainsTouch = false;
                }
            }
            else
            {
                LocalTouchPosition = null;
            }

            if (IsMouseInside &&
                !Desktop.MouseWheelDelta.IsZero() &&
                AcceptsMouseWheel)
            {
                inputContext.MouseWheelWidget = this;
            }

            for (var i = _layoutChildren.Count - 1; i >= 0; i--)
            {
                var child = _layoutChildren[i];
                child.ProcessInput(inputContext);
            }

            if (this is IModalContent)
            {
                // Modal widget prevents all further input processing
                inputContext.MouseOrTouchHandled = true;
            }
            else
            {
                if (!UIEnvironment.IsMobile)
                {
                    if (IsMouseInside && !InputFallsThrough(LocalMousePosition.Value))
                    {
                        inputContext.MouseOrTouchHandled = true;
                    }
                }
                else
                {
                    if (IsTouchInside && !InputFallsThrough(LocalTouchPosition.Value))
                    {
                        inputContext.MouseOrTouchHandled = true;
                    }
                }
            }

            inputContext.ParentContainsMouse = oldContainsMouse;
            inputContext.ParentContainsTouch = oldContainsTouch;
        }
        else
        {
            if (!UIEnvironment.IsMobile)
            {
                LocalMousePosition = null;
            }

            LocalTouchPosition = null;

            for (var i = _layoutChildren.Count - 1; i >= 0; i--)
            {
                var child = _layoutChildren[i];
                child.ProcessInput(inputContext);
            }
        }
    }

    void IInputEventsProcessor.ProcessEvent(InputEventType eventType)
    {
        // It's important to note that widget should process input events even if Desktop is null
        // Just add corresponding null checks in that case

        switch (eventType)
        {
            case InputEventType.MouseLeft:
                if (Desktop != null && Desktop.Tooltip != null && Desktop.Tooltip.Tag == this)
                {
                    // Tooltip for this widget is shown
                    Desktop.HideTooltip();
                }

                _lastMouseMovement = null;

                if (UIEnvironment.SetMouseCursorFromWidget && MouseCursor != null)
                {
                    Widget ancestor = Parent;
                    while (ancestor != null && !ancestor.IsMouseInside)
                    {
                        ancestor = ancestor.Parent;
                    }

                    if (ancestor != null && ancestor.MouseCursor != null)
                    {
                        UIEnvironment.MouseCursorType = ancestor.MouseCursor.Value.GetValueOrDefault();
                    }
                    else
                    {
                        UIEnvironment.MouseCursorType = UIEnvironment.DefaultMouseCursorType;
                    }
                }

                OnMouseLeft();
                MouseLeft?.Invoke(this, EventArgs.Empty);
                break;
            case InputEventType.MouseEntered:
                _lastMouseMovement = DateTime.Now;
                if (UIEnvironment.SetMouseCursorFromWidget && MouseCursor != null)
                {
                    UIEnvironment.MouseCursorType = MouseCursor.Value.GetValueOrDefault();
                }

                OnMouseEntered();
                MouseEntered?.Invoke(this, EventArgs.Empty);
                break;
            case InputEventType.MouseMoved:
                _lastMouseMovement = DateTime.Now;
                OnMouseMoved();
                MouseMoved?.Invoke(this, EventArgs.Empty);
                break;
            case InputEventType.MouseWheel:
                if (Desktop != null)
                {
                    OnMouseWheel(Desktop.MouseWheelDelta);

                    // Add yet another null check, since OnMouseWheel call might nullify the Desktop
                    if (Desktop != null)
                    {
                        MouseWheelChanged?.Invoke(this, new(Desktop.MouseWheelDelta));
                    }
                }
                break;
            case InputEventType.TouchLeft:
                OnTouchLeft();
                TouchLeft?.Invoke(this, EventArgs.Empty);
                break;
            case InputEventType.TouchEntered:
                OnTouchEntered();
                TouchEntered?.Invoke(this, EventArgs.Empty);
                break;
            case InputEventType.TouchMoved:
                OnTouchMove();
                TouchMoved?.Invoke(this, EventArgs.Empty);
                break;
            case InputEventType.TouchDown:
                if (Desktop != null)
                {
                    if (IsEnabled && AcceptsKeyboardFocus)
                    {
                        Desktop.FocusedKeyboardWidget = this;
                    }

                    if (DragHandle != null && DragHandle.IsTouchInside)
                    {
                        var parent = Parent != null ? (ITransformable)Parent : Desktop;
                        _startPos = parent.ToLocal(new Vector2(Desktop.TouchPosition.Value.X, Desktop.TouchPosition.Value.Y));
                        _startLeftTop = new Point(Left, Top);
                    }
                }

                OnTouchDown();
                TouchDown?.Invoke(this, EventArgs.Empty);
                break;
            case InputEventType.TouchUp:
                OnTouchUp();
                TouchUp?.Invoke(this, EventArgs.Empty);
                break;
            case InputEventType.TouchDoubleClick:
                OnTouchDoubleClick();
                TouchDoubleClick?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    internal protected virtual void OnMouseLeft()
    {
    }

    internal protected virtual void OnMouseEntered()
    {
    }

    internal protected virtual void OnMouseMoved()
    {
    }

    internal protected virtual void OnMouseWheel(float delta)
    {
    }

    internal protected virtual void OnTouchLeft()
    {
    }

    internal protected virtual void OnTouchEntered()
    {
    }

    internal protected virtual void OnTouchMove()
    {
    }

    internal protected virtual void OnTouchDown()
    {
    }

    internal protected virtual void OnTouchUp()
    {
    }

    internal protected virtual void OnTouchDoubleClick()
    {
    }

    // Desktop-Bezogen
    internal protected virtual void OnHostMouseMoved(EventArgs e)
    {
        HostMouseMoved?.Invoke(this, EventArgs.Empty);
    }

    internal protected virtual void OnHostTouchMoved(EventArgs e)
    {
        HostTouchMoved?.Invoke(this, EventArgs.Empty);
    }

    internal protected virtual void OnHostTouchDown(EventArgs e)
    {
        HostTouchDown?.Invoke(this, EventArgs.Empty);
    }

    internal protected virtual void OnHostTouchUp(EventArgs e)
    {
        HostTouchUp?.Invoke(this, EventArgs.Empty);
    }
}