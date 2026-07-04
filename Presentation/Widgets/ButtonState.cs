using System;

namespace Sachssoft.Sasogine.Presentation.Widgets;

[Flags]
public enum ButtonState
{
    None = 0,      // keine Interaktion
    Hovered = 1 << 0, // Maus oder Touch über Button
    Pressed = 1 << 1, // dauerhaft gedrückt
    Released = 1 << 2, // gerade losgelassen
    Clicked = 1 << 3, // einmaliger Klick (Edge-trigger)
    DoubleClicked = 1 << 4, // Doppelklick (Edge-trigger)
    PointerEnter = 1 << 5, // Pointer ist gerade reingekommen
    PointerLeave = 1 << 6  // Pointer ist gerade rausgegangen
}
