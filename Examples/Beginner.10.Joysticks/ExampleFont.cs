namespace Sachssoft.Engine.Controls.Examples;
internal static class ExampleFont
{
    internal static string Resolve(string? requested)
    {
        if (requested is not null)
            return File.Exists(requested) ? requested : throw new FileNotFoundException("The specified font was not found.", requested);
        string systemFonts = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        string[] candidates =
        {
            Path.Combine(AppContext.BaseDirectory, "Content", "Fonts", "Default.ttf"),
            Path.Combine(systemFonts, "segoeui.ttf"),
            Path.Combine(systemFonts, "arial.ttf"),
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/usr/share/fonts/truetype/liberation2/LiberationSans-Regular.ttf",
            "/System/Library/Fonts/Supplemental/Arial.ttf"
        };
        foreach (string candidate in candidates)
            if (File.Exists(candidate))
                return candidate;
        throw new FileNotFoundException("No default font found. Start with --font <path-to-font.ttf>.");
    }
}
