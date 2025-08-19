using System;

namespace Sachssoft.Sasogine.Tiling.Stacked;

public unsafe interface ITileInstanceWithData : ITileInstance
{
    byte* DataPointer { get; }
    int DataLength { get; }
    void SetData(ReadOnlySpan<byte> data);
    void GetData(Span<byte> buffer);
}