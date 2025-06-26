using System;

namespace sachssoft.Sasogine.Document;

public abstract class FormatReaderBase
{
    protected FormatReaderBase() { }

    public abstract bool Contains(object? context);

    public bool ReadBoolean(bool fallback = default) => ReadBoolean(null, fallback);
    public byte ReadByte(byte fallback = default) => ReadByte(null, fallback);
    public sbyte ReadSByte(sbyte fallback = default) => ReadSByte(null, fallback);
    public short ReadInt16(short fallback = default) => ReadInt16(null, fallback);
    public ushort ReadUInt16(ushort fallback = default) => ReadUInt16(null, fallback);
    public int ReadInt32(int fallback = default) => ReadInt32(null, fallback);
    public uint ReadUInt32(uint fallback = default) => ReadUInt32(null, fallback);
    public long ReadInt64(long fallback = default) => ReadInt64(null, fallback);
    public ulong ReadUInt64(ulong fallback = default) => ReadUInt64(null, fallback);
    public float ReadSingle(float fallback = default) => ReadSingle(null, fallback);
    public double ReadDouble(double fallback = default) => ReadDouble(null, fallback);
    public decimal ReadDecimal(decimal fallback = default) => ReadDecimal(null, fallback);
    public char ReadChar(char fallback = default) => ReadChar(null, fallback);
    public string? ReadString(string? fallback = default) => ReadString(null, fallback);
    public DateTime ReadDateTime(DateTime fallback = default) => ReadDateTime(null, fallback);
    public Guid ReadGuid(Guid fallback = default) => ReadGuid(null, fallback);
    public TEnum ReadEnum<TEnum>(TEnum fallback = default) where TEnum : struct, Enum => ReadEnum<TEnum>(null, fallback);
    public object? ReadObject(ObjectReaderGeneratorCallback generator, object? fallback = default) => ReadObject(null, generator, fallback);
    public FormatReaderBase? Read() => Read(null);

    public abstract bool ReadBoolean(object? context, bool fallback = default);
    public abstract byte ReadByte(object? context, byte fallback = default);
    public abstract sbyte ReadSByte(object? context, sbyte fallback = default);
    public abstract short ReadInt16(object? context, short fallback = default);
    public abstract ushort ReadUInt16(object? context, ushort fallback = default);
    public abstract int ReadInt32(object? context, int fallback = default);
    public abstract uint ReadUInt32(object? context, uint fallback = default);
    public abstract long ReadInt64(object? context, long fallback = default);
    public abstract ulong ReadUInt64(object? context, ulong fallback = default);
    public abstract float ReadSingle(object? context, float fallback = default);
    public abstract double ReadDouble(object? context, double fallback = default);
    public abstract decimal ReadDecimal(object? context, decimal fallback = default);
    public abstract char ReadChar(object? context, char fallback = default);
    public abstract string? ReadString(object? context, string? fallback = default);
    public abstract DateTime ReadDateTime(object? context, DateTime fallback = default);
    public abstract Guid ReadGuid(object? context, Guid fallback = default);
    public abstract TEnum ReadEnum<TEnum>(object? context, TEnum fallback = default) where TEnum : struct, Enum;
    public abstract object? ReadObject(object? context, ObjectReaderGeneratorCallback generator, object? fallback = default);
    public abstract FormatReaderBase? Read(object? context);

    public bool[] ReadBooleanArray(bool[]? fallback = default) => ReadBooleanArray(null, fallback);
    public byte[] ReadByteArray(byte[]? fallback = default) => ReadByteArray(null, fallback);
    public sbyte[] ReadSByteArray(sbyte[]? fallback = default) => ReadSByteArray(null, fallback);
    public short[] ReadInt16Array(short[]? fallback = default) => ReadInt16Array(null, fallback);
    public ushort[] ReadUInt16Array(ushort[]? fallback = default) => ReadUInt16Array(null, fallback);
    public int[] ReadInt32Array(int[]? fallback = default) => ReadInt32Array(null, fallback);
    public uint[] ReadUInt32Array(uint[]? fallback = default) => ReadUInt32Array(null, fallback);
    public long[] ReadInt64Array(long[]? fallback = default) => ReadInt64Array(null, fallback);
    public ulong[] ReadUInt64Array(ulong[]? fallback = default) => ReadUInt64Array(null, fallback);
    public float[] ReadSingleArray(float[]? fallback = default) => ReadSingleArray(null, fallback);
    public double[] ReadDoubleArray(double[]? fallback = default) => ReadDoubleArray(null, fallback);
    public decimal[] ReadDecimalArray(decimal[]? fallback = default) => ReadDecimalArray(null, fallback);
    public char[] ReadCharArray(char[]? fallback = default) => ReadCharArray(null, fallback);
    public string[] ReadStringArray(string[]? fallback = default) => ReadStringArray(null, fallback);
    public DateTime[] ReadDateTimeArray(DateTime[]? fallback = default) => ReadDateTimeArray(null, fallback);
    public Guid[] ReadGuidArray(Guid[]? fallback = default) => ReadGuidArray(null, fallback);
    public TEnum[] ReadEnumArray<TEnum>(TEnum[]? fallback = default) where TEnum : struct, Enum => ReadEnumArray<TEnum>(null, fallback);
    public object[] ReadObjectArray(ObjectReaderGeneratorCallback generator, object[]? fallback = default) => ReadObjectArray(null, generator, fallback);
    public FormatReaderBase[] ReadArray() => ReadArray(null);

    public abstract bool[] ReadBooleanArray(object? context, bool[]? fallback = default);
    public abstract byte[] ReadByteArray(object? context, byte[]? fallback = default);
    public abstract sbyte[] ReadSByteArray(object? context, sbyte[]? fallback = default);
    public abstract short[] ReadInt16Array(object? context, short[]? fallback = default);
    public abstract ushort[] ReadUInt16Array(object? context, ushort[]? fallback = default);
    public abstract int[] ReadInt32Array(object? context, int[]? fallback = default);
    public abstract uint[] ReadUInt32Array(object? context, uint[]? fallback = default);
    public abstract long[] ReadInt64Array(object? context, long[]? fallback = default);
    public abstract ulong[] ReadUInt64Array(object? context, ulong[]? fallback = default);
    public abstract float[] ReadSingleArray(object? context, float[]? fallback = default);
    public abstract double[] ReadDoubleArray(object? context, double[]? fallback = default);
    public abstract decimal[] ReadDecimalArray(object? context, decimal[]? fallback = default);
    public abstract char[] ReadCharArray(object? context, char[]? fallback = default);
    public abstract string[] ReadStringArray(object? context, string[]? fallback = default);
    public abstract DateTime[] ReadDateTimeArray(object? context, DateTime[]? fallback = default);
    public abstract Guid[] ReadGuidArray(object? context, Guid[]? fallback = default);
    public abstract TEnum[] ReadEnumArray<TEnum>(object? context, TEnum[]? fallback = default) where TEnum : struct, Enum;
    public abstract object[] ReadObjectArray(object? context, ObjectReaderGeneratorCallback generator, object[]? fallback = default);
    public abstract FormatReaderBase[] ReadArray(object? context);
}
