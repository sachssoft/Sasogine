using System;

namespace Sachssoft.Sasogine.Components.Models
{
    /// <summary>
    /// Defines numeric editor settings for a property.
    /// Supports different numeric property types by storing values as objects
    /// and converting them according to the property's actual type at runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NumericPropertyEditorAttribute : PropertyEditorAttribute
    {
        /// <summary>
        /// Gets or sets the minimum allowed value.
        /// </summary>
        public object? Minimum { get; init; }

        /// <summary>
        /// Gets or sets the maximum allowed value.
        /// </summary>
        public object? Maximum { get; init; }

        /// <summary>
        /// Gets or sets the small increment/decrement step.
        /// </summary>
        public object? SmallChange { get; init; }

        /// <summary>
        /// Gets or sets the large increment/decrement step.
        /// </summary>
        public object? LargeChange { get; init; }

        /// <summary>
        /// Gets or sets the display format string.
        /// </summary>
        public string? Format { get; init; }

        /// <summary>
        /// Indicates whether minimum and maximum bounds are defined.
        /// </summary>
        public bool HasBounds =>
            Minimum != null && Maximum != null;
    }
}