using System;

namespace Sachssoft.Sasogine.Basic.Components.Models
{
    /// <summary>
    /// Defines color editor settings for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ColorPropertyEditorAttribute : PropertyEditorAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPropertyEditorAttribute"/> class.
        /// </summary>
        public ColorPropertyEditorAttribute()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the color editor supports an alpha channel.
        /// </summary>
        public bool AllowAlpha { get; init; } = false;
    }
}