namespace Sachssoft.Engine.Controls.Input;
/// <summary>Stable pointer identity; touch IDs need not be distinct from the mouse ID.</summary>
public readonly record struct ControlPointer(ControlPointerKind Kind, int Id = 0)
{
    /// <summary>Gets the desktop mouse identity.</summary>
    public static ControlPointer Mouse => new(ControlPointerKind.Mouse);
}
