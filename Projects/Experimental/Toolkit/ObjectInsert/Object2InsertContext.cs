using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Provides information about a 2D object insertion operation.
    /// </summary>
    public sealed class Object2InsertContext
    {
        /// <summary>
        /// Gets the final position of the inserted object.
        /// </summary>
        public Vector2 Position { get; init; }

        /// <summary>
        /// Gets the final size of the inserted object.
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