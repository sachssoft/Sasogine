using Sachssoft.Sasogine.Tiling;
using Sachssoft.Sasogine.Tiling.Stacked;
using System;

namespace Sachssoft.Sasogine.Editor.Tiling;

public unsafe struct EditorTileInstance : ITileInstanceWithFlags<EditorTileInstanceFlags>
{
    private uint _identifier;
    private EditorTileInstanceFlags _flags;

    public uint Identifier
    {
        get => _identifier;
        set => _identifier = value;
    }

    public EditorTileInstanceFlags Flags
    {
        get => _flags;
        set => _flags = value;
    }

    public int DataLength => 0;

    public byte* DataPointer => null;

    public void GetData(Span<byte> buffer)
    {
        if (buffer.Length > 0)
            throw new InvalidOperationException("No data available.");
    }

    public void SetData(ReadOnlySpan<byte> data)
    {
        if (!data.IsEmpty)
            throw new InvalidOperationException("No data supported.");
    }

    public override string ToString()
    {
        return $"{nameof(EditorTileInstance)} [ID={Identifier}, Flags={Flags}]";
    }

    bool IOutputTile.Equals(IOutputTile other)
    {
        if (other is not EditorTileInstance otherTile)
            return false;

        return _identifier == otherTile._identifier &&
               _flags == otherTile._flags;
    }
}
