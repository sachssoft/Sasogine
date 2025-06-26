using sachssoft.Sasogine.Tiling;
using sachssoft.Sasogine.Tiling.Stacked;
using System;
using System.Collections.Generic;

// TileData ohne Daten
public unsafe struct TileInstance<TFlags> : ITileInstanceWithFlags<TFlags> where TFlags : unmanaged, Enum
{
    private uint _identifier;
    private TFlags _flags;

    public uint Identifier
    {
        get => _identifier;
        set => _identifier = value;
    }

    public TFlags Flags
    {
        get => _flags;
        set => _flags = value;
    }

    bool IOutputTile.Equals(IOutputTile other)
    {
        if (other is not TileInstance<TFlags> otherTile)
            return false;

        return _identifier == otherTile._identifier &&
               EqualityComparer<TFlags>.Default.Equals(_flags, otherTile._flags);
    }

}
