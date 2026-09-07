using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Defines a backend for registering and resolving font families and faces.
    /// </summary>
    /// <remarks>
    /// Implementations provide backend-specific font representations used by
    /// the text rendering system.
    /// </remarks>
    public interface IFontBackend
    {
        /// <summary>
        /// Registers a font face under the specified family name.
        /// </summary>
        /// <param name="familyName">
        /// The name of the font family.
        /// </param>
        /// <param name="face">
        /// The font face to register.
        /// </param>
        void Register(string familyName, FontFace face);

        /// <summary>
        /// Registers all font faces contained in the specified font family.
        /// </summary>
        /// <param name="fontFamily">
        /// The font family to register.
        /// </param>
        void Register(FontFamily fontFamily);

        /// <summary>
        /// Gets the names of all registered font families.
        /// </summary>
        /// <returns>
        /// The registered font family names.
        /// </returns>
        IEnumerable<string> GetFamilies();

        /// <summary>
        /// Gets all registered font faces.
        /// </summary>
        /// <returns>
        /// The registered font faces.
        /// </returns>
        IEnumerable<FontFace> GetFaces();

        /// <summary>
        /// Gets the font family with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the font family.
        /// </param>
        /// <returns>
        /// The matching font family, or <see langword="null"/> if no matching
        /// family is registered.
        /// </returns>
        FontFamily? GetFamily(string name);
    }
}