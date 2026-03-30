namespace Sachssoft.Sasogine.Presentation.Deterlite.Layouts
{
    internal static class LayoutMath
    {
        public static float Clamp(float value, float min, float max)
        {
            if (!float.IsNaN(min))
                value = float.Max(value, min);

            if (!float.IsNaN(max))
                value = float.Min(value, max);

            return value;
        }
    }
}
