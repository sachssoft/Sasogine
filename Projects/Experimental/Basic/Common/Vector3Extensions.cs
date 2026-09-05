using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Provides extension methods for converting <see cref="Vector3"/> values
    /// to Sasogine point and size types.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Converts the specified vector to a <see cref="Point3"/>.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The converted point.</returns>
        public static Point3 ToPoint3(this Vector3 value)
        {
            return new Point3(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelPoint3"/>.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The converted pixel point.</returns>
        public static PixelPoint3 ToPixelPoint3(this Vector3 value)
        {
            return new PixelPoint3(
                (int)value.X,
                (int)value.Y,
                (int)value.Z);
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="Size3"/>.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The converted size.</returns>
        public static Size3 ToSize3(this Vector3 value)
        {
            return new Size3(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelSize3"/>.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The converted pixel size.</returns>
        public static PixelSize3 ToPixelSize3(this Vector3 value)
        {
            return new PixelSize3(
                (int)value.X,
                (int)value.Y,
                (int)value.Z);
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelPoint3"/>
        /// by rounding its components to the nearest integer values.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The rounded pixel point.</returns>
        public static PixelPoint3 RoundToPixelPoint3(this Vector3 value)
        {
            return new PixelPoint3(
                (int)System.MathF.Round(value.X),
                (int)System.MathF.Round(value.Y),
                (int)System.MathF.Round(value.Z));
        }

        /// <summary>
        /// Converts the specified vector to a <see cref="PixelSize3"/>
        /// by rounding its components to the nearest integer values.
        /// </summary>
        /// <param name="value">The vector to convert.</param>
        /// <returns>The rounded pixel size.</returns>
        public static PixelSize3 RoundToPixelSize3(this Vector3 value)
        {
            return new PixelSize3(
                (int)System.MathF.Round(value.X),
                (int)System.MathF.Round(value.Y),
                (int)System.MathF.Round(value.Z));
        }
    }
}