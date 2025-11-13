using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Localization
{
    /// <summary>
    /// Represents a localized text entry.
    /// Implements <see cref="ILocalizedEntryWrapper"/> and stores a string value.
    /// </summary>
    public class LocalizedString : ILocalizedEntryWrapper
    {
        /// <summary>
        /// The actual localized text value.
        /// </summary>
        public string? Value { get; private set; } = null;

        /// <summary>
        /// Initializes a new instance of <see cref="LocalizedString"/>.
        /// </summary>
        public LocalizedString() { }

        /// <summary>
        /// Loads the localization entry from a dictionary of attributes.
        /// Expects an attribute "Value" containing the text.
        /// </summary>
        /// <param name="attributes">The attributes dictionary from XML.</param>
        /// <exception cref="ArgumentNullException">Thrown if attributes is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if "Value" attribute is missing.</exception>
        public void Load(Dictionary<string, string?> attributes, LoaderBase? loader)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));

            if (!attributes.TryGetValue("Value", out var value) || value == null)
                return;

            Value = value;
        }

        /// <summary>
        /// Implicit conversion to string for convenience.
        /// </summary>
        /// <param name="entry">The <see cref="LocalizedString"/> instance.</param>
        public static implicit operator string?(LocalizedString entry) => entry.Value;

        object? ILocalizedEntryWrapper.Value => Value;
    }
}
