using System;

namespace Sachssoft.Sasogine.Editor.Tiling;

[Flags]
public enum EditorTileInstanceFlags : uint
{
    None = 0,
    IsVisibled = 1,
    IsLocked = 2,
    IsSelected = 4
}
