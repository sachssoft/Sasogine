using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Provides a mutable fluent builder for constructing <see cref="Path"/> instances
    /// from lines, Bézier curves and elliptical arcs.
    /// </summary>
    public sealed class PathBuilder
    {
        private readonly List<Vector2> _points = new();
        private readonly List<Vector2[]> _polygons = new();
        private readonly List<PathCommand> _commands = new();

        private Vector2 _currentPoint;
        private Vector2? _previousControlPoint;
        private bool _started;
        private bool _closed;

        private sealed record PathCommand(
            string Command,
            Vector2[] Points,
            Vector2? Control1 = null,
            Vector2? Control2 = null,
            float Rx = 0f,
            float Ry = 0f,
            float Rotation = 0f,
            bool LargeArc = false,
            bool Sweep = false);

        /// <summary>
        /// Initializes a new empty <see cref="PathBuilder"/>.
        /// </summary>
        public PathBuilder()
        {
        }

        /// <summary>
        /// Starts a new subpath at the specified position.
        /// </summary>
        /// <param name="start">The starting position.</param>
        /// <returns>This builder.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the builder has already been started.
        /// </exception>
        public PathBuilder Start(Vector2 start)
        {
            if (_started)
                throw new InvalidOperationException(
                    "PathBuilder.Start() wurde bereits aufgerufen.");

            _started = true;
            _closed = false;
            _currentPoint = start;
            _previousControlPoint = null;

            _points.Clear();
            _polygons.Clear();
            _commands.Clear();

            _points.Add(start);
            _commands.Add(new PathCommand("M", new[] { start }));

            return this;
        }

        /// <summary>
        /// Closes the current subpath and adds it to the resulting path.
        /// </summary>
        /// <returns>This builder.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the builder has not been started.
        /// </exception>
        public PathBuilder Close()
        {
            EnsureStarted();

            if (_closed)
                return this;

            if (_points.Count >= 3)
            {
                if (_currentPoint != _points[0])
                    _points.Add(_points[0]);

                _polygons.Add(_points.ToArray());
            }

            _points.Clear();
            _previousControlPoint = null;
            _closed = true;

            _commands.Add(new PathCommand("Z", Array.Empty<Vector2>()));

            return this;
        }

        /// <summary>
        /// Adds a line segment to the specified position.
        /// </summary>
        /// <param name="x">The X coordinate of the end position.</param>
        /// <param name="y">The Y coordinate of the end position.</param>
        /// <returns>This builder.</returns>
        public PathBuilder AddLine(float x, float y)
        {
            EnsureStarted();
            EnsureOpen();

            var point = new Vector2(x, y);

            _points.Add(point);
            _currentPoint = point;
            _previousControlPoint = null;

            _commands.Add(new PathCommand("L", new[] { point }));

            return this;
        }

        /// <summary>
        /// Adds a horizontal line segment to the specified X coordinate.
        /// </summary>
        /// <param name="x">The X coordinate of the end position.</param>
        /// <returns>This builder.</returns>
        public PathBuilder AddHorizontalLine(float x)
        {
            return AddLine(x, _currentPoint.Y);
        }

        /// <summary>
        /// Adds a vertical line segment to the specified Y coordinate.
        /// </summary>
        /// <param name="y">The Y coordinate of the end position.</param>
        /// <returns>This builder.</returns>
        public PathBuilder AddVerticalLine(float y)
        {
            return AddLine(_currentPoint.X, y);
        }

        /// <summary>
        /// Adds a quadratic Bézier curve.
        /// </summary>
        /// <param name="control">The control point.</param>
        /// <param name="end">The end position.</param>
        /// <param name="segments">The number of line segments used for sampling.</param>
        /// <returns>This builder.</returns>
        public PathBuilder AddQuadraticBezier(
            Vector2 control,
            Vector2 end,
            int segments = 8)
        {
            EnsureStarted();
            EnsureOpen();

            var sampled = GeometrySampler.SampleQuadraticBezier(
                _currentPoint,
                control,
                end,
                segments);

            AddSampledPoints(sampled);

            _currentPoint = end;
            _previousControlPoint = control;

            _commands.Add(new PathCommand(
                "Q",
                new[] { end },
                Control1: control));

            return this;
        }

        /// <summary>
        /// Adds a cubic Bézier curve.
        /// </summary>
        /// <param name="control1">The first control point.</param>
        /// <param name="control2">The second control point.</param>
        /// <param name="end">The end position.</param>
        /// <param name="segments">The number of line segments used for sampling.</param>
        /// <returns>This builder.</returns>
        public PathBuilder AddCubicBezier(
            Vector2 control1,
            Vector2 control2,
            Vector2 end,
            int segments = 8)
        {
            EnsureStarted();
            EnsureOpen();

            var sampled = GeometrySampler.SampleCubicBezier(
                _currentPoint,
                control1,
                control2,
                end,
                segments);

            AddSampledPoints(sampled);

            _currentPoint = end;
            _previousControlPoint = control2;

            _commands.Add(new PathCommand(
                "C",
                new[] { end },
                Control1: control1,
                Control2: control2));

            return this;
        }

        /// <summary>
        /// Adds an elliptical arc to the specified end position.
        /// </summary>
        /// <param name="end">The end position.</param>
        /// <param name="rx">The horizontal radius.</param>
        /// <param name="ry">The vertical radius.</param>
        /// <param name="rotation">The ellipse rotation in radians.</param>
        /// <param name="largeArc">Indicates whether the larger arc should be used.</param>
        /// <param name="sweep">Indicates the sweep direction.</param>
        /// <param name="segments">The number of line segments used for sampling.</param>
        /// <returns>This builder.</returns>
        public PathBuilder AddArc(
            Vector2 end,
            float rx,
            float ry,
            float rotation,
            bool largeArc,
            bool sweep,
            int segments = 8)
        {
            EnsureStarted();
            EnsureOpen();

            var sampled = GeometrySampler.SampleArc(
                _currentPoint,
                end,
                rx,
                ry,
                rotation,
                largeArc,
                sweep,
                segments);

            AddSampledPoints(sampled);

            _currentPoint = end;
            _previousControlPoint = null;

            _commands.Add(new PathCommand(
                "A",
                new[] { end },
                Rx: rx,
                Ry: ry,
                Rotation: rotation,
                LargeArc: largeArc,
                Sweep: sweep));

            return this;
        }

        /// <summary>
        /// Builds the current geometry as an immutable collection of polygon contours.
        /// </summary>
        /// <returns>A snapshot of the currently constructed polygon contours.</returns>
        public IReadOnlyList<IReadOnlyList<Vector2>> Build()
        {
            var polygons = new List<IReadOnlyList<Vector2>>(
                _polygons.Count + (_points.Count >= 3 ? 1 : 0));

            foreach (var polygon in _polygons)
                polygons.Add(Array.AsReadOnly((Vector2[])polygon.Clone()));

            if (_points.Count >= 3)
                polygons.Add(Array.AsReadOnly(_points.ToArray()));

            return polygons.AsReadOnly();
        }

        /// <summary>
        /// Creates an immutable <see cref="Path"/> from the current builder state.
        /// </summary>
        /// <returns>A new immutable path containing the current geometry.</returns>
        public Path ToPath()
        {
            var polygons = new List<Vector2[]>(
                _polygons.Count + (_points.Count >= 3 ? 1 : 0));

            foreach (var polygon in _polygons)
                polygons.Add((Vector2[])polygon.Clone());

            if (_points.Count >= 3)
                polygons.Add(_points.ToArray());

            return new Path(polygons);
        }

        /// <summary>
        /// Converts the builder commands into path data.
        /// </summary>
        /// <returns>A serialized representation of the path commands.</returns>
        public string ToData()
        {
            var builder = new StringBuilder();

            foreach (var command in _commands)
            {
                builder.Append(command.Command);

                if (command.Points.Length > 0)
                {
                    foreach (var point in command.Points)
                    {
                        builder.Append(' ');
                        builder.Append(point.X);
                        builder.Append(',');
                        builder.Append(point.Y);
                    }
                }

                builder.Append(' ');
            }

            return builder.ToString().Trim();
        }

        /// <summary>
        /// Returns the serialized path data.
        /// </summary>
        /// <returns>The path data.</returns>
        public override string ToString()
        {
            return ToData();
        }

        private void AddSampledPoints(IReadOnlyList<Vector2> sampled)
        {
            for (int i = 1; i < sampled.Count; i++)
                _points.Add(sampled[i]);
        }

        private void EnsureStarted()
        {
            if (!_started)
                throw new InvalidOperationException(
                    "PathBuilder wurde noch nicht gestartet. Rufe zuerst Start() auf.");
        }

        private void EnsureOpen()
        {
            if (_closed)
                throw new InvalidOperationException(
                    "Der aktuelle Pfad wurde bereits mit Close() geschlossen.");
        }
    }
}