using LibTessDotNet;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System;
using System.Collections.Generic;


namespace Sachssoft.Sasogine.Experimental.Graphics.Meshes;

internal class LibTessPolygonTriangulator : IPolygonTriangulator
{
    public PolygonTriangulationResult Triangulate(IReadOnlyList<IReadOnlyList<Vector2>> contours, PolygonTriangulationOptions options)
    {
        Tess tess = new Tess();
        foreach (IReadOnlyList<Vector2> contour in contours)
        {
            if (contour.Count >= 3)
            {
                ContourVertex[] array = new ContourVertex[contour.Count];
                for (int i = 0; i < contour.Count; i++)
                {
                    Vector2 vector = contour[i];
                    array[i].Position = new Vec3(vector.X, vector.Y, 0.0f);
                }

                tess.AddContour(array);
            }
        }

        tess.Tessellate();
        Vector2[] array2 = new Vector2[tess.VertexCount];
        for (int j = 0; j < tess.VertexCount; j++)
        {
            Vec3 position = tess.Vertices[j].Position;
            array2[j] = new Vector2((float)position.X, (float)position.Y);
        }

        int[] array3 = new int[tess.ElementCount * 3];
        Array.Copy(tess.Elements, array3, array3.Length);
        return new PolygonTriangulationResult(array2, array3);
    }
}