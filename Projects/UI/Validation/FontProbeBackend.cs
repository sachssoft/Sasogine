using Sachssoft.Engine.Graphics.Text;

/// <summary>Tracks font reuse and resource ownership without creating a graphics device.</summary>
internal sealed class FontProbeBackend(FontRegistry registry) : IFontBackend, IDisposable
{
    private readonly Dictionary<(FontFace Face, float Size), FontProbe> _fonts = new();
    internal List<FontProbe> Created { get; } = new();

    public void Register(string familyName, FontFace face) => registry.Register(familyName, face);
    public void Register(FontFamily fontFamily) => registry.Register(fontFamily);
    public IEnumerable<string> GetFamilies() => registry.FamilyNames;
    public IEnumerable<FontFace> GetFaces() => registry.GetFaces();
    public FontFamily? GetFamily(string name) => registry.GetFamily(name);

    public IFont GetFont(FontFace face, float size)
    {
        if (!_fonts.TryGetValue((face, size), out FontProbe? font))
        {
            font = new FontProbe(face, (int)size);
            _fonts.Add((face, size), font);
            Created.Add(font);
        }
        return font;
    }

    public void Dispose()
    {
        foreach (FontProbe font in _fonts.Values)
            font.Dispose();
        _fonts.Clear();
    }
}
