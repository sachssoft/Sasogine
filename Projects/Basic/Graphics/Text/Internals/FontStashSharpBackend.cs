using FontStashSharp;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Graphics.Text.Internals
{
    /// <summary>
    /// Provides a FontStashSharp-based implementation of
    /// <see cref="IFontBackend"/>.
    /// </summary>
    /// <remarks>
    /// The backend converts registered <see cref="FontFace"/> instances into
    /// FontStashSharp font systems and caches runtime fonts by face and size.
    /// </remarks>
    internal sealed class FontStashSharpBackend : IFontBackend
    {
        private readonly Dictionary<string, List<FontFaceEntry>> _fonts =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<(FontFace Face, int Size), SpriteFontBase> _cache =
            new();

        private sealed record FontFaceEntry(
            FontFace Face,
            FontSystem System);

        /// <summary>
        /// Registers all font faces contained in the specified font family.
        /// </summary>
        /// <param name="fontFamily">
        /// The font family to register.
        /// </param>
        public void Register(FontFamily fontFamily)
        {
            ArgumentNullException.ThrowIfNull(fontFamily);

            foreach (FontFace face in fontFamily.Faces)
                Register(fontFamily.Name, face);
        }

        /// <summary>
        /// Registers a font face under the specified family name.
        /// </summary>
        /// <param name="familyName">
        /// The name of the font family.
        /// </param>
        /// <param name="face">
        /// The font face to register.
        /// </param>
        /// <exception cref="ArgumentException">
        /// A face with the same weight and style is already registered for the
        /// specified family.
        /// </exception>
        public void Register(string familyName, FontFace face)
        {
            ArgumentException.ThrowIfNullOrEmpty(familyName);
            ArgumentNullException.ThrowIfNull(face);

            if (!_fonts.TryGetValue(
                familyName,
                out List<FontFaceEntry>? entries))
            {
                entries = new List<FontFaceEntry>();
                _fonts.Add(familyName, entries);
            }

            bool exists = entries.Any(entry =>
                entry.Face.WeightDefinition == face.WeightDefinition &&
                entry.Face.StyleDefinition == face.StyleDefinition);

            if (exists)
            {
                throw new ArgumentException(
                    $"The font family '{familyName}' already contains a " +
                    $"'{face.WeightDefinition} {face.StyleDefinition}' face.",
                    nameof(face));
            }

            var system = new FontSystem();

            system.AddFont(face.Data.ToArray());

            entries.Add(
                new FontFaceEntry(
                    face,
                    system));
        }

        /// <summary>
        /// Gets the names of all registered font families.
        /// </summary>
        /// <returns>
        /// The registered font family names.
        /// </returns>
        public IEnumerable<string> GetFamilies()
        {
            return _fonts.Keys;
        }

        /// <summary>
        /// Gets all registered font faces.
        /// </summary>
        /// <returns>
        /// The registered font faces.
        /// </returns>
        public IEnumerable<FontFace> GetFaces()
        {
            foreach (List<FontFaceEntry> entries in _fonts.Values)
            {
                foreach (FontFaceEntry entry in entries)
                    yield return entry.Face;
            }
        }

        /// <summary>
        /// Gets the font family with the specified name.
        /// </summary>
        /// <param name="name">
        /// The font family name.
        /// </param>
        /// <returns>
        /// The matching font family, or <see langword="null"/> if the family is
        /// not registered.
        /// </returns>
        public FontFamily? GetFamily(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            if (!_fonts.TryGetValue(
                name,
                out List<FontFaceEntry>? entries))
            {
                return null;
            }

            return new FontFamily(
                name,
                entries
                    .Select(entry => entry.Face)
                    .ToArray());
        }

        /// <summary>
        /// Gets or creates the runtime font for the specified face and size.
        /// </summary>
        /// <param name="face">
        /// The registered font face.
        /// </param>
        /// <param name="size">
        /// The font size.
        /// </param>
        /// <returns>
        /// The cached or newly created runtime font.
        /// </returns>
        internal SpriteFontBase GetSpriteFont(
            FontFace face,
            int size)
        {
            ArgumentNullException.ThrowIfNull(face);

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            var key = (face, size);

            if (_cache.TryGetValue(
                key,
                out SpriteFontBase? cached))
            {
                return cached;
            }

            FontFaceEntry entry = FindEntry(face);

            SpriteFontBase font = entry.System.GetFont(size);

            _cache.Add(key, font);

            return font;
        }

        /// <summary>
        /// Gets or creates the runtime font matching the specified font options.
        /// </summary>
        /// <param name="font">
        /// The font options used to resolve the font face and size.
        /// </param>
        /// <returns>
        /// The cached or newly created runtime font.
        /// </returns>
        internal SpriteFontBase GetOrCreateSpriteFont(FontOptions font)
        {
            ArgumentNullException.ThrowIfNull(font);

            FontFace face = ResolveFace(font);

            return GetSpriteFont(
                face,
                font.Size);
        }

        private FontFace ResolveFace(FontOptions font)
        {
            FontFamily family = GetFamily(font.FontName) ??
                throw new InvalidOperationException(
                    $"Font family is not registered: {font.FontName}");

            return family.GetFace(
                font.Weight,
                font.Style);
        }

        private FontFaceEntry FindEntry(FontFace face)
        {
            foreach (List<FontFaceEntry> entries in _fonts.Values)
            {
                foreach (FontFaceEntry entry in entries)
                {
                    if (ReferenceEquals(entry.Face, face))
                        return entry;
                }
            }

            throw new InvalidOperationException(
                $"Font face is not registered: {face.Name}");
        }
    }
}