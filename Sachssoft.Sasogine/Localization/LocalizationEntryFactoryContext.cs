using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Localization
{
    /// <summary>
    /// Context passed to a localization entry factory when creating a wrapper.
    /// Contains attributes, loader, loader options, and allows canceling creation.
    /// </summary>
    public class LocalizationEntryFactoryContext
    {
        public LocalizationEntryFactoryContext(Dictionary<string, string?> attributes, LoaderOptions loaderOptions)
        {
            Attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            LoaderOptions = loaderOptions ?? throw new ArgumentNullException(nameof(loaderOptions));
        }

        /// <summary>
        /// The attributes read from the XML element.
        /// </summary>
        public Dictionary<string, string?> Attributes { get; }

        /// <summary>
        /// Loader options provided when loading the dictionary.
        /// </summary>
        public LoaderOptions LoaderOptions { get; }

        public string? FilePath { get; set; }

        /// <summary>
        /// Can be set to true to cancel creation of this entry.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// The function that creates the ILocalizationEntryWrapper instance.
        /// </summary>
        [AllowNull]
        public Func<ILocalizedEntryWrapper> Instance { get; set; }
    }
}
