using nkast.Aether.Physics2D.Collision;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace sachssoft.Sasogine.Document;

public class JsonReader : FormatReader<JsonReader, string>
{
    private JsonObject _node;

    public JsonReader()
    {
        _node = new JsonObject();
    }

    internal JsonObject Node
    {
        get => _node;
        set => _node = value;
    }

    public ConversionErrorHandling ConversionErrorHandling
    {
        get;
        set;
    }

    public override bool Contains(string? property)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return _node.ContainsKey(property);
    }

    private bool ReadPrimitiveValue<T>(
        string property,
        ref T? value,
        T? fallback,
        Func<JsonNode, T?> convert
    )
    {
        bool OutputValue(ref T? value, ConversionErrorHandling error_handling)
        {
            value = error_handling switch
            {
                ConversionErrorHandling.Default => default,   // Standardwert (z.B. null oder 0)
                ConversionErrorHandling.Replace => fallback,  // Fallback-Wert
                _ => value                                     // Bei Cancel oder Ignore bleibt Wert unverändert
            };
            // Gibt true zurück, wenn Wert gesetzt wurde (Default oder Replace), sonst false
            return error_handling == ConversionErrorHandling.Default || error_handling == ConversionErrorHandling.Replace;
        }

        if (property == null || !_node.TryGetPropertyValue(property, out var json_node))
        {
            // Property fehlt komplett
            return OutputValue(ref value, ConversionErrorHandling);
        }

        if (json_node is null)
        {
            // Property ist explizit null
            return OutputValue(ref value, ConversionErrorHandling);
        }

        // Versuche zu konvertieren
        var converted = convert(json_node);

        if (converted == null)
        {
            // Konvertierung fehlgeschlagen oder Wert null
            return OutputValue(ref value, ConversionErrorHandling);
        }
        else
        {
            // Erfolg: Wert setzen
            value = converted;
            return true;
        }
    }

    private T[] ReadPrimitiveArray<T>(
        string? property,
        T[] fallback,
        Func<object?, T?> convert,
        ConversionErrorHandling errorHandling = ConversionErrorHandling.Cancel,
        T? replacement_value = default
    )
    {
       if (property == null || !_node.TryGetPropertyValue(property, out var val))
            return fallback ?? Array.Empty<T>();

        if (val is IEnumerable<object> enumerable)
        {
            var list = new List<T>();
            foreach (var item in enumerable)
            {
                var converted = convert(item);

                if (converted == null)
                {
                    switch (errorHandling)
                    {
                        case ConversionErrorHandling.Ignore:
                            // überspringen
                            continue;

                        case ConversionErrorHandling.Cancel:
                            // Abbruch, fallback zurückgeben
                            return fallback ?? Array.Empty<T>();

                        case ConversionErrorHandling.Replace:
                        case ConversionErrorHandling.Default:
                            // Ersatzwert benutzen, wenn gesetzt
                            if (replacement_value != null)
                                list.Add(replacement_value);
                            else
                                list.Add(default!);
                            break;

                        default:
                            // Optional: Bei unbekanntem Wert abbrechen oder ignorieren
                            return fallback ?? Array.Empty<T>();
                    }
                }
                else
                {
                    list.Add(converted);
                }
            }
            return list.ToArray();
        }

        return fallback ?? Array.Empty<T>();
    }

    public override bool ReadBoolean(string? property, bool fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        bool value = fallback;

        bool success = ReadPrimitiveValue<bool>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<bool>()
        );

        return success ? value : fallback;
    }

    public override byte ReadByte(string? property, byte fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        byte value = fallback;

        bool success = ReadPrimitiveValue<byte>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<byte>()
        );

        return success ? value : fallback;
    }

    public override sbyte ReadSByte(string? property, sbyte fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        sbyte value = fallback;

        bool success = ReadPrimitiveValue<sbyte>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<sbyte>()
        );

        return success ? value : fallback;
    }

    public override short ReadInt16(string? property, short fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        short value = fallback;

        bool success = ReadPrimitiveValue<short>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<short>()
        );

        return success ? value : fallback;
    }

    public override ushort ReadUInt16(string? property, ushort fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        ushort value = fallback;

        bool success = ReadPrimitiveValue<ushort>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<ushort>()
        );

        return success ? value : fallback;
    }

    public override int ReadInt32(string? property, int fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        int value = fallback;

        bool success = ReadPrimitiveValue<int>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<int>()
        );

        return success ? value : fallback;
    }

    public override uint ReadUInt32(string? property, uint fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        uint value = fallback;

        bool success = ReadPrimitiveValue<uint>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<uint>()
        );

        return success ? value : fallback;
    }

    public override long ReadInt64(string? property, long fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        long value = fallback;

        bool success = ReadPrimitiveValue<long>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<long>()
        );

        return success ? value : fallback;
    }

    public override ulong ReadUInt64(string? property, ulong fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        ulong value = fallback;

        bool success = ReadPrimitiveValue<ulong>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<ulong>()
        );

        return success ? value : fallback;
    }

    public override float ReadSingle(string? property, float fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        float value = fallback;

        bool success = ReadPrimitiveValue<float>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<float>()
        );

        return success ? value : fallback;
    }

    public override double ReadDouble(string? property, double fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        double value = fallback;

        bool success = ReadPrimitiveValue<double>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<double>()
        );

        return success ? value : fallback;
    }

    public override decimal ReadDecimal(string? property, decimal fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        decimal value = fallback;

        bool success = ReadPrimitiveValue<decimal>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<decimal>()
        );

        return success ? value : fallback;
    }

    public override char ReadChar(string? property, char fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        char value = fallback;

        bool success = ReadPrimitiveValue<char>(
            property,
            ref value,
            fallback,
            node =>
            {
                var s = ((JsonValue)node).GetValue<string>();
                return !string.IsNullOrEmpty(s) ? s[0] : default;
            }
        );

        return success ? value : fallback;
    }

    public override string? ReadString(string? property, string? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        string? value = fallback;

        bool success = ReadPrimitiveValue<string?>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<string>()
        );

        return success ? value : fallback;
    }

    public override DateTime ReadDateTime(string? property, DateTime fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        DateTime value = fallback;

        bool success = ReadPrimitiveValue<DateTime>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<DateTime>()
        );

        return success ? value : fallback;
    }

    public override Guid ReadGuid(string? property, Guid fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        Guid value = fallback;

        bool success = ReadPrimitiveValue<Guid>(
            property,
            ref value,
            fallback,
            node => ((JsonValue)node).GetValue<Guid>()
        );

        return success ? value : fallback;
    }

    public override TEnum ReadEnum<TEnum>(string? property, TEnum fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));

        if (_node.TryGetPropertyValue(property, out var val) && val is JsonValue value)
        {
            if (value.TryGetValue<string>(out var safe_value))
            {
                return Enum.TryParse<TEnum>(JsonUtils.TrainToPascalCase(safe_value), out var result) ? result : fallback;
            }
        }

        return fallback;
    }

    public override IObjectReader<JsonReader>? ReadObject(string? property, ObjectReaderGeneratorCallback<JsonReader> generator, IObjectReader<JsonReader>? fallback = null)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));

        var list = new List<IObjectReader<JsonReader>>();

        if (_node.TryGetPropertyValue(property, out var val) && val is JsonObject obj)
        {
            if (obj is JsonObject)
            {
                var reader = new JsonReader();
                reader._node = (JsonObject)obj;
                var instance = generator.Invoke(reader);

                if (instance != null)
                {
                    // Gilt nur für Referenztypen (Klassen): Nach erfolgreicher Instanziierung wird instance.Read(reader) aufgerufen,
                    // um weitere Eigenschaften zu initialisieren. Bei Werttypen (Structs) sollte dies bereits im Generator erfolgen,
                    // da keine nachträgliche Initialisierung via Read(reader) vorgesehen ist.

                    if (!instance.GetType().IsValueType)
                    {
                        instance.Read(reader);
                    }

                    return instance;
                }
            }
        }

        return null;
    }

    public override FormatReader<JsonReader, string> Read(string? property)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));

        var reader = new JsonReader
        {
            ConversionErrorHandling = ConversionErrorHandling
        };

        if (_node.TryGetPropertyValue(property, out var json_node) && json_node is JsonObject json_object)
        {
            reader._node = json_object;
        }

        return reader;
    }

    // --- Arrays ---

    public override bool[] ReadBooleanArray(string? property, bool[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<bool>(property, fallback ?? Array.Empty<bool>(), item => item is JsonValue jv ? jv.GetValue<bool>() : default);
    }

    public override byte[] ReadByteArray(string? property, byte[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<byte>(property, fallback ?? Array.Empty<byte>(), item => item is JsonValue jv ? jv.GetValue<byte>() : default);
    }

    public override sbyte[] ReadSByteArray(string? property, sbyte[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<sbyte>(property, fallback ?? Array.Empty<sbyte>(), item => item is JsonValue jv ? jv.GetValue<sbyte>() : default);
    }

    public override short[] ReadInt16Array(string? property, short[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<short>(property, fallback ?? Array.Empty<short>(), item => item is JsonValue jv ? jv.GetValue<short>() : default);
    }

    public override ushort[] ReadUInt16Array(string? property, ushort[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<ushort>(property, fallback ?? Array.Empty<ushort>(), item => item is JsonValue jv ? jv.GetValue<ushort>() : default);
    }

    public override int[] ReadInt32Array(string? property, int[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<int>(property, fallback ?? Array.Empty<int>(), item => item is JsonValue jv ? jv.GetValue<int>() : default);
    }

    public override uint[] ReadUInt32Array(string? property, uint[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<uint>(property, fallback ?? Array.Empty<uint>(), item => item is JsonValue jv ? jv.GetValue<uint>() : default);
    }

    public override long[] ReadInt64Array(string? property, long[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<long>(property, fallback ?? Array.Empty<long>(), item => item is JsonValue jv ? jv.GetValue<long>() : default);
    }

    public override ulong[] ReadUInt64Array(string? property, ulong[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<ulong>(property, fallback ?? Array.Empty<ulong>(), item => item is JsonValue jv ? jv.GetValue<ulong>() : default);
    }

    public override float[] ReadSingleArray(string? property, float[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<float>(property, fallback ?? Array.Empty<float>(), item => item is JsonValue jv ? jv.GetValue<float>() : default);
    }

    public override double[] ReadDoubleArray(string? property, double[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<double>(property, fallback ?? Array.Empty<double>(), item => item is JsonValue jv ? jv.GetValue<double>() : default);
    }

    public override decimal[] ReadDecimalArray(string? property, decimal[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<decimal>(property, fallback ?? Array.Empty<decimal>(), item => item is JsonValue jv ? jv.GetValue<decimal>() : default);
    }

    public override char[] ReadCharArray(string? property, char[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<char>(property, fallback ?? Array.Empty<char>(), item =>
        {
            if (item is JsonValue jv)
            {
                var s = jv.GetValue<string>();
                return !string.IsNullOrEmpty(s) ? s[0] : default;
            }
            return default;
        });
    }

    public override string[] ReadStringArray(string? property, string[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<string>(property, fallback ?? Array.Empty<string>(), item => item is JsonValue jv ? jv.GetValue<string>() : null);
    }

    public override DateTime[] ReadDateTimeArray(string? property, DateTime[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<DateTime>(property, fallback ?? Array.Empty<DateTime>(), item => item is JsonValue jv ? jv.GetValue<DateTime>() : default);
    }

    public override Guid[] ReadGuidArray(string? property, Guid[]? fallback = default)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));
        return ReadPrimitiveArray<Guid>(property, fallback ?? Array.Empty<Guid>(), item => item is JsonValue jv ? jv.GetValue<Guid>() : default);
    }

    public override TEnum[] ReadEnumArray<TEnum>(string? property, TEnum[]? fallback = null)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));

        var list = new List<TEnum>();

        if (_node.TryGetPropertyValue(property, out var val) && val is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item is JsonValue item_val)
                {
                    var string_value = ReadString(JsonUtils.TrainToPascalCase(item_val.GetValue<string>()));
                    if (Enum.TryParse<TEnum>(string_value, out var result))
                    {
                        list.Add(result);
                    }
                }
            }
        }

        return list.ToArray();
    }

    public override IObjectReader<JsonReader>[] ReadObjectArray(string? property, ObjectReaderGeneratorCallback<JsonReader> generator, IObjectReader<JsonReader>[]? fallback = null)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));

        var list = new List<IObjectReader<JsonReader>>();

        if (_node.TryGetPropertyValue(property, out var val) && val is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item is JsonObject)
                {
                    var reader = new JsonReader();
                    reader._node = (JsonObject)item;
                    var instance = generator.Invoke(reader);

                    if (instance != null)
                    {
                        instance.Read(reader);
                        list.Add(instance);
                    }
                }
            }
        }

        return list.ToArray();
    }

    public override FormatReader<JsonReader, string>[] ReadArray(string? property)
    {
        _ = property ?? throw new ArgumentNullException(nameof(property));

        var result = new List<FormatReader<JsonReader, string>>();

        if (_node.TryGetPropertyValue(property, out var json_node) && json_node is JsonArray json_array)
        {
            foreach (var item in json_array)
            {
                if (item is JsonObject json_object)
                {
                    var reader = new JsonReader
                    {
                        ConversionErrorHandling = ConversionErrorHandling,
                        _node = json_object
                    };

                    result.Add(reader);
                }
            }
        }

        return result.ToArray();
    }
}
