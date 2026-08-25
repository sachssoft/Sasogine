using LibTessDotNet.Double;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Internal
{
    internal class LibTessPolygonTriangulator : IPolygonTriangulator
    {
        public PolygonTriangulationResult Triangulate(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonTriangulationOptions options)
        {
            var tess = new Tess();

            foreach (var contour in contours)
            {
                if (contour.Count < 3)
                    continue;

                var vertices = new ContourVertex[contour.Count];

                for (var i = 0; i < contour.Count; i++)
                {
                    var position = contour[i];

                    vertices[i].Position = new Vec3(
                        position.X,
                        position.Y,
                        0.0);
                }

                tess.AddContour(
                    vertices,
                    ContourOrientation.Original);
            }

            tess.Tessellate(
                WindingRule.EvenOdd,
                ElementType.Polygons,
                3);

            var resultVertices = new Vector2[tess.VertexCount];

            for (var i = 0; i < tess.VertexCount; i++)
            {
                var position = tess.Vertices[i].Position;

                resultVertices[i] = new Vector2(
                    (float)position.X,
                    (float)position.Y);
            }

            var resultIndices = new int[tess.ElementCount * 3];

            Array.Copy(
                tess.Elements,
                resultIndices,
                resultIndices.Length);

            return new PolygonTriangulationResult(
                resultVertices,
                resultIndices);
        }
    }
}