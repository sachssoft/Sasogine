using FontStashSharp;
using Sachssoft.Engine.Graphics.Text;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Owns shared renderer adapters and one glyph system per engine font face.
/// </summary>
internal sealed class ControlFontCache : IDisposable
{
    private readonly FontRegistry _registry;
    private readonly Func<FontFace, int, IControlFont>? _factory;
    private readonly Dictionary<ControlFontKey, IControlFont> _fonts = new();
    private readonly Dictionary<FontFace, FontSystem> _systems = new(ReferenceEqualityComparer.Instance);
    private bool _disposed;
    internal ControlFontCache(FontRegistry registry, Func<FontFace, int, IControlFont>? factory)
    {
        _registry = registry;
        _factory = factory;
    }

    internal IControlFont Resolve(ControlFontRequest request, IControlFont fallback)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        IControlFont source = request.Source ?? fallback;
        if (!request.HasDescriptor)
            return source;
        var sourceFace = source as FontFaceControlFont;
        int size = request.Size ?? sourceFace?.Size ?? 16;
        FontFace face;
        if (request.Name is string name)
        {
            face = _registry.Resolve(name, request.Weight ?? FontWeight.Normal, request.Style ?? FontStyle.Normal);
        }
        else if (sourceFace is not null)
        {
            FontFamily? family = null;
            // This search happens only for a changed request, never during drawing or a cached layout.
            foreach (FontFamily candidate in _registry.Families)
            {
                foreach (FontFace candidateFace in candidate.Faces)
                    if (ReferenceEquals(candidateFace, sourceFace.Face))
                    {
                        family = candidate;
                        break;
                    }

                if (family is not null)
                    break;
            }

            face = family?.GetFace(request.Weight ?? sourceFace.Face.WeightDefinition, request.Style ?? sourceFace.Face.StyleDefinition) ?? sourceFace.Face;
        }
        else
        {
            throw new InvalidOperationException("FontName must identify a registered family when changing the size or face of a custom or bitmap font.");
        }

        if (sourceFace is not null && ReferenceEquals(face, sourceFace.Face) && size == sourceFace.Size)
            return source;
        var key = new ControlFontKey(face, size);
        if (_fonts.TryGetValue(key, out IControlFont? cached))
            return cached;
        IControlFont font;
        if (_factory is not null)
            font = _factory(face, size) ?? throw new InvalidOperationException("The control font factory returned null.");
        else
        {
            if (!_systems.TryGetValue(face, out FontSystem? system))
            {
                system = new FontSystem();
                try
                {
                    system.AddFont(face.Data.ToArray());
                }
                catch
                {
                    system.Dispose();
                    throw;
                }

                _systems.Add(face, system);
            }

            font = new FontFaceControlFont(face, size, system);
        }

        _fonts.Add(key, font);
        return font;
    }

    internal void Clear()
    {
        // A custom factory may intentionally share an adapter between cache keys.
        var disposed = new HashSet<IControlFont>(ReferenceEqualityComparer.Instance);
        foreach (IControlFont font in _fonts.Values)
            if (disposed.Add(font) && font is IDisposable resource)
                resource.Dispose();
        _fonts.Clear();
        foreach (FontSystem system in _systems.Values)
            system.Dispose();
        _systems.Clear();
    }

    /// <summary>
    /// Performs the dispose operation.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        Clear();
        _disposed = true;
    }
}
