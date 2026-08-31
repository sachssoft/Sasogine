using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools
{

    /// <summary>
    /// Provides information about an object insertion operation.
    /// </summary>
    public sealed class ObjectInsertContext
    {
        /// <summary>
        /// Gets the final insertion position.
        /// </summary>
        public Vector2 Position { get; init; }

        /// <summary>
        /// Gets the final insertion size.
        /// </summary>
        public Size2 Size { get; init; }

        /// <summary>
        /// Gets the position where the insertion operation started.
        /// </summary>
        public Vector2 StartPosition { get; init; }

        /// <summary>
        /// Gets the position where the insertion operation ended.
        /// </summary>
        public Vector2 EndPosition { get; init; }

        /// <summary>
        /// Gets a value indicating whether the insertion was performed by dragging.
        /// </summary>
        public bool IsDrag { get; init; }
    }
}