using Sachssoft.Engine;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Geometry.Internal;

/// <summary>
/// Fits closed polygon contours with line and cubic Bézier segments.
/// </summary>
internal static class CubicBezierContourFitter
{
    private readonly struct DPoint
    {
        public DPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }

        public static DPoint operator +(DPoint a, DPoint b) =>
            new(a.X + b.X, a.Y + b.Y);

        public static DPoint operator -(DPoint a, DPoint b) =>
            new(a.X - b.X, a.Y - b.Y);

        public static DPoint operator -(DPoint value) =>
            new(-value.X, -value.Y);

        public static DPoint operator *(DPoint value, double scale) =>
            new(value.X * scale, value.Y * scale);

        public static DPoint operator /(DPoint value, double divisor) =>
            new(value.X / divisor, value.Y / divisor);
    }

    private readonly struct CubicBezier
    {
        public CubicBezier(
            DPoint start,
            DPoint control1,
            DPoint control2,
            DPoint end)
        {
            Start = start;
            Control1 = control1;
            Control2 = control2;
            End = end;
        }

        public DPoint Start { get; }
        public DPoint Control1 { get; }
        public DPoint Control2 { get; }
        public DPoint End { get; }
    }

    /// <summary>
    /// Fits a closed contour and returns its start point and fitted segments.
    /// </summary>
    public static PolygonTraceSegment[] Fit(
        Point2[] points,
        float curveTolerance,
        float cornerThreshold,
        out Point2 start)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Length < 3)
        {
            throw new ArgumentException(
                "A closed contour must contain at least three points.",
                nameof(points));
        }

        if (!float.IsFinite(curveTolerance) || curveTolerance <= 0f)
            throw new ArgumentOutOfRangeException(nameof(curveTolerance));

        if (!float.IsFinite(cornerThreshold) ||
            cornerThreshold <= 0f ||
            cornerThreshold >= 180f)
        {
            throw new ArgumentOutOfRangeException(nameof(cornerThreshold));
        }

        int[] anchors = FindSplineAnchors(
            points,
            cornerThreshold);

        var segments = new List<PolygonTraceSegment>();
        start = points[anchors[0]];
        double errorSquared =
            (double)curveTolerance *
            curveTolerance;

        for (int i = 0; i < anchors.Length; i++)
        {
            int first = anchors[i];
            int last = anchors[(i + 1) % anchors.Length];
            Point2[] run = BuildCircularRun(points, first, last);

            FitOpenRun(
                run,
                errorSquared,
                segments);
        }

        if (segments.Count != 0)
            return segments.ToArray();

        start = points[0];

        for (int i = 0; i < points.Length - 1; i++)
            segments.Add(PolygonTraceSegment.Line(points[i + 1]));

        segments.Add(PolygonTraceSegment.Line(points[0]));

        return segments.ToArray();
    }

    private static int[] FindSplineAnchors(
        Point2[] points,
        float cornerThreshold)
    {
        var corners = new List<int>();
        double thresholdRadians =
            cornerThreshold * Math.PI / 180d;
        double thresholdCosine = Math.Cos(thresholdRadians);

        for (int i = 0; i < points.Length; i++)
        {
            Point2 previous = points[(i - 1 + points.Length) % points.Length];
            Point2 current = points[i];
            Point2 next = points[(i + 1) % points.Length];

            double ax = previous.X - current.X;
            double ay = previous.Y - current.Y;
            double bx = next.X - current.X;
            double by = next.Y - current.Y;

            double lengthA = Math.Sqrt(ax * ax + ay * ay);
            double lengthB = Math.Sqrt(bx * bx + by * by);

            if (lengthA <= 1e-12d || lengthB <= 1e-12d)
                continue;

            double cosine =
                (ax * bx + ay * by) /
                (lengthA * lengthB);

            cosine = Math.Clamp(cosine, -1d, 1d);

            if (cosine >= thresholdCosine)
                corners.Add(i);
        }

        if (corners.Count >= 2)
            return corners.ToArray();

        int first = corners.Count == 1
            ? corners[0]
            : 0;

        int second = FindFarthestPoint(points, first);

        if (second == first)
            second = (first + points.Length / 2) % points.Length;

        return first < second
            ? new[] { first, second }
            : new[] { second, first };
    }

    private static int FindFarthestPoint(
        IReadOnlyList<Point2> points,
        int originIndex)
    {
        Point2 origin = points[originIndex];
        int bestIndex = originIndex;
        double bestDistanceSquared = -1d;

        for (int i = 0; i < points.Count; i++)
        {
            double x = (double)points[i].X - origin.X;
            double y = (double)points[i].Y - origin.Y;
            double distanceSquared = x * x + y * y;

            if (distanceSquared <= bestDistanceSquared)
                continue;

            bestDistanceSquared = distanceSquared;
            bestIndex = i;
        }

        return bestIndex;
    }

    private static Point2[] BuildCircularRun(
        Point2[] points,
        int first,
        int last)
    {
        var run = new List<Point2>();
        int index = first;
        run.Add(points[index]);

        while (index != last)
        {
            index = (index + 1) % points.Length;
            run.Add(points[index]);

            if (run.Count > points.Length + 1)
            {
                throw new InvalidOperationException(
                    "Failed to construct a closed curve-fitting run.");
            }
        }

        return run.ToArray();
    }

    private static void FitOpenRun(
        Point2[] points,
        double errorSquared,
        List<PolygonTraceSegment> output)
    {
        if (points.Length < 2)
            return;

        if (points.Length == 2)
        {
            output.Add(PolygonTraceSegment.Line(points[1]));
            return;
        }

        DPoint leftTangent = Normalize(
            ToDPoint(points[1]) -
            ToDPoint(points[0]));

        DPoint rightTangent = Normalize(
            ToDPoint(points[^2]) -
            ToDPoint(points[^1]));

        if (LengthSquared(leftTangent) <= 1e-20d ||
            LengthSquared(rightTangent) <= 1e-20d)
        {
            AppendLines(points, output);
            return;
        }

        FitCubic(
            points,
            0,
            points.Length - 1,
            leftTangent,
            rightTangent,
            errorSquared,
            output,
            0);
    }

    private static void FitCubic(
        Point2[] points,
        int first,
        int last,
        DPoint leftTangent,
        DPoint rightTangent,
        double errorSquared,
        List<PolygonTraceSegment> output,
        int depth)
    {
        int pointCount = last - first + 1;

        if (pointCount <= 2)
        {
            output.Add(PolygonTraceSegment.Line(points[last]));
            return;
        }

        if (depth >= 64)
        {
            AppendLines(points, first, last, output);
            return;
        }

        double[] parameters = ChordLengthParameterize(
            points,
            first,
            last);

        CubicBezier curve = GenerateBezier(
            points,
            first,
            last,
            parameters,
            leftTangent,
            rightTangent);

        double maximumError = ComputeMaximumError(
            points,
            first,
            last,
            curve,
            parameters,
            out int splitPoint);

        if (maximumError <= errorSquared)
        {
            AddCurve(curve, points[last], output);
            return;
        }

        if (maximumError <= errorSquared * 4d)
        {
            for (int iteration = 0; iteration < 4; iteration++)
            {
                double[] refined = Reparameterize(
                    points,
                    first,
                    parameters,
                    curve);

                curve = GenerateBezier(
                    points,
                    first,
                    last,
                    refined,
                    leftTangent,
                    rightTangent);

                maximumError = ComputeMaximumError(
                    points,
                    first,
                    last,
                    curve,
                    refined,
                    out splitPoint);

                if (maximumError <= errorSquared)
                {
                    AddCurve(curve, points[last], output);
                    return;
                }

                parameters = refined;
            }
        }

        splitPoint = Math.Clamp(
            splitPoint,
            first + 1,
            last - 1);

        DPoint centerTangent = Normalize(
            ToDPoint(points[splitPoint - 1]) -
            ToDPoint(points[splitPoint + 1]));

        if (LengthSquared(centerTangent) <= 1e-20d)
        {
            centerTangent = Normalize(
                ToDPoint(points[splitPoint - 1]) -
                ToDPoint(points[splitPoint]));
        }

        if (LengthSquared(centerTangent) <= 1e-20d)
        {
            AppendLines(points, first, last, output);
            return;
        }

        FitCubic(
            points,
            first,
            splitPoint,
            leftTangent,
            centerTangent,
            errorSquared,
            output,
            depth + 1);

        FitCubic(
            points,
            splitPoint,
            last,
            -centerTangent,
            rightTangent,
            errorSquared,
            output,
            depth + 1);
    }

    private static void AddCurve(
        CubicBezier curve,
        Point2 end,
        List<PolygonTraceSegment> output)
    {
        output.Add(
            PolygonTraceSegment.CubicBezier(
                ToPoint2(curve.Control1),
                ToPoint2(curve.Control2),
                end));
    }

    private static CubicBezier GenerateBezier(
        Point2[] points,
        int first,
        int last,
        double[] parameters,
        DPoint leftTangent,
        DPoint rightTangent)
    {
        DPoint start = ToDPoint(points[first]);
        DPoint end = ToDPoint(points[last]);

        double c00 = 0d;
        double c01 = 0d;
        double c11 = 0d;
        double x0 = 0d;
        double x1 = 0d;
        int pointCount = last - first + 1;

        for (int i = 0; i < pointCount; i++)
        {
            double u = parameters[i];
            double t = 1d - u;
            double b0 = t * t * t;
            double b1 = 3d * u * t * t;
            double b2 = 3d * u * u * t;
            double b3 = u * u * u;

            DPoint a1 = leftTangent * b1;
            DPoint a2 = rightTangent * b2;
            DPoint point = ToDPoint(points[first + i]);
            DPoint basePoint =
                start * (b0 + b1) +
                end * (b2 + b3);
            DPoint residual = point - basePoint;

            c00 += Dot(a1, a1);
            c01 += Dot(a1, a2);
            c11 += Dot(a2, a2);
            x0 += Dot(a1, residual);
            x1 += Dot(a2, residual);
        }

        double determinant = c00 * c11 - c01 * c01;
        double alphaLeft = 0d;
        double alphaRight = 0d;

        if (Math.Abs(determinant) > 1e-12d)
        {
            alphaLeft =
                (x0 * c11 - x1 * c01) /
                determinant;

            alphaRight =
                (c00 * x1 - c01 * x0) /
                determinant;
        }

        double chordLength = Distance(start, end);
        double epsilon = 1e-6d * chordLength;

        if (!double.IsFinite(alphaLeft) ||
            !double.IsFinite(alphaRight) ||
            alphaLeft < epsilon ||
            alphaRight < epsilon)
        {
            alphaLeft = chordLength / 3d;
            alphaRight = chordLength / 3d;
        }

        return new CubicBezier(
            start,
            start + leftTangent * alphaLeft,
            end + rightTangent * alphaRight,
            end);
    }

    private static double[] ChordLengthParameterize(
        Point2[] points,
        int first,
        int last)
    {
        int count = last - first + 1;
        var parameters = new double[count];

        for (int i = 1; i < count; i++)
        {
            parameters[i] =
                parameters[i - 1] +
                Distance(
                    ToDPoint(points[first + i - 1]),
                    ToDPoint(points[first + i]));
        }

        double total = parameters[^1];

        if (total <= 1e-12d)
        {
            for (int i = 1; i < count; i++)
                parameters[i] = i / (double)(count - 1);

            return parameters;
        }

        for (int i = 1; i < count; i++)
            parameters[i] /= total;

        return parameters;
    }

    private static double ComputeMaximumError(
        Point2[] points,
        int first,
        int last,
        CubicBezier curve,
        double[] parameters,
        out int splitPoint)
    {
        splitPoint = (first + last) / 2;
        double maximumError = 0d;

        for (int i = first + 1; i < last; i++)
        {
            double u = parameters[i - first];
            DPoint point = EvaluateBezier(curve, u);
            DPoint delta = point - ToDPoint(points[i]);
            double error = LengthSquared(delta);

            if (error <= maximumError)
                continue;

            maximumError = error;
            splitPoint = i;
        }

        return maximumError;
    }

    private static double[] Reparameterize(
        Point2[] points,
        int first,
        double[] parameters,
        CubicBezier curve)
    {
        var result = new double[parameters.Length];
        double previous = 0d;

        for (int i = 0; i < parameters.Length; i++)
        {
            double u = NewtonRaphsonRootFind(
                curve,
                ToDPoint(points[first + i]),
                parameters[i]);

            u = Math.Clamp(u, 0d, 1d);

            if (i > 0 && u <= previous)
                u = Math.Min(1d, previous + 1e-6d);

            result[i] = u;
            previous = u;
        }

        result[0] = 0d;
        result[^1] = 1d;

        return result;
    }

    private static double NewtonRaphsonRootFind(
        CubicBezier curve,
        DPoint point,
        double u)
    {
        DPoint q = EvaluateBezier(curve, u);
        DPoint q1 = EvaluateBezierFirstDerivative(curve, u);
        DPoint q2 = EvaluateBezierSecondDerivative(curve, u);
        DPoint difference = q - point;

        double numerator = Dot(difference, q1);
        double denominator =
            Dot(q1, q1) +
            Dot(difference, q2);

        if (Math.Abs(denominator) <= 1e-14d)
            return u;

        return u - numerator / denominator;
    }

    private static DPoint EvaluateBezier(
        CubicBezier curve,
        double t)
    {
        double mt = 1d - t;
        double b0 = mt * mt * mt;
        double b1 = 3d * mt * mt * t;
        double b2 = 3d * mt * t * t;
        double b3 = t * t * t;

        return
            curve.Start * b0 +
            curve.Control1 * b1 +
            curve.Control2 * b2 +
            curve.End * b3;
    }

    private static DPoint EvaluateBezierFirstDerivative(
        CubicBezier curve,
        double t)
    {
        double mt = 1d - t;

        return
            (curve.Control1 - curve.Start) * (3d * mt * mt) +
            (curve.Control2 - curve.Control1) * (6d * mt * t) +
            (curve.End - curve.Control2) * (3d * t * t);
    }

    private static DPoint EvaluateBezierSecondDerivative(
        CubicBezier curve,
        double t)
    {
        return
            (curve.Control2 - curve.Control1 * 2d + curve.Start) *
                (6d * (1d - t)) +
            (curve.End - curve.Control2 * 2d + curve.Control1) *
                (6d * t);
    }

    private static void AppendLines(
        Point2[] points,
        List<PolygonTraceSegment> output)
    {
        AppendLines(
            points,
            0,
            points.Length - 1,
            output);
    }

    private static void AppendLines(
        Point2[] points,
        int first,
        int last,
        List<PolygonTraceSegment> output)
    {
        for (int i = first + 1; i <= last; i++)
            output.Add(PolygonTraceSegment.Line(points[i]));
    }

    private static DPoint ToDPoint(Point2 point)
    {
        return new DPoint(point.X, point.Y);
    }

    private static Point2 ToPoint2(DPoint point)
    {
        if (!double.IsFinite(point.X) ||
            !double.IsFinite(point.Y) ||
            point.X < -float.MaxValue ||
            point.X > float.MaxValue ||
            point.Y < -float.MaxValue ||
            point.Y > float.MaxValue)
        {
            throw new OverflowException(
                "Curve fitting produced coordinates outside the supported range.");
        }

        return new Point2(
            (float)point.X,
            (float)point.Y);
    }

    private static DPoint Normalize(DPoint value)
    {
        double lengthSquared = LengthSquared(value);

        if (lengthSquared <= 1e-20d)
            return new DPoint(0d, 0d);

        return value / Math.Sqrt(lengthSquared);
    }

    private static double Dot(DPoint a, DPoint b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    private static double LengthSquared(DPoint value)
    {
        return value.X * value.X + value.Y * value.Y;
    }

    private static double Distance(DPoint a, DPoint b)
    {
        double x = b.X - a.X;
        double y = b.Y - a.Y;
        return Math.Sqrt(x * x + y * y);
    }
}
