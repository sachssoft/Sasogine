using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Enums;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Geometry.Shapes
{
    public class StarPath : ShapePathBase
    {
        public int Spikes { get; init; } = 5;
        public float OuterRadius { get; init; } = 1f;
        public float InnerRadius { get; init; } = 0.5f;
        public float Angle { get; init; } = 0f;

        public float OuterRounding { get; init; } = 0f; // 0..1
        public float InnerRounding { get; init; } = 0f; // 0..1

        public RoundingType OuterRoundingType { get; init; } = RoundingType.Quadratic;
        public RoundingType InnerRoundingType { get; init; } = RoundingType.Quadratic;

        public int Segments { get; init; } = 8;

        protected override Path BuildDefinedPath()
        {
            if (Spikes < 2) throw new InvalidOperationException("Star must have at least 2 spikes.");

            float rotationRad = MathHelper.ToRadians(Angle);
            float step = MathHelper.TwoPi / Spikes;

            var polygon = new List<Vector2>();
            Vector2 center = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < Spikes; i++)
            {
                // Berechne Außen- und Innenpunkt
                float outerAngle = rotationRad + i * step;
                float innerAngle = outerAngle + step / 2f;

                Vector2 outerPoint = center + new Vector2(MathF.Cos(outerAngle), MathF.Sin(outerAngle)) * OuterRadius;
                Vector2 innerPoint = center + new Vector2(MathF.Cos(innerAngle), MathF.Sin(innerAngle)) * InnerRadius;

                // Vorheriger Punkt für Rounding
                Vector2 prev = polygon.Count > 0 ? polygon[^1] : outerPoint;

                // Außenpunkt mit optionalem Rounding
                if (OuterRounding > 0f && polygon.Count > 0)
                {
                    float dist = OuterRounding * Vector2.Distance(prev, outerPoint);
                    Vector2 start = outerPoint - Vector2.Normalize(outerPoint - prev) * dist;
                    Vector2 end = outerPoint + Vector2.Normalize(innerPoint - outerPoint) * dist;

                    var sampled = OuterRoundingType switch
                    {
                        RoundingType.Linear => GeometrySampler.SampleLinear(start, end, Segments),
                        RoundingType.Quadratic => GeometrySampler.SampleQuadraticBezier(start, outerPoint, end, Segments),
                        RoundingType.Cubic => GeometrySampler.SampleCubicBezier(start, start + Vector2.Normalize(outerPoint - prev) * dist * 0.5f,
                                                                               end - Vector2.Normalize(innerPoint - outerPoint) * dist * 0.5f,
                                                                               end, Segments),
                        _ => throw new NotImplementedException()
                    };
                    polygon.AddRange(sampled);
                }
                else
                {
                    polygon.Add(outerPoint);
                }

                // Innenpunkt mit optionalem Rounding
                if (InnerRounding > 0f)
                {
                    float dist = InnerRounding * Vector2.Distance(outerPoint, innerPoint);
                    Vector2 start = innerPoint - Vector2.Normalize(innerPoint - outerPoint) * dist;
                    Vector2 end = innerPoint + Vector2.Normalize(outerPoint - innerPoint) * dist;

                    var sampled = InnerRoundingType switch
                    {
                        RoundingType.Linear => GeometrySampler.SampleLinear(start, end, Segments),
                        RoundingType.Quadratic => GeometrySampler.SampleQuadraticBezier(start, innerPoint, end, Segments),
                        RoundingType.Cubic => GeometrySampler.SampleCubicBezier(start, start + Vector2.Normalize(innerPoint - outerPoint) * dist * 0.5f,
                                                                               end - Vector2.Normalize(outerPoint - innerPoint) * dist * 0.5f,
                                                                               end, Segments),
                        _ => throw new NotImplementedException()
                    };
                    polygon.AddRange(sampled);
                }
                else
                {
                    polygon.Add(innerPoint);
                }
            }

            // Polygon schließen
            if (polygon.Count > 0 && polygon[0] != polygon[^1])
                polygon.Add(polygon[0]);

            return new Path(new[] { polygon.ToArray() });
        }
    }
}
