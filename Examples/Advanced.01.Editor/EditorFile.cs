using Microsoft.Xna.Framework;
using System.Text.Json;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Versioned JSON with explicit fields; no serializer discovery or reflection.</summary>
internal static class EditorFile
{
    internal static void Validate(EditorObject o)
    {
        if (!float.IsFinite(o.RotationDegrees) || o.RotationDegrees < 0 || o.RotationDegrees >= 360 || o.Id < 1 || o.Id > 1000000 || string.IsNullOrWhiteSpace(o.Name) || o.Name.Length > 80 || o.Name.Any(char.IsControl) || !float.IsFinite(o.X) || !float.IsFinite(o.Y) || !float.IsFinite(o.Width) || !float.IsFinite(o.Height) || o.X < 0 || o.Y < 0 || o.Width < 24 || o.Height < 24 || o.X + o.Width > 960 || o.Y + o.Height > 540)
            throw new InvalidDataException("Invalid rectangle. Names: 1-80 characters; bounds must fit the 960 x 540 canvas.");
    }

    internal static void ValidateAll(IReadOnlyList<EditorObject> objects)
    {
        if (objects.Count > 256)
            throw new InvalidDataException("Maximum 256 objects.");
        var ids = new HashSet<int>();
        foreach (var o in objects)
        {
            Validate(o);
            if (!ids.Add(o.Id))
                throw new InvalidDataException("Duplicate object ID.");
        }
    }

    internal static byte[] Encode(IReadOnlyList<EditorObject> objects)
    {
        ValidateAll(objects);
        using var stream = new MemoryStream();
        using (var w = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
        {
            w.WriteStartObject();
            w.WriteNumber("version", 2);
            w.WriteStartArray("objects");
            foreach (var o in objects)
            {
                w.WriteStartObject();
                w.WriteNumber("id", o.Id);
                w.WriteString("name", o.Name);
                w.WriteNumber("x", o.X);
                w.WriteNumber("y", o.Y);
                w.WriteNumber("width", o.Width);
                w.WriteNumber("height", o.Height);
                w.WriteNumber("rgba", o.Color.PackedValue);
                w.WriteBoolean("visible", o.Visible);
                w.WriteNumber("rotationDegrees", o.RotationDegrees);
                w.WriteEndObject();
            }

            w.WriteEndArray();
            w.WriteEndObject();
        }

        return stream.ToArray();
    }

    internal static EditorObject[] Decode(byte[] bytes)
    {
        if (bytes.Length > 1024 * 1024)
            throw new InvalidDataException("Document exceeds 1 MiB.");
        using var doc = JsonDocument.Parse(bytes, new JsonDocumentOptions { MaxDepth = 8 });
        var root = doc.RootElement;
        int version = root.GetProperty("version").GetInt32();
        if (version is not (1 or 2))
            throw new InvalidDataException("Unsupported document version.");
        var items = root.GetProperty("objects");
        if (items.GetArrayLength() > 256)
            throw new InvalidDataException("Maximum 256 objects.");
        var result = new List<EditorObject>();
        foreach (var o in items.EnumerateArray())
            result.Add(new(o.GetProperty("id").GetInt32(), o.GetProperty("name").GetString() ?? "", o.GetProperty("x").GetSingle(), o.GetProperty("y").GetSingle(), o.GetProperty("width").GetSingle(), o.GetProperty("height").GetSingle(), new Color(o.GetProperty("rgba").GetUInt32()), o.GetProperty("visible").GetBoolean(), version == 1 ? 0 : o.GetProperty("rotationDegrees").GetSingle()));
        ValidateAll(result);
        return result.ToArray();
    }

    internal static EditorObject[] Load(string path)
    {
        if (new FileInfo(path).Length > 1024 * 1024)
            throw new InvalidDataException("Document exceeds 1 MiB.");
        return Decode(File.ReadAllBytes(path));
    }

    internal static void Save(string path, IReadOnlyList<EditorObject> objects)
    {
        byte[] bytes = Encode(objects);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllBytes(temporary, bytes);
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }
}
