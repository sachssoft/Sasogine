using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry.Internal;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry
{
    public static class PolygonOperations
    {
        public static IReadOnlyList<IReadOnlyList<Vector2>> Stroke(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonStrokeOptions options,
            IPolygonStroker? strokerBackend = null)
        {
            strokerBackend ??= new Clipper2PolygonStroker();

            return strokerBackend.Stroke(
                contours,
                options);
        }

        public static PolygonTriangulationResult Triangulate(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonTriangulationOptions options,
            IPolygonTriangulator? triangulatorBackend = null)
        {
            triangulatorBackend ??= new LibTessPolygonTriangulator();

            return triangulatorBackend.Triangulate(
                contours,
                options);
        }

        public static IReadOnlyList<IReadOnlyList<Vector2>> Clip(
            IReadOnlyList<IReadOnlyList<Vector2>> subject,
            IReadOnlyList<IReadOnlyList<Vector2>> clip,
            PolygonClipOperation operation = PolygonClipOperation.Union,
            IPolygonClipper? clipperBackend = null)
        {
            clipperBackend ??= new Clipper2PolygonClipper();

            return clipperBackend.Clip(
                subject,
                clip,
                operation);
        }

        public static IReadOnlyList<IReadOnlyList<Vector2>> Simplify(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonSimplificationOptions options = default,
            IPolygonSimplifier? simplifierBackend = null)
        {
            simplifierBackend ??= new Clipper2PolygonSimplifier();

            return simplifierBackend.Simplify(
                contours,
                options);
        }
        public static IReadOnlyList<IReadOnlyList<Vector2>> Transform(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            Matrix transform,
            IPolygonTransformer? transformerBackend = null)
        {
            transformerBackend ??= new DefaultPolygonTransformer();

            return transformerBackend.Transform(
                contours,
                transform);
        }

        public static IReadOnlyList<IReadOnlyList<Vector2>> Offset(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonOffsetOptions? options = null,
            IPolygonOffsetter? offsetterBackend = null)
        {
            options ??= new PolygonOffsetOptions();
            offsetterBackend ??= new Clipper2PolygonOffsetter();

            return offsetterBackend.Offset(
                contours,
                options);
        }
    }
}