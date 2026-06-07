using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Represents a single font face (file, weight, style).
    /// Compatible with FontStashSharp.
    /// </summary>
    public sealed class FontFace
    {
        public required string Name { get; init; }

        /// <summary>
        /// Weight (Normal / Bold)
        /// </summary>
        public FontWeight WeightDefinition { get; init; } = FontWeight.Normal;

        /// <summary>
        /// Style (Normal / Italic)
        /// </summary>
        public FontStyle StyleDefinition { get; init; } = FontStyle.Normal;

        public required ResourceSourceBase Loader { get; init; }

        /// <summary>
        /// Parameterless constructor für Object Initializer
        /// </summary>
        public FontFace() { }

        /// <summary>
        /// Optional Constructor with all parameters
        /// </summary>
        [SetsRequiredMembers]
        public FontFace(ResourceSourceBase loader, string name, FontWeight weightDefinition = FontWeight.Normal, FontStyle styleDefinition = FontStyle.Normal)
        {
            Loader = loader;
            Name = name;
            WeightDefinition = weightDefinition;
            StyleDefinition = styleDefinition;
        }
    }
}