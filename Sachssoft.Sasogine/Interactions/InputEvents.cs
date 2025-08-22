using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Interactions;

[Obsolete]
public sealed class InputEvents
{
    private float _delta_wheel_mouse;
    private float _current_wheel_value;

    public KeyboardState Keyboard { get; private set; }
    public GamePadState[] GamePads { get; private set; } = new GamePadState[4];
    public MouseState Mouse { get; private set; }

    internal InputEvents()
    {
        Keyboard = new KeyboardState();
        Mouse = new MouseState();
        for (int i = 0; i < GamePads.Length; i++)
            GamePads[i] = GamePad.GetState(i);
    }

    public GamePadState GetGamePad(PlayerIndex player_index)
    {
        return GamePads[(int)player_index];
    }

    public float MouseWheelDelta => _delta_wheel_mouse;

    public void Update(GameContext context)
    {
        Keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();

        for (int i = 0; i < GamePads.Length; i++)
            GamePads[i] = GamePad.GetState(i);

        UpdateWheel();
    }

    private void UpdateWheel()
    {
        _delta_wheel_mouse = Mouse.ScrollWheelValue - _current_wheel_value;
        _current_wheel_value = Mouse.ScrollWheelValue;
    }

    public bool IsGamePadConnected(int index, out AllowedGamePadTypes? type)
    {
        type = null;

        if (index < 0 || index >= GamePads.Length)
            return false;

        var c = GamePad.GetCapabilities(index);
        if (!c.IsConnected)
            return false;

        switch (c.GamePadType)
        {
            case GamePadType.GamePad:
                type = AllowedGamePadTypes.GamePad;
                return true;
            case GamePadType.ArcadeStick:
                type = AllowedGamePadTypes.ArcadeStick;
                return true;
            default:
                type = AllowedGamePadTypes.Unknown;
                return false;
        }
    }

    public bool IsGamePadButtonPressed(int index, Buttons button)
    {
        if (index < 0 || index >= GamePads.Length)
            return false;

        if (IsGamePadConnected(index, out _) == false)
            return false;

        return GamePads[index].IsButtonDown(button);
    }

    public IEnumerable<int> GetConnectedGamePadIndices()
    {
        for (int i = 0; i < GamePads.Length; i++)
        {
            if (IsGamePadConnected(i, out _))
                yield return i;
        }
    }

    public bool IsAnyKeyDown()
    {
        return Keyboard.GetPressedKeys().Length > 0;
    }

    public bool IsAnyGamePadButtonDown(int index)
    {
        if (index < 0 || index >= GamePads.Length)
            return false;

        if (!IsGamePadConnected(index, out _))
            return false;

        var state = GamePads[index];

        // Prüfe alle Buttons (Beispiel-Check, je nach Bedarf erweitern)
        foreach (Buttons button in Enum.GetValues<Buttons>())
        {
            if (state.IsButtonDown(button))
                return true;
        }
        return false;
    }

    public bool IsAnyMouseButtonDown()
    {
        return Mouse.LeftButton == ButtonState.Pressed
            || Mouse.RightButton == ButtonState.Pressed
            || Mouse.MiddleButton == ButtonState.Pressed
            || Mouse.XButton1 == ButtonState.Pressed
            || Mouse.XButton2 == ButtonState.Pressed;
    }

    public Vector2 GetGamePadLeftThumbstick(int index)
    {
        if (index < 0 || index >= GamePads.Length)
            return Vector2.Zero;

        if (!IsGamePadConnected(index, out _))
            return Vector2.Zero;

        return GamePads[index].ThumbSticks.Left;
    }

    public Vector2 GetGamePadRightThumbstick(int index)
    {
        if (index < 0 || index >= GamePads.Length)
            return Vector2.Zero;

        if (!IsGamePadConnected(index, out _))
            return Vector2.Zero;

        return GamePads[index].ThumbSticks.Right;
    }

    public float GetGamePadLeftTrigger(int index)
    {
        if (index < 0 || index >= GamePads.Length)
            return 0f;

        if (!IsGamePadConnected(index, out _))
            return 0f;

        return GamePads[index].Triggers.Left;
    }

    public float GetGamePadRightTrigger(int index)
    {
        if (index < 0 || index >= GamePads.Length)
            return 0f;

        if (!IsGamePadConnected(index, out _))
            return 0f;

        return GamePads[index].Triggers.Right;
    }
}
