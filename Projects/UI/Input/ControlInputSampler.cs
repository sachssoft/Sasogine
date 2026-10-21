using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls.Input;
/// <summary>Converts caller-sampled device states into a stable UI event order.</summary>
/// <remarks>
/// Sample devices once in the application, then pass those snapshots here. Native
/// text events can be queued through AddText. For exact native event ordering or
/// replay, construct ControlInputFrame directly. Polling cannot observe clicks
/// occurring entirely between two samples. Keyboard repeat is not synthesized.
/// </remarks>
public sealed class ControlInputSampler
{
    private static readonly MouseButton[] PointerButtons =
    {
        MouseButton.Left,
        MouseButton.Right,
        MouseButton.Middle,
        MouseButton.XButton1,
        MouseButton.XButton2
    };
    private MouseState _mouse;
    private KeyboardState _keyboard;
    private readonly List<char> _text = new();
    private readonly Dictionary<int, Vector2> _touches = new();
    private readonly List<TouchLocation> _orderedTouches = new();
    private readonly HashSet<int> _seenTouches = new();
    /// <summary>Queues a text character on the UI thread for the next active frame.</summary>
    public void AddText(char character) => _text.Add(character);
    /// <summary>Creates logical events from one pair of device snapshots.</summary>
    public ControlInputFrame Sample(MouseState mouse, KeyboardState keyboard, Rectangle viewport, float scale, TimeSpan elapsed, bool active = true) => Sample(mouse, keyboard, default(TouchCollection), viewport, scale, elapsed, active);
    /// <summary>Creates ordered mouse, touch and keyboard events from caller-sampled snapshots.</summary>
    /// <remarks>Disable MonoGame touch-to-mouse emulation when supplying native touches, to avoid duplicate gestures.</remarks>
    public ControlInputFrame Sample(MouseState mouse, KeyboardState keyboard, TouchCollection touches, Rectangle viewport, float scale, TimeSpan elapsed, bool active = true) => SampleCore(mouse, keyboard, touches, viewport, scale, elapsed, active, false);
    private readonly List<ControlInputEvent> _events = new(16);
    private ControlInputFrame? _reusable;
    /// <summary>Samples into reusable storage. Consume immediately; do not retain this borrowed frame for replay.</summary>
    public ControlInputFrame SampleReusable(MouseState mouse, KeyboardState keyboard, TouchCollection touches, Rectangle viewport, float scale, TimeSpan elapsed, bool active = true) => SampleCore(mouse, keyboard, touches, viewport, scale, elapsed, active, true);
    private ControlInputFrame SampleCore(MouseState mouse, KeyboardState keyboard, TouchCollection touches, Rectangle viewport, float scale, TimeSpan elapsed, bool active, bool reuse)
    {
        if (!float.IsFinite(scale) || scale <= 0)
            throw new ArgumentOutOfRangeException(nameof(scale));
        if (viewport.Width < 0 || viewport.Height < 0)
            throw new ArgumentOutOfRangeException(nameof(viewport));
        var events = reuse ? _events : new List<ControlInputEvent>();
        events.Clear();
        Vector2 position = new(((float)mouse.X - viewport.X) / scale, ((float)mouse.Y - viewport.Y) / scale);
        if (!active)
        {
            events.Add(new ControlInputEvent(ControlInputEventType.Deactivate));
            _touches.Clear();
        }
        else
        {
            bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            bool control = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
            bool alt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
            // Include the position even if stationary: resize/DPI changes can move it logically.
            events.Add(new ControlInputEvent(ControlInputEventType.PointerMove, position));
            foreach (MouseButton button in PointerButtons)
            {
                bool wasDown = Down(_mouse, button), isDown = Down(mouse, button);
                if (wasDown != isDown)
                    events.Add(new ControlInputEvent(isDown ? ControlInputEventType.PointerDown : ControlInputEventType.PointerUp, position, button, Shift: shift, Control: control, Alt: alt));
            }

            int wheel = unchecked(mouse.ScrollWheelValue - _mouse.ScrollWheelValue);
            if (wheel != 0)
                events.Add(new ControlInputEvent(ControlInputEventType.PointerWheel, position, WheelDelta: wheel));
            SampleTouches(touches, viewport, scale, events);
            // Key codes have a bounded numeric range; scan deterministically without arrays or discovery.
            for (int keyCode = 0; keyCode <= 255; keyCode++)
            {
                Keys key = (Keys)keyCode;
                bool wasDown = _keyboard.IsKeyDown(key), isDown = keyboard.IsKeyDown(key);
                if (wasDown != isDown)
                    events.Add(new ControlInputEvent(isDown ? ControlInputEventType.KeyDown : ControlInputEventType.KeyUp, Key: key, Shift: shift, Control: keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl), Alt: keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt)));
            }

            foreach (char character in _text)
                events.Add(new ControlInputEvent(ControlInputEventType.TextInput, Character: character));
        }

        var result = reuse ? (_reusable ??= new ControlInputFrame(_events)).Reset(new Size2(viewport.Width / scale, viewport.Height / scale), elapsed) : new ControlInputFrame(new Size2(viewport.Width / scale, viewport.Height / scale), elapsed, events);
        _mouse = mouse;
        _keyboard = keyboard;
        _text.Clear();
        return result.SetViewport(viewport, scale);
    }

    private void SampleTouches(TouchCollection touches, Rectangle viewport, float scale, List<ControlInputEvent> events)
    {
        _orderedTouches.Clear();
        _seenTouches.Clear();
        foreach (var touch in touches)
            _orderedTouches.Add(touch);
        _orderedTouches.Sort(static (a, b) => a.Id.CompareTo(b.Id));
        foreach (var touch in _orderedTouches)
        {
            if (!_seenTouches.Add(touch.Id))
                throw new ArgumentException("Duplicate touch ID.", nameof(touches));
            Vector2 position = (touch.Position - new Vector2(viewport.X, viewport.Y)) / scale;
            bool known = _touches.ContainsKey(touch.Id);
            ControlInputEventType? type = touch.State switch
            {
                TouchLocationState.Pressed when !known => ControlInputEventType.PointerDown,
                TouchLocationState.Pressed or TouchLocationState.Moved when known => ControlInputEventType.PointerMove,
                TouchLocationState.Released when known => ControlInputEventType.PointerUp,
                _ => null
            };
            if (type.HasValue)
            {
                events.Add(new ControlInputEvent(type.Value, position, PointerId: touch.Id, PointerKind: ControlPointerKind.Touch));
                if (type == ControlInputEventType.PointerUp)
                    _touches.Remove(touch.Id);
                else
                    _touches[touch.Id] = position;
            }
            else if (known)
            {
                events.Add(new ControlInputEvent(ControlInputEventType.PointerCancel, _touches[touch.Id], PointerId: touch.Id, PointerKind: ControlPointerKind.Touch));
                _touches.Remove(touch.Id);
            }
        }

        _orderedMissing.Clear();
        foreach (int id in _touches.Keys)
            if (!_seenTouches.Contains(id))
                _orderedMissing.Add(id);
        _orderedMissing.Sort();
        foreach (int id in _orderedMissing)
        {
            events.Add(new ControlInputEvent(ControlInputEventType.PointerCancel, _touches[id], PointerId: id, PointerKind: ControlPointerKind.Touch));
            _touches.Remove(id);
        }
    }

    private readonly List<int> _orderedMissing = new();
    private static bool Down(MouseState state, MouseButton button) => button switch
    {
        MouseButton.Left => state.LeftButton == ButtonState.Pressed,
        MouseButton.Right => state.RightButton == ButtonState.Pressed,
        MouseButton.Middle => state.MiddleButton == ButtonState.Pressed,
        MouseButton.XButton1 => state.XButton1 == ButtonState.Pressed,
        MouseButton.XButton2 => state.XButton2 == ButtonState.Pressed,
        _ => false
    };
}
