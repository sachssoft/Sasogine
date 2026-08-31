using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides utility methods for creating, validating, and ensuring
    /// globally unique identifiers represented as strings.
    /// </summary>
    public static class IdentifierFactory
    {
        /// <summary>
        /// Creates a new identifier using the compact GUID format
        /// without hyphens.
        /// </summary>
        /// <returns>
        /// A new 32-character GUID identifier.
        /// </returns>
        public static string Create()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Creates a new identifier using the standard GUID format
        /// with hyphens.
        /// </summary>
        /// <returns>
        /// A new GUID identifier in the standard 8-4-4-4-12 format.
        /// </returns>
        public static string CreatePretty()
        {
            return Guid.NewGuid().ToString("D");
        }

        /// <summary>
        /// Determines whether the specified identifier represents
        /// a valid GUID.
        /// </summary>
        /// <param name="id">Identifier to validate.</param>
        /// <returns>
        /// True if the identifier represents a valid GUID; otherwise false.
        /// </returns>
        public static bool IsValid(string? id)
        {
            return Guid.TryParse(id, out _);
        }

        /// <summary>
        /// Returns the specified identifier when it is valid;
        /// otherwise creates and returns a new identifier.
        /// </summary>
        /// <param name="id">Identifier to validate.</param>
        /// <returns>
        /// The existing valid identifier or a newly created identifier.
        /// </returns>
        public static string EnsureOrCreate(string? id)
        {
            return IsValid(id)
                ? id!
                : Create();
        }
    }
}