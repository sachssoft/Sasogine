using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Defines the local transformation applied to rendered text.
    /// </summary>
    public readonly struct TextTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextTransform"/> structure.
        /// </summary>
        /// <param name="position">
        /// The position of the text.
        /// </param>
        /// <param name="rotation">
        /// The rotation of the text in radians.
        /// </param>
        /// <param name="scale">
        /// The scale applied to the text.
        /// </param>
        /// <param name="pivot">
        /// The local pivot point used for rotation and scaling.
        /// </param>
        public TextTransform(
            Point2 position,
            float rotation = 0f,
            Vector2? scale = null,
            Point2? pivot = null)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale ?? Vector2.One;
            Pivot = pivot ?? Point2.Zero;
        }

        /// <summary>
        /// Gets the position of the text.
        /// </summary>
        public Point2 Position { get; }

        /// <summary>
        /// Gets the rotation of the text in radians.
        /// </summary>
        public float Rotation { get; }

        /// <summary>
        /// Gets the scale applied to the text.
        /// </summary>
        public Vector2 Scale { get; }

        /// <summary>
        /// Gets the local pivot point used for rotation and scaling.
        /// </summary>
        public Point2 Pivot { get; }
    }
}