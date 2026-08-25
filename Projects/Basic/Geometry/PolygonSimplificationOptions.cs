using System;

namespace Sachssoft.Sasogine.Geometry
{
    public sealed class PolygonSimplificationOptions
    {
        public float Tolerance { get; }

        public PolygonSimplificationOptions(float tolerance = 0.01f)
        {
            if (tolerance < 0f)
                throw new ArgumentOutOfRangeException(nameof(tolerance));

            Tolerance = tolerance;
        }
    }
}