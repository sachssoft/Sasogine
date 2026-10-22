using Sachssoft.Engine.Graphics.Text;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Registers installed font variants without distributing system font files.</summary>
internal static class ExampleFontFamily
{
    internal const string Name = "Gallery Sans";
    internal static void Register(FontRegistry registry, string path, FontFace regular)
    {
        var faces = new List<FontFace>
        {
            regular
        };
        string stem = Path.GetFileNameWithoutExtension(path);
        string folder = Path.GetDirectoryName(path)!;
        string prefix = stem.EndsWith("-Regular", StringComparison.OrdinalIgnoreCase) ? stem[..^8] : stem;
        string[] variants = stem.ToLowerInvariant() switch
        {
            "segoeui" => new[]
            {
                "segoeuib",
                "segoeuii",
                "segoeuiz"
            },
            "arial" => new[]
            {
                "arialbd",
                "ariali",
                "arialbi"
            },
            "dejavusans" => new[]
            {
                "DejaVuSans-Bold",
                "DejaVuSans-Oblique",
                "DejaVuSans-BoldOblique"
            },
            _ => new[]
            {
                prefix + "-Bold",
                prefix + "-Italic",
                prefix + "-BoldItalic"
            }
        };
        for (int i = 0; i < variants.Length; i++)
        {
            string candidate = Path.Combine(folder, variants[i] + Path.GetExtension(path));
            if (!File.Exists(candidate))
                continue;
            faces.Add(new FontFace(File.ReadAllBytes(candidate), variants[i], i == 1 ? FontWeight.Normal : FontWeight.Bold, i == 0 ? FontStyle.Normal : FontStyle.Italic));
        }

        registry.Register(Name, faces.ToArray());
    }
}
