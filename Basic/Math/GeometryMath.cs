using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Math
{
    public static class GeometryMath
    {
        /// <summary>
        /// Finds the nearest polygon index to a location in a Path.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void GetNearestPolygon(Vector2 location, Sasogine.Geometry.Path path, out int nearest_polygon_index)
        {
            float nearest_distance = float.PositiveInfinity;
            nearest_polygon_index = -1;

            // Alle Polygone durchsuchen
            for (int i = 0; i < path.GetPolygonCount(); i++)
            {
                for (int j = 0; j < path.GetPointCount(i); j++)
                {
                    var point = path.GetPoint(i, j);
                    var distance = (location - point).Length();

                    // Kleinster Abstand merken
                    if (distance < nearest_distance)
                    {
                        nearest_distance = distance;
                        nearest_polygon_index = i;
                    }
                }
            }
        }
    }
}
