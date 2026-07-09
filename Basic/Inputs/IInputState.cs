using System;

namespace Sachssoft.Sasogine.Input
{

    /// <summary>
    /// Interface für Eingabestatus (Buttons gedrückt oder nicht).
    /// </summary>
    public interface IInputState<TButton> where TButton : struct, Enum
    {
        bool IsButtonDown(TButton button);
        bool IsButtonUp(TButton button);
    }
}