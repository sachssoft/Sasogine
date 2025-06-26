using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace sachssoft.Sasogine.Tiling.Stacked;

public unsafe struct FixedTileData32<TFlags> : ITileInstanceWithFlags<TFlags>, ITileInstanceWithData
    where TFlags : unmanaged, Enum
{
    private const int SIZE = 32;
    private uint _identifier;
    private TFlags _flags;
    private fixed byte _data[SIZE];

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

    public int DataLength => SIZE;

    // Wichtig: Um den Pointer außerhalb des fixed Blocks zu bekommen,
    // müssen wir eine Methode mit fixed verwenden, die den Pointer **nur temporär** benutzt.
    // Daher bieten wir hier einen Span an für sicheren Zugriff.
    public Span<byte> GetDataSpan()
    {
        fixed (byte* ptr = _data)
        {
            return new Span<byte>(ptr, DataLength);
        }
    }

    public void SetData(ReadOnlySpan<byte> data)
    {
        if (data.Length > DataLength)
            throw new ArgumentException($"Data too long, max {DataLength} bytes.");

        var span = GetDataSpan();
        data.CopyTo(span);

        // Optional: restlichen Speicher mit 0 füllen
        if (data.Length < DataLength)
        {
            span.Slice(data.Length).Clear();
        }
    }

    public void GetData(Span<byte> buffer)
    {
        if (buffer.Length > DataLength)
            throw new ArgumentException($"Buffer too large, max {DataLength} bytes.");

        var span = GetDataSpan();
        span.Slice(0, buffer.Length).CopyTo(buffer);
    }


    // Interface verlangt byte* DataPointer
    // Wir fixieren hier und geben Pointer zurück **nur** für kurzfristigen Zugriff
    byte* ITileInstanceWithData.DataPointer
    {
        get
        {
            fixed (byte* ptr = _data)
            {
                return ptr; // Gültig nur innerhalb des fixed-Scopes
            }
        }
    }

    bool IOutputTile.Equals(IOutputTile other)
    {
        if (other is not FixedTileData32<TFlags> otherTile)
            return false;

        if (_identifier != otherTile._identifier)
            return false;

        if (!EqualityComparer<TFlags>.Default.Equals(_flags, otherTile._flags))
            return false;

        var span_a = GetDataSpan();
        var span_b = otherTile.GetDataSpan();

        return span_a.SequenceEqual(span_b);
    }

    public override string ToString()
    {
        return $"FixedTileData32 [ID={Identifier}, Flags={Flags}]";
    }
}
