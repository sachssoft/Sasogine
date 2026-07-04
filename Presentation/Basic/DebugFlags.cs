using System;

namespace Sachssoft.Sasogine.Presentation
{
    [Flags]
    public enum DebugFlags
    {
        None = 0,      // kein Debug
        FPS = 1 << 0, // FPS-Anzeige
        BoundsBorder = 1 << 1, // Rahmen um Frames
        ContentBorder = 1 << 2 // Inhaltliche Grenzen
    }
}
