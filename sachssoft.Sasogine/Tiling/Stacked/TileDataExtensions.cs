using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace sachssoft.Sasogine.Tiling.Stacked;

public unsafe static class TileDataExtensions
{
    // Hole Daten als Byte-Array (Kopie)
    public static byte[] ToArray<TTileData>(this TTileData tile_data)
        where TTileData : struct, ITileInstanceWithData
    {
        if (tile_data.DataPointer == null || tile_data.DataLength == 0)
            return Array.Empty<byte>();

        var result = new byte[tile_data.DataLength];
        fixed (byte* destPtr = result)
        {
            Buffer.MemoryCopy(tile_data.DataPointer, destPtr, tile_data.DataLength, tile_data.DataLength);
        }
        return result;
    }

    // Setze Daten aus Byte-Array
    public static void FromArray<TTileData, TFlags>(this ref TTileData tile_data, ReadOnlySpan<byte> data)
        where TTileData : struct, ITileInstanceWithData
    {
        if (data.Length > tile_data.DataLength)
            throw new ArgumentException($"Data too large, max length is {tile_data.DataLength} bytes.");

        // Hole Zeiger und kopiere Daten
        fixed (byte* srcPtr = data)
        {
            Buffer.MemoryCopy(srcPtr, tile_data.DataPointer, tile_data.DataLength, data.Length);
        }

        // Rest nullen, wenn nötig
        if (tile_data.DataLength > data.Length)
        {
            Unsafe.InitBlock(tile_data.DataPointer + data.Length, 0, (uint)(tile_data.DataLength - data.Length));
        }
    }

    // Prüfe ob ein bestimmtes Flag gesetzt ist (vorausgesetzt Flags sind bitflags)
    public static bool HasFlag<TTileData, TFlags>(this TTileData tile_data, TFlags flag)
        where TTileData : unmanaged, ITileInstanceWithFlags<TFlags>
        where TFlags : unmanaged, Enum
    {
        var flagsValue = Convert.ToUInt64(tile_data.Flags);
        var flagValue = Convert.ToUInt64(flag);
        return (flagsValue & flagValue) == flagValue;
    }

    // Setze oder entferne Flag (vorausgesetzt Flags sind bitflags)
    public static void SetFlag<TTileData, TFlags>(this ref TTileData tile_data, TFlags flag, bool value)
        where TTileData : unmanaged, ITileInstanceWithFlags<TFlags>
        where TFlags : unmanaged, Enum
    {
        ulong flagsValue = Convert.ToUInt64(tile_data.Flags);
        ulong flagValue = Convert.ToUInt64(flag);

        if (value)
            flagsValue |= flagValue;
        else
            flagsValue &= ~flagValue;

        tile_data.Flags = (TFlags)Enum.ToObject(typeof(TFlags), flagsValue);
    }
}
