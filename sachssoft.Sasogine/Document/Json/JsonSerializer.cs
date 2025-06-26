using System.IO;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Runtime.CompilerServices;

namespace sachssoft.Sasogine.Document.Json;

// AOT-Freundlich
public static class JsonSerializer
{
    public static void Load(IObjectFormatReader<JsonReader> reader, System.IO.Stream stream)
    {
        using var reader_stream = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var json = reader_stream.ReadToEnd();

        var json_node = JsonNode.Parse(json) ?? throw new InvalidDataException("JSON parsing failed.");
        var root_reader = new JsonReader();
        root_reader.Node = (JsonObject)json_node;
        reader.Read(root_reader);
    }

    public static void Save(IObjectFormatWriter<JsonWriter> writer, System.IO.Stream stream, JsonSerializerOptions? options = null)
    {
        var root_writer = new JsonWriter();
        writer.Write(root_writer);

        var root_node = (JsonObject)root_writer.Node!;

        // In JSON-String umwandeln
        var json = root_node.ToJsonString(options);

        // In UTF-8-Bytes konvertieren und in den Stream schreiben
        using var writer_stream = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer_stream.Write(json);
        writer_stream.Flush();
    }
}
