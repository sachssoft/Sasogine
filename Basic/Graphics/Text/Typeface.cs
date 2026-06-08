using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Represents a font family containing multiple font faces (weight + style variants).
    /// Provides fast lookup and deterministic fallback.
    /// </summary>
    public sealed class FontFamily
    {
        private readonly List<FontFace> _faces = new();
        private readonly Dictionary<FontKey, FontFace> _lookup;

        /// <summary>
        /// Immutable record for lookup key (Weight + Style).
        /// </summary>
        private record FontKey(FontWeight Weight, FontStyle Style);

        /// <summary>
        /// Name of the font family (e.g., "Arial", "Roboto").
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// All registered FontFaces in this family.
        /// </summary>
        public IReadOnlyList<FontFace> Faces => _faces;

        /// <summary>
        /// Creates a FontFamily with a name and one or more FontFaces.
        /// </summary>
        /// <param name="name">The font family name</param>
        /// <param name="faces">Array of FontFace instances (must contain at least one)</param>
        public FontFamily(string name, params FontFace[] faces)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            if (faces == null || faces.Length == 0)
                throw new ArgumentException("FontFamily must have at least one FontVariant.");

            // Deduplicate faces by Weight + Style
            foreach (var face in faces)
            {
                if (_faces.Any(f => f.WeightDefinition == face.WeightDefinition && f.StyleDefinition == face.StyleDefinition))
                    continue;
                _faces.Add(face);
            }

            // Deterministically sort faces for consistent lookup
            _faces.Sort((a, b) =>
            {
                int cmp = ((int)a.WeightDefinition).CompareTo((int)b.WeightDefinition);
                return cmp != 0 ? cmp : a.StyleDefinition.CompareTo(b.StyleDefinition);
            });

            // Build dictionary for fast lookup
            _lookup = _faces.ToDictionary(f => new FontKey(f.WeightDefinition, f.StyleDefinition), f => f);
        }

        /// <summary>
        /// Get the FontFace matching the requested weight and style.
        /// Fallbacks:
        /// 1. Exact match (weight + style)
        /// 2. Normal weight, same style
        /// 3. Normal weight + Normal style
        /// </summary>
        /// <param name="weight">Requested font weight</param>
        /// <param name="style">Requested font style</param>
        /// <returns>FontFace matching or fallback</returns>
        public FontFace GetFace(FontWeight weight, FontStyle style)
        {
            var key = new FontKey(weight, style);

            // 1. Exact match
            if (_lookup.TryGetValue(key, out var exact))
                return exact;

            // 2. Fallback: Normal weight, same style
            key = new FontKey(FontWeight.Normal, style);
            if (_lookup.TryGetValue(key, out var fallback))
                return fallback;

            // 3. Last fallback: Normal weight + Normal style
            return _lookup[new FontKey(FontWeight.Normal, FontStyle.Normal)];
        }
    }
}