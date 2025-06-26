using System;
using System.Formats.Tar;

namespace sachssoft.Sasogine.Document;

public abstract class FormatWriterBase
{
    protected FormatWriterBase()
    {
    }

    public void WriteBoolean(bool value) => WriteBoolean(null, value);
    public void WriteByte(byte value) => WriteByte(null, value);
    public void WriteSByte(sbyte value) => WriteSByte(null, value);
    public void WriteInt16(short value) => WriteInt16(null, value);
    public void WriteUInt16(ushort value) => WriteUInt16(null, value);
    public void WriteInt32(int value) => WriteInt32(null, value);
    public void WriteUInt32(uint value) => WriteUInt32(null, value);
    public void WriteInt64(long value) => WriteInt64(null, value);
    public void WriteUInt64(ulong value) => WriteUInt64(null, value);
    public void WriteSingle(float value) => WriteSingle(null, value);
    public void WriteDouble(double value) => WriteDouble(null, value);
    public void WriteDecimal(decimal value) => WriteDecimal(null, value);
    public void WriteChar(char value) => WriteChar(null, value);
    public void WriteString(string? value) => WriteString(null, value);
    public void WriteDateTime(DateTime value) => WriteDateTime(null, value);
    public void WriteGuid(Guid value) => WriteGuid(null, value);
    public void WriteEnum<TEnum>(TEnum value) where TEnum : struct, Enum => WriteEnum(null, value);
    public void WriteObject(object? value, Action<FormatWriterBase>? writer_before = null) => WriteObject(null, value, writer_before);
    public void Write(FormatWriterBase? value) => Write(null, value);

    public abstract void WriteBoolean(object? context, bool value);
    public abstract void WriteByte(object? context, byte value);
    public abstract void WriteSByte(object? context, sbyte value);
    public abstract void WriteInt16(object? context, short value);
    public abstract void WriteUInt16(object? context, ushort value);
    public abstract void WriteInt32(object? context, int value);
    public abstract void WriteUInt32(object? context, uint value);
    public abstract void WriteInt64(object? context, long value);
    public abstract void WriteUInt64(object? context, ulong value);
    public abstract void WriteSingle(object? context, float value);
    public abstract void WriteDouble(object? context, double value);
    public abstract void WriteDecimal(object? context, decimal value);
    public abstract void WriteChar(object? context, char value);
    public abstract void WriteString(object? context, string? value);
    public abstract void WriteDateTime(object? context, DateTime value);
    public abstract void WriteGuid(object? context, Guid value);
    public abstract void WriteEnum<TEnum>(object? context, TEnum value) where TEnum : struct, Enum;
    public abstract void WriteObject(object? context, object? value, Action<FormatWriterBase>? writer_before = null);
    public abstract void Write(object? context, FormatWriterBase? value);

    public void WriteBooleanArray(bool[] values) => WriteBooleanArray(null, values);
    public void WriteByteArray(byte[] values) => WriteByteArray(null, values);
    public void WriteSByteArray(sbyte[] values) => WriteSByteArray(null, values);
    public void WriteInt16Array(short[] values) => WriteInt16Array(null, values);
    public void WriteUInt16Array(ushort[] values) => WriteUInt16Array(null, values);
    public void WriteInt32Array(int[] values) => WriteInt32Array(null, values);
    public void WriteUInt32Array(uint[] values) => WriteUInt32Array(null, values);
    public void WriteInt64Array(long[] values) => WriteInt64Array(null, values);
    public void WriteUInt64Array(ulong[] values) => WriteUInt64Array(null, values);
    public void WriteSingleArray(float[] values) => WriteSingleArray(null, values);
    public void WriteDoubleArray(double[] values) => WriteDoubleArray(null, values);
    public void WriteDecimalArray(decimal[] values) => WriteDecimalArray(null, values);
    public void WriteCharArray(char[] values) => WriteCharArray(null, values);
    public void WriteStringArray(string[] values) => WriteStringArray(null, values);
    public void WriteDateTimeArray(DateTime[] values) => WriteDateTimeArray(null, values);
    public void WriteGuidArray(Guid[] values) => WriteGuidArray(null, values);
    public void WriteEnumArray<TEnum>(TEnum[] values) where TEnum : struct, Enum => WriteEnumArray(null, values);
    public void WriteObjectArray(object[] values, Action<FormatWriterBase>? writer_before = null) => WriteObjectArray(null, values, writer_before);
    public void WriteArray(FormatWriterBase[] values) => WriteArray(null, values);

    public abstract void WriteBooleanArray(object? context, bool[] values);
    public abstract void WriteByteArray(object? context, byte[] values);
    public abstract void WriteSByteArray(object? context, sbyte[] values);
    public abstract void WriteInt16Array(object? context, short[] values);
    public abstract void WriteUInt16Array(object? context, ushort[] values);
    public abstract void WriteInt32Array(object? context, int[] values);
    public abstract void WriteUInt32Array(object? context, uint[] values);
    public abstract void WriteInt64Array(object? context, long[] values);
    public abstract void WriteUInt64Array(object? context, ulong[] values);
    public abstract void WriteSingleArray(object? context, float[] values);
    public abstract void WriteDoubleArray(object? context, double[] values);
    public abstract void WriteDecimalArray(object? context, decimal[] values);
    public abstract void WriteCharArray(object? context, char[] values);
    public abstract void WriteStringArray(object? context, string[] values);
    public abstract void WriteDateTimeArray(object? context, DateTime[] values);
    public abstract void WriteGuidArray(object? context, Guid[] values);
    public abstract void WriteEnumArray<TEnum>(object? context, TEnum[] values) where TEnum : struct, Enum;
    public abstract void WriteObjectArray(object? context, object[] values, Action<FormatWriterBase>? writer_before = null);
    public abstract void WriteArray(object? context, FormatWriterBase[] values);
}
