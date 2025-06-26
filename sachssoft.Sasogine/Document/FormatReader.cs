using System;

namespace sachssoft.Sasogine.Document;

public abstract class FormatReader<TReader, TProperty> : FormatReaderBase
    where TReader : FormatReader<TReader, TProperty>
{
    protected FormatReader()
    {
    }

    public override sealed bool Contains(object? context) => Contains(property: (TProperty?)context);
    public abstract bool Contains(TProperty? property);

    public abstract bool ReadBoolean(TProperty? property, bool fallback = default);
    public abstract byte ReadByte(TProperty? property, byte fallback = default);
    public abstract sbyte ReadSByte(TProperty? property, sbyte fallback = default);
    public abstract short ReadInt16(TProperty? property, short fallback = default);
    public abstract ushort ReadUInt16(TProperty? property, ushort fallback = default);
    public abstract int ReadInt32(TProperty? property, int fallback = default);
    public abstract uint ReadUInt32(TProperty? property, uint fallback = default);
    public abstract long ReadInt64(TProperty? property, long fallback = default);
    public abstract ulong ReadUInt64(TProperty? property, ulong fallback = default);
    public abstract float ReadSingle(TProperty? property, float fallback = default);
    public abstract double ReadDouble(TProperty? property, double fallback = default);
    public abstract decimal ReadDecimal(TProperty? property, decimal fallback = default);
    public abstract char ReadChar(TProperty? property, char fallback = default);
    public abstract string? ReadString(TProperty? property, string? fallback = default);
    public abstract DateTime ReadDateTime(TProperty? property, DateTime fallback = default);
    public abstract Guid ReadGuid(TProperty? property, Guid fallback = default);
    public abstract TEnum ReadEnum<TEnum>(TProperty? property, TEnum fallback = default) where TEnum : struct, Enum;
    public abstract IObjectReader<TReader>? ReadObject(TProperty? property, ObjectReaderGeneratorCallback<TReader> generator, IObjectReader<TReader>? fallback = default);
    public abstract FormatReader<TReader, TProperty>? Read(TProperty? property);

    public abstract bool[] ReadBooleanArray(TProperty? property, bool[]? fallback = default);
    public abstract byte[] ReadByteArray(TProperty? property, byte[]? fallback = default);
    public abstract sbyte[] ReadSByteArray(TProperty? property, sbyte[]? fallback = default);
    public abstract short[] ReadInt16Array(TProperty? property, short[]? fallback = default);
    public abstract ushort[] ReadUInt16Array(TProperty? property, ushort[]? fallback = default);
    public abstract int[] ReadInt32Array(TProperty? property, int[]? fallback = default);
    public abstract uint[] ReadUInt32Array(TProperty? property, uint[]? fallback = default);
    public abstract long[] ReadInt64Array(TProperty? property, long[]? fallback = default);
    public abstract ulong[] ReadUInt64Array(TProperty? property, ulong[]? fallback = default);
    public abstract float[] ReadSingleArray(TProperty? property, float[]? fallback = default);
    public abstract double[] ReadDoubleArray(TProperty? property, double[]? fallback = default);
    public abstract decimal[] ReadDecimalArray(TProperty? property, decimal[]? fallback = default);
    public abstract char[] ReadCharArray(TProperty? property, char[]? fallback = default);
    public abstract string[] ReadStringArray(TProperty? property, string[]? fallback = default);
    public abstract DateTime[] ReadDateTimeArray(TProperty? property, DateTime[]? fallback = default);
    public abstract Guid[] ReadGuidArray(TProperty? property, Guid[]? fallback = default);
    public abstract TEnum[] ReadEnumArray<TEnum>(TProperty? property, TEnum[]? fallback = default) where TEnum : struct, Enum;
    public abstract IObjectReader<TReader>[] ReadObjectArray(TProperty? property, ObjectReaderGeneratorCallback<TReader> generator, IObjectReader<TReader>[]? fallback = default);
    public abstract FormatReader<TReader, TProperty>[] ReadArray(TProperty? property);

    #region HIDDEN MEMBERS (sealed override mit object? context und fallback)

    public override sealed bool ReadBoolean(object? context, bool fallback = default) => ReadBoolean((TProperty?)context, fallback);
    public override sealed byte ReadByte(object? context, byte fallback = default) => ReadByte((TProperty?)context, fallback);
    public override sealed sbyte ReadSByte(object? context, sbyte fallback = default) => ReadSByte((TProperty?)context, fallback);
    public override sealed short ReadInt16(object? context, short fallback = default) => ReadInt16((TProperty?)context, fallback);
    public override sealed ushort ReadUInt16(object? context, ushort fallback = default) => ReadUInt16((TProperty?)context, fallback);
    public override sealed int ReadInt32(object? context, int fallback = default) => ReadInt32((TProperty?)context, fallback);
    public override sealed uint ReadUInt32(object? context, uint fallback = default) => ReadUInt32((TProperty?)context, fallback);
    public override sealed long ReadInt64(object? context, long fallback = default) => ReadInt64((TProperty?)context, fallback);
    public override sealed ulong ReadUInt64(object? context, ulong fallback = default) => ReadUInt64((TProperty?)context, fallback);
    public override sealed float ReadSingle(object? context, float fallback = default) => ReadSingle((TProperty?)context, fallback);
    public override sealed double ReadDouble(object? context, double fallback = default) => ReadDouble((TProperty?)context, fallback);
    public override sealed decimal ReadDecimal(object? context, decimal fallback = default) => ReadDecimal((TProperty?)context, fallback);
    public override sealed char ReadChar(object? context, char fallback = default) => ReadChar((TProperty?)context, fallback);
    public override sealed string? ReadString(object? context, string? fallback = default) => ReadString((TProperty?)context, fallback);
    public override sealed DateTime ReadDateTime(object? context, DateTime fallback = default) => ReadDateTime((TProperty?)context, fallback);
    public override sealed Guid ReadGuid(object? context, Guid fallback = default) => ReadGuid((TProperty?)context, fallback);
    public override sealed TEnum ReadEnum<TEnum>(object? context, TEnum fallback = default) => ReadEnum<TEnum>((TProperty?)context, fallback);
    public override sealed object? ReadObject(object? context, ObjectReaderGeneratorCallback generator, object? fallback = default) => ReadObject((TProperty?)context, (reader) => (IObjectReader<TReader>)generator(reader)!, (IObjectReader<TReader>?)fallback);
    public override sealed FormatReaderBase? Read(object? context) => Read((TProperty?)context);

    public override sealed bool[] ReadBooleanArray(object? context, bool[]? fallback = default) => ReadBooleanArray((TProperty?)context, fallback);
    public override sealed byte[] ReadByteArray(object? context, byte[]? fallback = default) => ReadByteArray((TProperty?)context, fallback);
    public override sealed sbyte[] ReadSByteArray(object? context, sbyte[]? fallback = default) => ReadSByteArray((TProperty?)context, fallback);
    public override sealed short[] ReadInt16Array(object? context, short[]? fallback = default) => ReadInt16Array((TProperty?)context, fallback);
    public override sealed ushort[] ReadUInt16Array(object? context, ushort[]? fallback = default) => ReadUInt16Array((TProperty?)context, fallback);
    public override sealed int[] ReadInt32Array(object? context, int[]? fallback = default) => ReadInt32Array((TProperty?)context, fallback);
    public override sealed uint[] ReadUInt32Array(object? context, uint[]? fallback = default) => ReadUInt32Array((TProperty?)context, fallback);
    public override sealed long[] ReadInt64Array(object? context, long[]? fallback = default) => ReadInt64Array((TProperty?)context, fallback);
    public override sealed ulong[] ReadUInt64Array(object? context, ulong[]? fallback = default) => ReadUInt64Array((TProperty?)context, fallback);
    public override sealed float[] ReadSingleArray(object? context, float[]? fallback = default) => ReadSingleArray((TProperty?)context, fallback);
    public override sealed double[] ReadDoubleArray(object? context, double[]? fallback = default) => ReadDoubleArray((TProperty?)context, fallback);
    public override sealed decimal[] ReadDecimalArray(object? context, decimal[]? fallback = default) => ReadDecimalArray((TProperty?)context, fallback);
    public override sealed char[] ReadCharArray(object? context, char[]? fallback = default) => ReadCharArray((TProperty?)context, fallback);
    public override sealed string[] ReadStringArray(object? context, string[]? fallback = default) => ReadStringArray((TProperty?)context, fallback);
    public override sealed DateTime[] ReadDateTimeArray(object? context, DateTime[]? fallback = default) => ReadDateTimeArray((TProperty?)context, fallback);
    public override sealed Guid[] ReadGuidArray(object? context, Guid[]? fallback = default) => ReadGuidArray((TProperty?)context, fallback);
    public override sealed TEnum[] ReadEnumArray<TEnum>(object? context, TEnum[]? fallback = default) => ReadEnumArray<TEnum>((TProperty?)context, fallback);
    public override sealed object[] ReadObjectArray(object? context, ObjectReaderGeneratorCallback generator, object[]? fallback = default) => ReadObjectArray(property: (TProperty?)context, generator: (reader) => (IObjectReader<TReader>?)generator(reader), fallback: ConvertFallback(fallback));
    public override sealed FormatReaderBase[] ReadArray(object? context) => ReadArray((TProperty?)context);

    #endregion

    private IObjectReader<TReader>[]? ConvertFallback(object[]? fallback)
    {
        if (fallback == null) return null;
        var result = new IObjectReader<TReader>[fallback.Length];
        for (int i = 0; i < fallback.Length; i++)
            result[i] = (IObjectReader<TReader>)fallback[i];
        return result;
    }
}
