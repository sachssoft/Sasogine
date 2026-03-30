using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Inspection;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Capture;
using Sachssoft.Sasogine.Surface.Controls.Focus;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Internals;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Sachssoft.Sasogine.Surface.Controls;

public partial class Desktop : NotifyObject, IPresentationHost, ITransformable, IDisposable, IInputEventsProcessor
{
    private readonly FocusManager _focusManager = new FocusManager();
    private readonly CaptureManager _captureManager = new CaptureManager();

    private InternalKeyboardManager _keyboardManager;
    private InternalPointerManager _pointerManager;

    private CultureInfo? _culture = CultureInfo.InvariantCulture;

    private readonly InputContext _inputContext = new InputContext();
    private readonly RenderContext _renderContext = new RenderContext();

    private bool _widgetsDirty = true;
    private Widget? _focusedKeyboardWidget;
    private Widget? _previousKeyboardFocus;

    //public bool HasExternalTextInput = false;

    private bool _isDisposed = false;
    private bool _view_set = true;

    public bool[] DownKeys => _downKeys;

    public int RepeatKeyDownStartInMs { get; set; } = 500;

    public int RepeatKeyDownInternalInMs { get; set; } = 50;


    internal InternalKeyboardManager KeyboardManager => _keyboardManager;


    public Game Game
    {
        get => _game!;
    }

    public Widget? FocusedKeyboardWidget
    {
        get { return _focusedKeyboardWidget; }

        set
        {
            if (value == _focusedKeyboardWidget)
            {
                return;
            }

            var oldValue = _focusedKeyboardWidget;
            if (oldValue != null)
            {
                if (WidgetLosingKeyboardFocus != null)
                {
                    var args = new CancellableEventArgs<Widget>(oldValue);
                    WidgetLosingKeyboardFocus(null, args);
                    if (oldValue.IsPlaced && args.Cancel)
                    {
                        return;
                    }
                }
            }

            _focusedKeyboardWidget = value;
            if (oldValue != null)
            {
                oldValue.OnKeyboardFocusLost();
            }

            if (_focusedKeyboardWidget != null)
            {
                _focusedKeyboardWidget.OnKeyboardFocusGot();
                WidgetGotKeyboardFocus?.Invoke(this, new(_focusedKeyboardWidget));
            }
        }
    }

    public bool IsMouseOverGUI
    {
        get
        {
            return IsPointOverGUI(MousePosition);
        }
    }

    public bool IsTouchOverGUI
    {
        get
        {
            return TouchPosition != null && IsPointOverGUI(TouchPosition.Value);
        }
    }

    public Action<Keys> KeyDownHandler;

    public Desktop()
    {
        Opacity = 1.0f;
        Widgets.CollectionChanged += WidgetsOnCollectionChanged;
        KeyDownHandler = OnKeyDown;

#if FNA
		TextInputEXT.TextInput += OnChar;
#endif

        //if (Stylesheet.Current.DesktopStyle != null)
        //{
        //    Background = Stylesheet.Current.DesktopStyle.Background;
        //}
    }

    public virtual void Initialize(Game game)
    {
        //base.Initialize(game);
        UIEnvironment.Game = game;
        _game = game;

        Stylesheet.Current = Stylesheet.LoadDefault(game.GraphicsDevice, StylesheetSize.Normal);

        _keyboardManager = new InternalKeyboardManager(game);
        _pointerManager = new InternalPointerManager();

        _keyboardManager.KeyDown += OnInternalKeyDown;
        _keyboardManager.KeyUp += OnInternalKeyUp;
        //_keyboardManager.KeyPressed += OnInternalKeyPressed;
        _keyboardManager.Char += OnInternalChar;

        _pointerManager.MouseMoved += pos => InputEventsManager.Queue(this, InputEventType.MouseMoved);
        _pointerManager.MouseLeftDown += pos => InputEventsManager.Queue(this, InputEventType.MouseLeft);
        _pointerManager.MouseLeftUp += pos => InputEventsManager.Queue(this, InputEventType.TouchUp);
        _pointerManager.MouseWheelChanged += delta => InputEventsManager.Queue(this, InputEventType.MouseWheel);

        _pointerManager.TouchDown += pos => InputOnTouchDown();
        _pointerManager.TouchDown += pos => InputEventsManager.Queue(this, InputEventType.TouchDown);
        _pointerManager.TouchUp += pos => InputEventsManager.Queue(this, InputEventType.TouchUp);
        _pointerManager.TouchMoved += pos => InputEventsManager.Queue(this, InputEventType.TouchMoved);
    }

    #region Pointer Management

    #endregion

    #region Key Management
    private void OnInternalKeyDown(Keys key)
    {
        // Direkt an Widgets weitergeben
        _focusedKeyboardWidget?.OnKeyDown(key);

        // z.B. Desktop-spezifische Shortcuts
        //if (key == Keys.Escape && _popup != null)
        //    ClosePopup();

        if (key == Keys.Escape && _popups.Count > 0)
            CloseAllPopups();

    }

    private void OnInternalKeyUp(Keys key)
    {
        _focusedKeyboardWidget?.OnKeyUp(key);
    }

    //private void OnInternalKeyPressed(Keys key)
    //{
    //    _focusedKeyboardWidget?.OnKeyPressed(key); // Optional, falls Widget das unterstützen soll
    //}

    private void OnInternalChar(char c)
    {
        _focusedKeyboardWidget?.OnChar(c);
    }

    #endregion

    public bool IsKeyDown(Keys keys)
    {
        return _downKeys[(int)keys];
    }

    public Widget GetChild(int index)
    {
        return LayoutChildren[index];
    }

    private void InputOnTouchDown()
    {
        // Popups rückwärts prüfen, oberstes zuerst
        var tempStack = new Stack<PopupBase>();

        while (_popups.Count > 0)
        {
            var popup = _popups.Pop();

            if (!popup.Content.IsTouchInside)
            {
                var eventArgs = new CancellableEventArgs<Widget>(popup.Content);
                popup.OnClosing(eventArgs);

                if (eventArgs.Cancel)
                {
                    tempStack.Push(popup); // bleibt offen
                    continue;
                }

                // Popup schließen
                Widgets.Remove(popup.Content);
                popup.Content.IsVisible = false;
                popup.OnClosed(EventArgs.Empty);
            }
            else
            {
                tempStack.Push(popup); // Popup innen berührt → offen lassen
            }
        }

        // Stack wiederherstellen
        while (tempStack.Count > 0)
        {
            _popups.Push(tempStack.Pop());
        }
        /////////////////////////////////////

        HideTooltip();
    }

    private void FixOverWidgetPosition(Widget widget, Point position)
    {
        widget.HorizontalAlignment = HorizontalAlignment.Left;
        widget.VerticalAlignment = VerticalAlignment.Top;

        var measure = widget.Measure(LayoutBounds.Size());

        if (position.X + measure.X > LayoutBounds.Right)
        {
            position.X = LayoutBounds.Right - measure.X;
        }

        if (position.Y + measure.Y > LayoutBounds.Bottom)
        {
            position.Y = LayoutBounds.Bottom - measure.Y;
        }

        widget.Left = position.X;
        widget.Top = position.Y;
    }

    internal void ProcessWidgets(Func<Widget, bool> operation)
    {
        foreach (var w in LayoutChildren)
        {
            var result = w.ProcessWidgets(operation);
            if (!result)
            {
                return;
            }
        }
    }

    private void FocusNextWidget()
    {
        if (Widgets.Count == 0) return;

        var isNull = FocusedKeyboardWidget == null;
        var focusChanged = false;
        ProcessWidgets(w =>
        {
            if (isNull)
            {
                if (CanFocusWidget(w))
                {
                    w.SetKeyboardFocus();
                    focusChanged = true;
                    return false;
                }
            }
            else
            {
                if (w == FocusedKeyboardWidget)
                {
                    isNull = true;
                    // Next widget will be focused
                }
            }

            return true;
        });

        if (focusChanged || FocusedKeyboardWidget == null)
        {
            // Either new focus had been set or there are no focusable widgets
            return;
        }

        // Next run - try to focus first widget before focused one
        ProcessWidgets(w =>
        {
            if (CanFocusWidget(w))
            {
                w.SetKeyboardFocus();
                return false;
            }

            return true;
        });
    }

    private static bool CanFocusWidget(Widget widget) =>
        widget != null && widget.IsVisible &&
        widget.IsEnabled && widget.AcceptsKeyboardFocus;

    public void OnKeyDown(Keys key)
    {
        KeyDown?.Invoke(this, new(key));

        if (_focusedKeyboardWidget != null)
        {
            _focusedKeyboardWidget.OnKeyDown(key);

            Debug.WriteLine("Key: " + UIEnvironment.TextInputState.Key);
            OnChar(UIEnvironment.TextInputState.Character);
        }

        //if (key == Keys.Escape && _popup != null)
        //{
        //    ClosePopup();
        //}

        if (key == Keys.Escape && _popups.Count > 0)
        {
            ClosePopup();
        }
    }

    public void OnChar(char c)
    {
        Debug.WriteLine("On Char: " + UIEnvironment.TextInputState.Character);

        if (_focusedKeyboardWidget != null)
        {
            _focusedKeyboardWidget.OnChar(c);
        }

        Char?.Invoke(this, new(c));
    }

    public bool IsPointOverGUI(Point p) => IsPointOverGUI(p, out _);

    public bool IsPointOverGUI(Point p, out Widget? w)
    {
        w = null;
        foreach (var widget in LayoutChildren)
        {
            var result = widget.HitTest(p);
            if (result != null)
            {
                w = widget;
                return true;
            }
        }

        return false;
    }

    private void ReleaseUnmanagedResources()
    {
        _renderContext.Dispose();
    }

    public virtual void Dispose()
    {
        if (_isDisposed)
            return;

#if FNA
		TextInputEXT.TextInput -= OnChar;
#endif

        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Desktop()
    {
        ReleaseUnmanagedResources();
    }
}