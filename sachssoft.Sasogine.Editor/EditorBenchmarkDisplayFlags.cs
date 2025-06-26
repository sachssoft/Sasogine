using System;

namespace sachssoft.Sasogine.Views.Editor;

[Flags]
public enum EditorDiagnosticsDisplayFlags
{
    None = 0,
    FramePerSecond = 1 << 0,
    MapUpdate = 1 << 1,
    MapRender = 1 << 2,
    ToolCursor = 1 << 3,
    Surface = 1 << 4,
    TileIdentifier = 1 << 5,
    CameraPosition = 1 << 6,
    CameraZoom = 1 << 7
}