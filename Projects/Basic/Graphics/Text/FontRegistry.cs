using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Provides a central registry for font families and font faces.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The registry manages the logical set of fonts available to the text
    /// rendering system independently from a concrete font backend.
    /// </para>
    /// <para>
    /// Font families are identified by name and resolved using a
    /// case-insensitive comparison.
    /// </para>
    /// </remarks>
    public sealed class FontRegistry
    {
        private readonly Dictionary<string, FontFamily> _families =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the number of registered font families.
        /// </summary>
        public int Count => _families.Count;

        /// <summary>
        /// Gets all registered font families.
        /// </summary>
        public IEnumerable<FontFamily> Families => _families.Values;

        /// <summary>
        /// Gets the names of all registered font families.
        /// </summary>
        public IEnumerable<string> FamilyNames => _families.Keys;

        /// <summary>
        /// Registers the specified font family.
        /// </summary>
        /// <param name="fontFamily">
        /// The font family to register.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fontFamily"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// A font family with the same name is already registered.
        /// </exception>
        public void Register(FontFamily fontFamily)
        {
            ArgumentNullException.ThrowIfNull(fontFamily);

            if (!_families.TryAdd(fontFamily.Name, fontFamily))
            {
                throw new ArgumentException(
                    $"A font family named '{fontFamily.Name}' is already registered.",
                    nameof(fontFamily));
            }
        }

        /// <summary>
        /// Registers a font family using the specified name and font faces.
        /// </summary>
        /// <param name="familyName">
        /// The name of the font family.
        /// </param>
        /// <param name="faces">
        /// The font faces contained in the family.
        /// </param>
        public void Register(
            string familyName,
            params FontFace[] faces)
        {
            ArgumentException.ThrowIfNullOrEmpty(familyName);
            ArgumentNullException.ThrowIfNull(faces);

            Register(
                new FontFamily(
                    familyName,
                    faces));
        }

        /// <summary>
        /// Registers the specified font families.
        /// </summary>
        /// <param name="fontFamilies">
        /// The font families to register.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fontFamilies"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterRange(IEnumerable<FontFamily> fontFamilies)
        {
            ArgumentNullException.ThrowIfNull(fontFamilies);

            foreach (FontFamily fontFamily in fontFamilies)
                Register(fontFamily);
        }

        /// <summary>
        /// Determines whether a font family with the specified name is registered.
        /// </summary>
        /// <param name="name">
        /// The font family name.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the font family is registered; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Contains(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            return _families.ContainsKey(name);
        }

        /// <summary>
        /// Gets the font family with the specified name.
        /// </summary>
        /// <param name="name">
        /// The font family name.
        /// </param>
        /// <returns>
        /// The matching font family, or <see langword="null"/> if no matching
        /// family is registered.
        /// </returns>
        public FontFamily? GetFamily(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            _families.TryGetValue(
                name,
                out FontFamily? family);

            return family;
        }

        /// <summary>
        /// Attempts to get the font family with the specified name.
        /// </summary>
        /// <param name="name">
        /// The font family name.
        /// </param>
        /// <param name="fontFamily">
        /// When this method returns, contains the matching font family if found;
        /// otherwise, <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the font family was found; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool TryGetFamily(
            string name,
            out FontFamily? fontFamily)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            return _families.TryGetValue(
                name,
                out fontFamily);
        }

        /// <summary>
        /// Gets all font faces registered in the registry.
        /// </summary>
        /// <returns>
        /// All registered font faces.
        /// </returns>
        public IEnumerable<FontFace> GetFaces()
        {
            return _families.Values.SelectMany(
                family => family.Faces);
        }

        /// <summary>
        /// Resolves the font face that best matches the specified font options.
        /// </summary>
        /// <param name="font">
        /// The font options used to resolve the font family, weight, and style.
        /// </param>
        /// <returns>
        /// The resolved font face.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="font"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The requested font family is not registered.
        /// </exception>
        public FontFace Resolve(FontOptions font)
        {
            ArgumentNullException.ThrowIfNull(font);

            FontFamily family = GetFamily(font.FontName) ??
                throw new InvalidOperationException(
                    $"Font family is not registered: {font.FontName}");

            return family.GetFace(
                font.Weight,
                font.Style);
        }

        /// <summary>
        /// Resolves the font face that best matches the specified family name,
        /// weight, and style.
        /// </summary>
        /// <param name="familyName">
        /// The font family name.
        /// </param>
        /// <param name="weight">
        /// The requested font weight.
        /// </param>
        /// <param name="style">
        /// The requested font style.
        /// </param>
        /// <returns>
        /// The resolved font face.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The requested font family is not registered.
        /// </exception>
        public FontFace Resolve(
            string familyName,
            FontWeight weight = FontWeight.Normal,
            FontStyle style = FontStyle.Normal)
        {
            ArgumentException.ThrowIfNullOrEmpty(familyName);

            FontFamily family = GetFamily(familyName) ??
                throw new InvalidOperationException(
                    $"Font family is not registered: {familyName}");

            return family.GetFace(
                weight,
                style);
        }

        /// <summary>
        /// Removes the font family with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the font family to remove.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the font family was removed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Remove(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            return _families.Remove(name);
        }

        /// <summary>
        /// Removes all registered font families.
        /// </summary>
        public void Clear()
        {
            _families.Clear();
        }

        /// <summary>
        /// Registers all fonts from this registry with the specified font backend.
        /// </summary>
        /// <param name="fontBackend">
        /// The font backend that receives the registered font families.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fontBackend"/> is <see langword="null"/>.
        /// </exception>
        public void RegisterTo(IFontBackend fontBackend)
        {
            ArgumentNullException.ThrowIfNull(fontBackend);

            foreach (FontFamily fontFamily in _families.Values)
                fontBackend.Register(fontFamily);
        }
    }
}