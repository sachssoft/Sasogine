using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Geometry
{
    public static class GeometryUtils
    {

        ///// <summary>
        ///// Creates a wide line polygon with symmetric thickness.
        ///// </summary>
        //// Rechteck-Polygon mit gleicher Breite auf beiden Seiten
        //public static Vertices CreateWidedLine(Vector2 start, Vector2 end, float thickness)
        //{
        //    return CreateWidedLine(start, end, thickness / 2f, thickness / 2f);
        //}

        ///// <summary>
        ///// Creates a wide line polygon with asymmetric thickness.
        ///// </summary>
        //// Rechteck-Polygon mit unterschiedlichen Breiten
        //public static Vertices CreateWidedLine(Vector2 start, Vector2 end, float positiveWidth, float negativeWidth)
        //{
        //    // Breiten auf mindestens 0 begrenzen
        //    positiveWidth = MathF.Max(0f, positiveWidth);
        //    negativeWidth = MathF.Max(0f, negativeWidth);

        //    // Richtung von start nach end
        //    var delta = end - start;
        //    var length = delta.Length();
        //    if (length < 1e-6f)
        //        length = 1f; // Division durch Null vermeiden

        //    // Normalisierter senkrechter Vektor
        //    var perp = new Vector2(-delta.Y, delta.X) / length;

        //    // Vertices erzeugen
        //    var vertices = new Vertices
        //    {
        //        start - perp * negativeWidth,
        //        start + perp * positiveWidth,
        //        end + perp * positiveWidth,
        //        end - perp * negativeWidth
        //    };

        //    return vertices;
        //}

    }
}
