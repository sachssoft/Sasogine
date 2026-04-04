using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Presentation.Layouts
{
    internal static class LayoutMath
    {
        public static float Clamp(float value, float min, float max)
        {
            if (!float.IsNaN(min) && value < min)
                value = min;

            if (!float.IsNaN(max) && value > max)
                value = max;

            return value;
        }

        public static bool IsValidMeasure(Vector2 size)
        {
            return
                !float.IsNaN(size.X) &&
                !float.IsNaN(size.Y) &&
                size.X >= 0 &&
                size.Y >= 0 &&
                !float.IsNegativeInfinity(size.X) &&
                !float.IsNegativeInfinity(size.Y);
        }

        public static bool IsValidArrange(Bounds bounds)
        {
            return
                float.IsFinite(bounds.X) &&
                float.IsFinite(bounds.Y) &&
                float.IsFinite(bounds.Width) &&
                float.IsFinite(bounds.Height) &&
                bounds.Width >= 0 &&
                bounds.Height >= 0;
        }

        public static void ApplyConstraints(ref Vector2 value, Vector2 min, Vector2 max)
        {
            value.X = Clamp(value.X, min.X, max.X);
            value.Y = Clamp(value.Y, min.Y, max.Y);
        }

        public static Vector2 Constrain(Vector2 size, Vector2 constraint)
        {
            return new Vector2(
                float.IsNaN(constraint.X) ? size.X : float.Min(size.X, constraint.X),
                float.IsNaN(constraint.Y) ? size.Y : float.Min(size.Y, constraint.Y)
            );
        }

        // Width: NaN -> 0, negative -> 0
        public static float ResolveDimension(float value)
        {            
            if (!float.IsNaN(value) && value < 0f)
            {
                return 0f; // Negative Werte sind nicht erlaubt
            }

            return value; // NaN bleibt erhalten (interpretiert als Auto)
        }

        // MinWidth: NaN oder negative -> 0
        public static float ResolveDimensionMinimum(float value)
        {
            if (float.IsNaN(value) || value < 0f)
            {
                return 0f;
            }

            return value;
        }

        // MaxWidth: NaN -> +Infinity, negative -> 0
        public static float ResolveDimensionMaximum(float value)
        {
            if (float.IsNaN(value))
            {
                return float.PositiveInfinity;
            }

            if (value < 0f)
            {
                return 0f;
            }

            return value;
        }

    }
}
