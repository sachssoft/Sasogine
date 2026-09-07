using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Represents a font family containing multiple font faces with different
    /// weights and styles.
    /// </summary>
    /// <remarks>
    /// Font faces are uniquely identified within the family by their combination
    /// of <see cref="FontWeight"/> and <see cref="FontStyle"/>.
    /// </remarks>
    public sealed class FontFamily
    {
        private readonly List<FontFace> _faces = new();
        private readonly Dictionary<FontKey, FontFace> _lookup;

        private readonly record struct FontKey(
            FontWeight Weight,
            FontStyle Style);

        /// <summary>
        /// Initializes a new instance of the <see cref="FontFamily"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the font family.
        /// </param>
        /// <param name="faces">
        /// The font faces contained in the family.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <paramref name="faces"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="faces"/> does not contain any font faces.
        /// </exception>
        public FontFamily(string name, params FontFace[] faces)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(faces);

            if (faces.Length == 0)
            {
                throw new ArgumentException(
                    "The font family must contain at least one font face.",
                    nameof(faces));
            }

            Name = name;

            foreach (FontFace face in faces)
            {
                ArgumentNullException.ThrowIfNull(face);

                bool exists = _faces.Any(existing =>
                    existing.WeightDefinition == face.WeightDefinition &&
                    existing.StyleDefinition == face.StyleDefinition);

                if (!exists)
                    _faces.Add(face);
            }

            _faces.Sort((a, b) =>
            {
                int result = a.WeightDefinition.CompareTo(b.WeightDefinition);

                if (result != 0)
                    return result;

                return a.StyleDefinition.CompareTo(b.StyleDefinition);
            });

            _lookup = _faces.ToDictionary(
                face => new FontKey(
                    face.WeightDefinition,
                    face.StyleDefinition));
        }

        /// <summary>
        /// Gets the name of the font family.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the font faces contained in the family.
        /// </summary>
        public IReadOnlyList<FontFace> Faces => _faces;

        /// <summary>
        /// Gets the font face that best matches the specified weight and style.
        /// </summary>
        /// <param name="weight">
        /// The requested font weight.
        /// </param>
        /// <param name="style">
        /// The requested font style.
        /// </param>
        /// <returns>
        /// The best matching font face.
        /// </returns>
        /// <remarks>
        /// Resolution prefers an exact match, followed by normal weight with
        /// the requested style, normal weight with normal style, and finally
        /// the first available face.
        /// </remarks>
        public FontFace GetFace(
            FontWeight weight,
            FontStyle style)
        {
            if (_lookup.TryGetValue(
                new FontKey(weight, style),
                out FontFace? face))
            {
                return face;
            }

            if (_lookup.TryGetValue(
                new FontKey(FontWeight.Normal, style),
                out face))
            {
                return face;
            }

            if (_lookup.TryGetValue(
                new FontKey(FontWeight.Normal, FontStyle.Normal),
                out face))
            {
                return face;
            }

            return _faces[0];
        }
    }
}