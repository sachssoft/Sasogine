using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides data for changes to an engine object's identifier or class.
    /// </summary>
    public class EngineObjectChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineObjectChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldId">Previous identifier of the engine object.</param>
        /// <param name="newId">New identifier of the engine object.</param>
        /// <param name="oldClass">Previous class of the engine object.</param>
        /// <param name="newClass">New class of the engine object.</param>
        public EngineObjectChangedEventArgs(
            string? oldId,
            string? newId,
            string? oldClass,
            string? newClass)
        {
            OldId = oldId;
            NewId = newId;
            OldClass = oldClass;
            NewClass = newClass;
        }

        /// <summary>
        /// Gets the previous identifier of the engine object.
        /// </summary>
        public string? OldId { get; }

        /// <summary>
        /// Gets the new identifier of the engine object.
        /// </summary>
        public string? NewId { get; }

        /// <summary>
        /// Gets the previous class of the engine object.
        /// </summary>
        public string? OldClass { get; }

        /// <summary>
        /// Gets the new class of the engine object.
        /// </summary>
        public string? NewClass { get; }
    }
}