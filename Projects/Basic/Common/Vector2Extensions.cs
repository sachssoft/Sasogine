using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides extension methods for converting <see cref="Vector2"/> values
    /// to Sasogine point and size types.
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Converts the specified vector to a <see cref="Point2"/>.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The converted point.</returns>
        public static Point2 ToPoint2(this Vector2 value)
        {
            return new Point2(value.X, value.Y);
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelPoint2"/>.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The converted pixel point.</returns>
        public static PixelPoint2 ToPixelPoint2(this Vector2 value)
        {
            return new PixelPoint2(
                (int)value.X,
                (int)value.Y);
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="Size2"/>.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The converted size.</returns>
        public static Size2 ToSize2(this Vector2 value)
        {
            return new Size2(value.X, value.Y);
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelSize2"/>.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The converted pixel size.</returns>
        public static PixelSize2 ToPixelSize2(this Vector2 value)
        {
            return new PixelSize2(
                (int)value.X,
                (int)value.Y);
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelPoint2"/>
        /// by rounding its components to the nearest integer values.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The rounded pixel point.</returns>
        public static PixelPoint2 RoundToPixelPoint2(this Vector2 value)
        {
            return new PixelPoint2(
                (int)System.MathF.Round(value.X),
                (int)System.MathF.Round(value.Y));
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelSize2"/>
        /// by rounding its components to the nearest integer values.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The rounded pixel size.</returns>
        public static PixelSize2 RoundToPixelSize2(this Vector2 value)
        {
            return new PixelSize2(
                (int)System.MathF.Round(value.X),
                (int)System.MathF.Round(value.Y));
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelPoint2"/>
        /// by rounding its components down.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The floored pixel point.</returns>
        public static PixelPoint2 FloorToPixelPoint2(this Vector2 value)
        {
            return new PixelPoint2(
                (int)System.MathF.Floor(value.X),
                (int)System.MathF.Floor(value.Y));
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelSize2"/>
        /// by rounding its components down.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The floored pixel size.</returns>
        public static PixelSize2 FloorToPixelSize2(this Vector2 value)
        {
            return new PixelSize2(
                (int)System.MathF.Floor(value.X),
                (int)System.MathF.Floor(value.Y));
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelPoint2"/>
        /// by rounding its components up.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The ceiling pixel point.</returns>
        public static PixelPoint2 CeilingToPixelPoint2(this Vector2 value)
        {
            return new PixelPoint2(
                (int)System.MathF.Ceiling(value.X),
                (int)System.MathF.Ceiling(value.Y));
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelSize2"/>
        /// by rounding its components up.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The ceiling pixel size.</returns>
        public static PixelSize2 CeilingToPixelSize2(this Vector2 value)
        {
            return new PixelSize2(
                (int)System.MathF.Ceiling(value.X),
                (int)System.MathF.Ceiling(value.Y));
        }
    }
}