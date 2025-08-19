using Sachssoft.Sasogine.Resources.Wrappers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Sachssoft.Sasogine.Containers
{
    /// <summary>
    /// Provides access to localized license text files within a package.
    /// Supports reading, writing and enumeration of license files by culture.
    /// </summary>
    public sealed class PackageLicense
    {
        private const string FILE_NAME = "license";
        private const string FILE_EXTENSION = ".txt";
        private const string FILE_FORMAT = FILE_NAME + "{0}" + FILE_EXTENSION;

        private readonly PackageBase _package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageLicense"/> class for the specified package.
        /// </summary>
        /// <param name="package">The package containing the license files.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="package"/> is null.</exception>
        internal PackageLicense(PackageBase package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
        }

        /// <summary>
        /// Gets the license text wrapper for the specified culture.
        /// </summary>
        /// <param name="culture">The culture for which to retrieve the license text.</param>
        /// <returns>A <see cref="TextWrapper"/> containing the license text, or <c>null</c> if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="culture"/> is null.</exception>
        public TextWrapper? this[CultureInfo culture] => Get(culture);

        /// <summary>
        /// Gets the default license text wrapper for the invariant culture.
        /// </summary>
        public TextWrapper? Default => Get(CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the license text wrapper for the invariant culture.
        /// </summary>
        /// <returns>A <see cref="TextWrapper"/> for the invariant culture license, or <c>null</c> if not found.</returns>
        public TextWrapper? Get() => Get(CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the license text wrapper for the specified culture.
        /// </summary>
        /// <param name="culture">The culture to retrieve the license text for.</param>
        /// <returns>A <see cref="TextWrapper"/> containing the license text, or <c>null</c> if no license exists for the culture.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="culture"/> is null.</exception>
        public TextWrapper? Get(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));

            string fileName = culture.Equals(CultureInfo.InvariantCulture)
                ? string.Format(FILE_FORMAT, "")
                : string.Format(FILE_FORMAT, "_" + culture.TwoLetterISOLanguageName.ToLowerInvariant());

            var stream = _package.OpenFile(fileName);
            if (stream == null)
                return null;

            return new TextWrapper(_package, p => p.OpenFile(fileName));
        }

        /// <summary>
        /// Sets the default (invariant culture) license text from a string.
        /// </summary>
        /// <param name="content">The license text content.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="content"/> is null.</exception>
        public void Set(string content) => Set(content, CultureInfo.InvariantCulture);

        /// <summary>
        /// Sets the default (invariant culture) license text from a text stream.
        /// </summary>
        /// <param name="textStream">A stream containing the license text.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="textStream"/> is null.</exception>
        public void Set(Stream textStream) => Set(textStream, CultureInfo.InvariantCulture);

        /// <summary>
        /// Sets the license text for the specified culture from a string.
        /// </summary>
        /// <param name="content">The license text content.</param>
        /// <param name="culture">The culture of the license text.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="content"/> or <paramref name="culture"/> is null.</exception>
        public void Set(string content, CultureInfo culture)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            Set(stream, culture);
        }

        /// <summary>
        /// Sets the license text for the specified culture from a text stream.
        /// The stream is read from its current position to the end.
        /// </summary>
        /// <param name="textStream">A stream containing the license text.</param>
        /// <param name="culture">The culture of the license text.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="textStream"/> or <paramref name="culture"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the package is not opened or is read-only.</exception>
        public void Set(Stream textStream, CultureInfo culture)
        {
            if (textStream == null)
                throw new ArgumentNullException(nameof(textStream));
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));
            if (!_package.IsOpen)
                throw new InvalidOperationException("Package is not opened.");
            if (_package.IsReadOnly)
                throw new InvalidOperationException("Package is readonly.");

            string fileName = culture.Equals(CultureInfo.InvariantCulture)
                ? string.Format(FILE_FORMAT, "")
                : string.Format(FILE_FORMAT, "_" + culture.TwoLetterISOLanguageName.ToLowerInvariant());

            // Delete existing entry if present
            var existingEntry = _package.Source.GetEntry(fileName);
            existingEntry?.Delete();

            // Create new entry and write content from stream
            var newEntry = _package.Source.CreateEntry(fileName);
            using var entryStream = newEntry.Open();

            if (textStream.CanSeek)
                textStream.Seek(0, SeekOrigin.Begin);

            textStream.CopyTo(entryStream);
        }

        /// <summary>
        /// Determines whether a license file exists for the specified culture.
        /// </summary>
        /// <param name="culture">The culture to check.</param>
        /// <returns><c>true</c> if the license file exists; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="culture"/> is null.</exception>
        public bool Contains(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));

            string fileName = culture.Equals(CultureInfo.InvariantCulture)
                ? string.Format(FILE_FORMAT, "")
                : string.Format(FILE_FORMAT, "_" + culture.TwoLetterISOLanguageName.ToLowerInvariant());

            return _package.IsFileExists(fileName);
        }

        /// <summary>
        /// Enumerates all cultures for which license files are available in the package.
        /// </summary>
        /// <returns>An enumerable of <see cref="CultureInfo"/> objects representing available license cultures.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the package is not opened.</exception>
        public IEnumerable<CultureInfo> FindAvailableCultures()
        {
            _package.ThrowIfNotOpened();

            // Pattern for "license.txt" or "license_de.txt"
            string pattern = $"^{Regex.Escape(FILE_NAME)}(?:_([a-z]{{2}}))?{Regex.Escape(FILE_EXTENSION)}$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            foreach (var entry in _package.Source.Entries)
            {
                if (!entry.FullName.StartsWith(FILE_NAME, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var match = regex.Match(entry.Name);
                if (!match.Success)
                    continue;

                string langCode = match.Groups[1].Value;

                if (string.IsNullOrEmpty(langCode))
                {
                    yield return CultureInfo.InvariantCulture;
                }
                else
                {
                    CultureInfo culture;
                    try
                    {
                        culture = CultureInfo.GetCultureInfo(langCode);
                    }
                    catch (CultureNotFoundException)
                    {
                        continue; // Ignore unknown language codes
                    }

                    yield return culture;
                }
            }
        }
    }
}
