using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Interactions;

public class MouseInteractionManager : InputInteractionManager<MouseButton>
{
    private readonly Dictionary<MouseWheelState, (Action press_action, Action? release_action)> _wheel_actions = new();
    private MouseWheelState _active_wheel_state = MouseWheelState.None;
    private int _previous_scroll_value = 0;
    private Point _current_position;
    private Point _last_position;
    private Point _delta;

    public MouseInteractionManager(MouseState initial_state)
        : base(new MouseStateWrapper(initial_state))
    {
    }

    public MouseInteractionManager()
        : base(new MouseStateWrapper(Mouse.GetState()))
    {
    }

    /// <summary>Aktuelle absolute Mausposition</summary>
    public Point Position => _current_position;

    /// <summary>Veränderung der Mausposition seit letztem Update</summary>
    public Point Delta => _delta;

    /// <summary>X-Achsen-Delta</summary>
    public int DeltaX => _delta.X;

    /// <summary>Y-Achsen-Delta</summary>
    public int DeltaY => _delta.Y;

    public void AddWheel(MouseWheelState wheel_state, Action press_action, Action? release_action = null)
    {
        _wheel_actions[wheel_state] = (press_action, release_action);
    }

    //public override void Update(GameContext context)
    //{
    //    var elapsed = context.GameTime.ElapsedGameTime;
    //    Update(Mouse.GetState(), elapsed);
    //}

    public override void Update(GameTime gameTime)
    {
        Update(Mouse.GetState(), gameTime.ElapsedGameTime);
    }

    public void Update(MouseState state, TimeSpan elapsed)
    {
        var current_state_wrapper = new MouseStateWrapper(state);
        Update(current_state_wrapper, elapsed);

        // Positionen aktualisieren
        _current_position = state.Position;
        _delta = _current_position - _last_position;
        _last_position = _current_position;

        // Wheel-Logik
        int current_scroll_value = state.ScrollWheelValue;
        int delta = current_scroll_value - _previous_scroll_value;

        MouseWheelState newWheelState = MouseWheelState.None;
        if (delta > 0)
            newWheelState = MouseWheelState.Up;
        else if (delta < 0)
            newWheelState = MouseWheelState.Down;

        if (newWheelState != _active_wheel_state)
        {
            // Release alten Zustand, falls gesetzt
            if (_active_wheel_state != MouseWheelState.None && _wheel_actions.TryGetValue(_active_wheel_state, out var oldActions))
            {
                oldActions.release_action?.Invoke();
            }

            // Press neuen Zustand, falls gesetzt und nicht None
            if (newWheelState != MouseWheelState.None && _wheel_actions.TryGetValue(newWheelState, out var newActions))
            {
                newActions.press_action.Invoke();
            }

            _active_wheel_state = newWheelState;
        }
        else if (newWheelState == MouseWheelState.None && _active_wheel_state != MouseWheelState.None)
        {
            // Release, wenn Wheel jetzt None ist und vorher aktiv
            if (_wheel_actions.TryGetValue(_active_wheel_state, out var releaseActions))
            {
                releaseActions.release_action?.Invoke();
            }
            _active_wheel_state = MouseWheelState.None;
        }

        _previous_scroll_value = current_scroll_value;
    }
}