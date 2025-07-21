using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sachssoft.Sasogine.Resources;

public class PackageManifest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Author { get; set; } = "";
    public string Type { get; set; } = "unknown";
    public Version Version { get; set; } = new Version(1, 0);
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    // Manuelles Parsen aus JSON-Stream (einfacher Parser, nur für flache Struktur)
    public static PackageManifest Parse(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var json = reader.ReadToEnd();

        var manifest = new PackageManifest();

        // Einfacher manueller Parse - keine Reflection
        // Beispiel: "Title": "My Package", "Author": "Me" ...
        foreach (var line in json.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var kvp = line.Trim().Trim(',', ' ', '\t').Split(':', 2);
            if (kvp.Length != 2) continue;

            var key = kvp[0].Trim(' ', '\"');
            var val = kvp[1].Trim(' ', '\"');

            switch (key)
            {
                case "Title": manifest.Title = val; break;
                case "Description": manifest.Description = val; break;
                case "Author": manifest.Author = val; break;
                case "Type": manifest.Type = val; break;
                case "Version":
                    if (Version.TryParse(val, out var ver))
                        manifest.Version = ver;
                    break;
                default:
                    manifest.Metadata[key] = val;
                    break;
            }
        }

        return manifest;
    }

    // Manuelles Serialisieren in JSON-String
    public string ToJson()
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"Title\": \"{Escape(Title)}\",");
        sb.AppendLine($"  \"Description\": \"{Escape(Description)}\",");
        sb.AppendLine($"  \"Author\": \"{Escape(Author)}\",");
        sb.AppendLine($"  \"Type\": \"{Escape(Type)}\",");
        sb.AppendLine($"  \"Version\": \"{Version}\",");

        sb.AppendLine("  \"Metadata\": {");
        foreach (var kv in Metadata)
        {
            sb.AppendLine($"    \"{Escape(kv.Key)}\": \"{Escape(kv.Value)}\",");
        }
        sb.AppendLine("  }");

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string Escape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}