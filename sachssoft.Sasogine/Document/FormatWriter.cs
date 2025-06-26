using System;
using System.Linq;

namespace sachssoft.Sasogine.Document;

public abstract class FormatWriter<TWriter, TProperty> : FormatWriterBase
    where TWriter : FormatWriter<TWriter, TProperty>
{
    protected FormatWriter()
    {
    }

    // Einzelwerte mit optionalem Kontext
    public abstract void WriteBoolean(TProperty? property, bool value);
    public abstract void WriteByte(TProperty? property, byte value);
    public abstract void WriteSByte(TProperty? property, sbyte value);
    public abstract void WriteInt16(TProperty? property, short value);
    public abstract void WriteUInt16(TProperty? property, ushort value);
    public abstract void WriteInt32(TProperty? property, int value);
    public abstract void WriteUInt32(TProperty? property, uint value);
    public abstract void WriteInt64(TProperty? property, long value);
    public abstract void WriteUInt64(TProperty? property, ulong value);
    public abstract void WriteSingle(TProperty? property, float value);
    public abstract void WriteDouble(TProperty? property, double value);
    public abstract void WriteDecimal(TProperty? property, decimal value);
    public abstract void WriteChar(TProperty? property, char value);
    public abstract void WriteString(TProperty? property, string? value);
    public abstract void WriteDateTime(TProperty? property, DateTime value);
    public abstract void WriteGuid(TProperty? property, Guid value);
    public abstract void WriteEnum<TEnum>(TProperty? property, TEnum value) where TEnum : struct, Enum;
    public abstract void WriteObject(TProperty? property, IObjectWriter<TWriter>? value, Action<TWriter>? writer_before = null);
    public abstract void Write(TProperty? property, FormatWriter<TWriter, TProperty>? value);

    // Arrays mit optionalem Kontext
    public abstract void WriteBooleanArray(TProperty? property, bool[] values);
    public abstract void WriteByteArray(TProperty? property, byte[] values);
    public abstract void WriteSByteArray(TProperty? property, sbyte[] values);
    public abstract void WriteInt16Array(TProperty? property, short[] values);
    public abstract void WriteUInt16Array(TProperty? property, ushort[] values);
    public abstract void WriteInt32Array(TProperty? property, int[] values);
    public abstract void WriteUInt32Array(TProperty? property, uint[] values);
    public abstract void WriteInt64Array(TProperty? property, long[] values);
    public abstract void WriteUInt64Array(TProperty? property, ulong[] values);
    public abstract void WriteSingleArray(TProperty? property, float[] values);
    public abstract void WriteDoubleArray(TProperty? property, double[] values);
    public abstract void WriteDecimalArray(TProperty? property, decimal[] values);
    public abstract void WriteCharArray(TProperty? property, char[] values);
    public abstract void WriteStringArray(TProperty? property, string[] values);
    public abstract void WriteDateTimeArray(TProperty? property, DateTime[] values);
    public abstract void WriteGuidArray(TProperty? property, Guid[] values);
    public abstract void WriteEnumArray<TEnum>(TProperty? property, TEnum[] values) where TEnum : struct, Enum;
    public abstract void WriteObjectArray(TProperty? property, IObjectWriter<TWriter>[] values);
    public abstract void WriteArray(TProperty? property, FormatWriter<TWriter, TProperty>[] values);


    #region HIDDEN MEMBERS (sealed overrides mit object? context)

    public override sealed void WriteBoolean(object? context, bool value) => WriteBoolean((TProperty?)context, value);
    public override sealed void WriteByte(object? context, byte value) => WriteByte((TProperty?)context, value);
    public override sealed void WriteSByte(object? context, sbyte value) => WriteSByte((TProperty?)context, value);
    public override sealed void WriteInt16(object? context, short value) => WriteInt16((TProperty?)context, value);
    public override sealed void WriteUInt16(object? context, ushort value) => WriteUInt16((TProperty?)context, value);
    public override sealed void WriteInt32(object? context, int value) => WriteInt32((TProperty?)context, value);
    public override sealed void WriteUInt32(object? context, uint value) => WriteUInt32((TProperty?)context, value);
    public override sealed void WriteInt64(object? context, long value) => WriteInt64((TProperty?)context, value);
    public override sealed void WriteUInt64(object? context, ulong value) => WriteUInt64((TProperty?)context, value);
    public override sealed void WriteSingle(object? context, float value) => WriteSingle((TProperty?)context, value);
    public override sealed void WriteDouble(object? context, double value) => WriteDouble((TProperty?)context, value);
    public override sealed void WriteDecimal(object? context, decimal value) => WriteDecimal((TProperty?)context, value);
    public override sealed void WriteChar(object? context, char value) => WriteChar((TProperty?)context, value);
    public override sealed void WriteString(object? context, string? value) => WriteString((TProperty?)context, value);
    public override sealed void WriteDateTime(object? context, DateTime value) => WriteDateTime((TProperty?)context, value);
    public override sealed void WriteGuid(object? context, Guid value) => WriteGuid((TProperty?)context, value);
    public override sealed void WriteEnum<TEnum>(object? context, TEnum value) => WriteEnum((TProperty?)context, value);
    public override sealed void WriteObject(object? context, object? value, Action<FormatWriterBase>? writer_before = null) => WriteObject(property: (TProperty?)context, (IObjectFormatWriter<TWriter>?)value, writer_before);
    public override sealed void Write(object? context, FormatWriterBase? value) => Write((TProperty?)context, (FormatWriter<TWriter, TProperty>?)value);

    public override sealed void WriteBooleanArray(object? context, bool[] values) => WriteBooleanArray((TProperty?)context, values);
    public override sealed void WriteByteArray(object? context, byte[] values) => WriteByteArray((TProperty?)context, values);
    public override sealed void WriteSByteArray(object? context, sbyte[] values) => WriteSByteArray((TProperty?)context, values);
    public override sealed void WriteInt16Array(object? context, short[] values) => WriteInt16Array((TProperty?)context, values);
    public override sealed void WriteUInt16Array(object? context, ushort[] values) => WriteUInt16Array((TProperty?)context, values);
    public override sealed void WriteInt32Array(object? context, int[] values) => WriteInt32Array((TProperty?)context, values);
    public override sealed void WriteUInt32Array(object? context, uint[] values) => WriteUInt32Array((TProperty?)context, values);
    public override sealed void WriteInt64Array(object? context, long[] values) => WriteInt64Array((TProperty?)context, values);
    public override sealed void WriteUInt64Array(object? context, ulong[] values) => WriteUInt64Array((TProperty?)context, values);
    public override sealed void WriteSingleArray(object? context, float[] values) => WriteSingleArray((TProperty?)context, values);
    public override sealed void WriteDoubleArray(object? context, double[] values) => WriteDoubleArray((TProperty?)context, values);
    public override sealed void WriteDecimalArray(object? context, decimal[] values) => WriteDecimalArray((TProperty?)context, values);
    public override sealed void WriteCharArray(object? context, char[] values) => WriteCharArray((TProperty?)context, values);
    public override sealed void WriteStringArray(object? context, string[] values) => WriteStringArray((TProperty?)context, values);
    public override sealed void WriteDateTimeArray(object? context, DateTime[] values) => WriteDateTimeArray((TProperty?)context, values);
    public override sealed void WriteGuidArray(object? context, Guid[] values) => WriteGuidArray((TProperty?)context, values);
    public override sealed void WriteEnumArray<TEnum>(object? context, TEnum[] values) => WriteEnumArray((TProperty?)context, values);
    public override sealed void WriteObjectArray(object? context, object[] values, Action<FormatWriterBase>? writer_before = null) => WriteObjectArray((TProperty?)context, values, writer_before);
    public override sealed void WriteArray(object? context, FormatWriterBase[] values) => WriteArray((TProperty?)context, values.Select(x => (FormatWriter<TWriter, TProperty>)x).ToArray());

    #endregion
}
