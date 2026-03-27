using System;

namespace Sachssoft.Sasogine.Tiling.Stacked;

public unsafe interface ITileInstanceWithFlags<TFlags> : ITileInstance where TFlags : unmanaged, Enum
{
    TFlags Flags { get; set; }
}
