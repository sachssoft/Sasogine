namespace Sachssoft.Engine.Controls;
/// <summary>Constrains the physical joystick direction.</summary>
public enum JoystickMovementMode
{
    /// <summary>All directions within a circle.</summary>
    Free,
    /// <summary>Left and right.</summary>
    Horizontal,
    /// <summary>Up and down.</summary>
    Vertical,
    /// <summary>Along a normalized custom axis.</summary>
    CustomAxis
}
