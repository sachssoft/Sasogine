using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Internal
{
    internal sealed class Clipper2PolygonStroker : IPolygonStroker
    {
        private readonly Clipper2PolygonOffsetter _offsetter;
        private readonly Clipper2PolygonClipper _clipper;

        public Clipper2PolygonStroker()
        {
            _offsetter =
                new Clipper2PolygonOffsetter();

            _clipper =
                new Clipper2PolygonClipper();
        }

        public IReadOnlyList<IReadOnlyList<Vector2>> Stroke(
            IReadOnlyList<IReadOnlyList<Vector2>> contours,
            PolygonStrokeOptions options)
        {
            if (contours is null)
                throw new ArgumentNullException(
                    nameof(contours));

            if (options is null)
                throw new ArgumentNullException(
                    nameof(options));

            if (options.Thickness <= 0f)
                return Array.Empty<IReadOnlyList<Vector2>>();

            float halfThickness =
                options.Thickness * 0.5f;

            PolygonOffsetEndType endType =
                options.EndType ==
                PolygonStrokeEndType.Closed
                    ? PolygonOffsetEndType.Polygon
                    : ConvertCapType(
                        options.CapType);

            var outer =
                _offsetter.Offset(
                    contours,
                    new PolygonOffsetOptions(
                        delta: halfThickness,
                        miterLimit: options.MiterLimit,
                        precision: options.Precision,
                        arcTolerance: options.ArcTolerance,
                        joinType: options.JoinType,
                        endType: endType));

            if (outer.Count == 0)
                return Array.Empty<IReadOnlyList<Vector2>>();

            if (options.EndType ==
                PolygonStrokeEndType.Open)
            {
                return outer;
            }

            var inner =
                _offsetter.Offset(
                    contours,
                    new PolygonOffsetOptions(
                        delta: -halfThickness,
                        miterLimit: options.MiterLimit,
                        precision: options.Precision,
                        arcTolerance: options.ArcTolerance,
                        joinType: options.JoinType,
                        endType:
                            PolygonOffsetEndType.Polygon));

            if (inner.Count == 0)
                return outer;

            return _clipper.Clip(
                outer,
                inner,
                PolygonClipOperation.Difference);
        }

        private static PolygonOffsetEndType ConvertCapType(
            PolygonStrokeCapType capType)
        {
            return capType switch
            {
                PolygonStrokeCapType.Butt =>
                    PolygonOffsetEndType.Butt,

                PolygonStrokeCapType.Square =>
                    PolygonOffsetEndType.Square,

                PolygonStrokeCapType.Round =>
                    PolygonOffsetEndType.Round,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(capType))
            };
        }
    }
}