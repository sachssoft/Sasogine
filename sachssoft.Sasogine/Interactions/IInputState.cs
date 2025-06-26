using System;

namespace sachssoft.Sasogine.Interactions;

/// <summary>
/// Interface für Eingabestatus (Buttons gedrückt oder nicht).
/// </summary>
public interface IInputState<TButton> where TButton : struct, Enum
{
    bool IsButtonDown(TButton button);
    bool IsButtonUp(TButton button);
}
