using Sachssoft.Sasogine.Presentation.Rendering;
using Sachssoft.Sasogine.Resources;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    /// <summary>
    /// Represents a single font face (file, weight, style).
    /// Compatible with FontStashSharp.
    /// </summary>
    public sealed class FontFace
    {
        public required Resource Resource { get; init; }

        /// <summary>
        /// Weight (Normal / Bold)
        /// </summary>
        public FontWeight Weight { get; init; } = FontWeight.Normal;

        /// <summary>
        /// Style (Normal / Italic)
        /// </summary>
        public FontStyle Style { get; init; } = FontStyle.Normal;

        /// <summary>
        /// Parameterless constructor für Object Initializer
        /// </summary>
        public FontFace() { }

        /// <summary>
        /// Optional Constructor with all parameters
        /// </summary>
        [SetsRequiredMembers]
        public FontFace(Resource resource, FontWeight weight = FontWeight.Normal, FontStyle style = FontStyle.Normal)
        {
            Resource = resource;
            Weight = weight;
            Style = style;
        }

        public override string ToString()
            => $"Resource={Resource.Id} (Weight={Weight}, Style={Style})";
    }
}