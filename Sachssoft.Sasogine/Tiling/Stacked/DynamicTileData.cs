using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sachssoft.Sasogine.Tiling.Stacked;

public unsafe struct DynamicTileData<TFlags> : ITileInstanceWithFlags<TFlags>, ITileInstanceWithData
    where TFlags : unmanaged, Enum
{
    private uint _identifier;
    private TFlags _flags;
    private byte* _data;
    private int _data_length;

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

    public byte* Data => _data;

    public int DataLength => _data_length;

    byte* ITileInstanceWithData.DataPointer => Data;

    bool IOutputTile.Equals(IOutputTile other)
    {
        if (other is not DynamicTileData<TFlags> otherTile)
            return false;

        if (_identifier != otherTile._identifier)
            return false;

        if (!EqualityComparer<TFlags>.Default.Equals(_flags, otherTile._flags))
            return false;

        if (_data_length != otherTile._data_length)
            return false;

        if (_data_length == 0)
            return true;

        return MemoryCompare(_data, otherTile._data, _data_length);
    }

    private static bool MemoryCompare(byte* a, byte* b, int length)
    {
        for (int i = 0; i < length; i++)
        {
            if (a[i] != b[i])
                return false;
        }
        return true;
    }

    public void Allocate(int size)
    {
        if (_data != null)
            Free();

        _data = (byte*)Marshal.AllocHGlobal(size);
        _data_length = size;

        Unsafe.InitBlock(_data, 0, (uint)size);
    }

    public void Free()
    {
        if (_data != null)
        {
            Marshal.FreeHGlobal((nint)_data);
            _data = null;
            _data_length = 0;
        }
    }

    public byte[] GetData()
    {
        if (_data == null || _data_length <= 0)
            return Array.Empty<byte>();

        var result = new byte[_data_length];
        fixed (byte* destPtr = result)
        {
            Buffer.MemoryCopy(_data, destPtr, _data_length, _data_length);
        }
        return result;
    }

    public void SetData(byte[] source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        Allocate(source.Length);
        fixed (byte* srcPtr = source)
        {
            Buffer.MemoryCopy(srcPtr, _data, _data_length, source.Length);
        }
    }

    void ITileInstanceWithData.GetData(Span<byte> buffer)
    {
        if (_data == null) throw new InvalidOperationException("Data pointer is null.");
        if (buffer.Length > _data_length)
            throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer is larger than data length.");

        fixed (byte* destPtr = buffer)
        {
            Buffer.MemoryCopy(_data, destPtr, buffer.Length, buffer.Length);
        }
    }

    void ITileInstanceWithData.SetData(ReadOnlySpan<byte> data)
    {
        if (_data == null || _data_length < data.Length)
        {
            Allocate(data.Length);
        }

        fixed (byte* srcPtr = data)
        {
            Buffer.MemoryCopy(srcPtr, _data, _data_length, data.Length);
        }

        if (_data_length > data.Length)
        {
            Unsafe.InitBlock(_data + data.Length, 0, (uint)(_data_length - data.Length));
        }
    }

    public override string ToString()
    {
        return $"DynamicTileData [ID={Identifier}, Flags={Flags}]";
    }
}
