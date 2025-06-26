using System;
using System.IO;
using System.Threading.Tasks;

namespace sachssoft.Sasogine.Document;

public static class SerializerExtensions
{
    public static void Save<TWriter>(this IObjectFormatWriter<TWriter> obj, System.IO.Stream stream)
        where TWriter : FormatWriterBase
    {
        obj.SaveTo(stream);
    }

    public static void Save<TWriter>(this IObjectFormatWriter<TWriter> obj, string file_path)
        where TWriter : FormatWriterBase
    {
        using var fs = File.Create(file_path);
        obj.SaveTo(fs);
    }

    public static async Task SaveAsync<TWriter>(this IObjectFormatWriter<TWriter> obj, string file_path)
    where TWriter : FormatWriterBase
    {
        await using var stream = File.Create(file_path);
        obj.SaveTo(stream); // nur async um den Dateizugriff zu kapseln
    }

    public static byte[] SaveToBytes<TWriter>(this IObjectFormatWriter<TWriter> obj)
    where TWriter : FormatWriterBase
    {
        using var stream = new MemoryStream();
        obj.SaveTo(stream);
        return stream.ToArray();
    }

    public static string SaveToBase64<TWriter>(this IObjectFormatWriter<TWriter> obj)
    where TWriter : FormatWriterBase
    {
        return Convert.ToBase64String(obj.SaveToBytes());
    }

    public static void Load<TReader>(this IObjectFormatReader<TReader> obj, System.IO.Stream stream)
        where TReader : FormatReaderBase
    {
        obj.LoadFrom(stream);
    }

    public static void Load<TReader>(this IObjectFormatReader<TReader> obj, string file_path)
        where TReader : FormatReaderBase
    {
        using var fs = File.OpenRead(file_path);
        obj.LoadFrom(fs);
    }

    public static async Task LoadAsync<TReader>(this IObjectFormatReader<TReader> obj, string file_path)
        where TReader : FormatReaderBase
    {
        await using var stream = File.OpenRead(file_path);
        obj.LoadFrom(stream); // nur async um den Dateizugriff zu kapseln
    }

    public static void LoadFromBytes<TReader>(this IObjectFormatReader<TReader> obj, byte[] data)
        where TReader : FormatReaderBase
    {
        using var stream = new MemoryStream(data);
        obj.LoadFrom(stream);
    }

    public static void LoadFromBase64<TReader>(this IObjectFormatReader<TReader> obj, string base64)
        where TReader : FormatReaderBase
    {
        var bytes = Convert.FromBase64String(base64);
        obj.LoadFromBytes(bytes);
    }
}
