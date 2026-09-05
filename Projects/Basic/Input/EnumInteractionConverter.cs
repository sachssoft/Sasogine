using System;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Input;

internal static class EnumInteractionConverter<TEnum>
    where TEnum : struct, Enum
{
    private static readonly Type _underlyingType =
        Enum.GetUnderlyingType(typeof(TEnum));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ToUInt64(TEnum value)
    {
        if (_underlyingType == typeof(byte))
            return Unsafe.As<TEnum, byte>(ref value);

        if (_underlyingType == typeof(sbyte))
            return unchecked((ulong)Unsafe.As<TEnum, sbyte>(ref value));

        if (_underlyingType == typeof(short))
            return unchecked((ulong)Unsafe.As<TEnum, short>(ref value));

        if (_underlyingType == typeof(ushort))
            return Unsafe.As<TEnum, ushort>(ref value);

        if (_underlyingType == typeof(int))
            return unchecked((ulong)Unsafe.As<TEnum, int>(ref value));

        if (_underlyingType == typeof(uint))
            return Unsafe.As<TEnum, uint>(ref value);

        if (_underlyingType == typeof(long))
            return unchecked((ulong)Unsafe.As<TEnum, long>(ref value));

        if (_underlyingType == typeof(ulong))
            return Unsafe.As<TEnum, ulong>(ref value);

        throw new InvalidOperationException(
            $"Unsupported enum underlying type '{_underlyingType}'.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TEnum FromUInt64(ulong value)
    {
        if (_underlyingType == typeof(byte))
        {
            var converted = (byte)value;
            return Unsafe.As<byte, TEnum>(ref converted);
        }

        if (_underlyingType == typeof(sbyte))
        {
            var converted = unchecked((sbyte)value);
            return Unsafe.As<sbyte, TEnum>(ref converted);
        }

        if (_underlyingType == typeof(short))
        {
            var converted = unchecked((short)value);
            return Unsafe.As<short, TEnum>(ref converted);
        }

        if (_underlyingType == typeof(ushort))
        {
            var converted = (ushort)value;
            return Unsafe.As<ushort, TEnum>(ref converted);
        }

        if (_underlyingType == typeof(int))
        {
            var converted = unchecked((int)value);
            return Unsafe.As<int, TEnum>(ref converted);
        }

        if (_underlyingType == typeof(uint))
        {
            var converted = (uint)value;
            return Unsafe.As<uint, TEnum>(ref converted);
        }

        if (_underlyingType == typeof(long))
        {
            var converted = unchecked((long)value);
            return Unsafe.As<long, TEnum>(ref converted);
        }

        if (_underlyingType == typeof(ulong))
            return Unsafe.As<ulong, TEnum>(ref value);

        throw new InvalidOperationException(
            $"Unsupported enum underlying type '{_underlyingType}'.");
    }
}