using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sachssoft.Sasogine.Geometry
{
    public sealed class PathBuilder
    {
        private readonly List<Vector2> _points = new();
        private readonly List<Vector2[]> _polygons = new();
        private Vector2 _currentPoint;
        private Vector2? _previousControlPoint;

        private readonly List<PathCommand> _commands = new();
        private bool _started; // Lebenszyklus-Flag

        private record PathCommand(
            string Command,
            Vector2[] Points,
            Vector2? Control1 = null,
            Vector2? Control2 = null,
            float Rx = 0,
            float Ry = 0,
            float Rotation = 0,
            bool LargeArc = false,
            bool Sweep = false
        );

        public PathBuilder() { }

        #region Lifecycle

        public PathBuilder Start(Vector2 start)
        {
            if (_started)
                throw new InvalidOperationException("PathBuilder.Start() wurde bereits aufgerufen.");

            _started = true;
            _currentPoint = start;
            _previousControlPoint = null;
            _points.Clear();
            _points.Add(start);

            _commands.Add(new PathCommand("M", new[] { start }));
            return this;
        }

        public PathBuilder Close()
        {
            if (!_started)
                throw new InvalidOperationException("PathBuilder.Close() darf nicht vor Start() aufgerufen werden.");

            if (_points.Count > 0)
            {
                if (_currentPoint != _points[0])
                    _points.Add(_points[0]); // Polygon schließen
                _polygons.Add(_points.ToArray());
                _points.Clear();
            }

            _previousControlPoint = null;
            return this;
        }

        #endregion

        #region Line Commands

        public PathBuilder AddLine(float x, float y)
        {
            EnsureStarted();
            var p = new Vector2(x, y);
            _points.Add(p);
            _currentPoint = p;
            _previousControlPoint = null;

            _commands.Add(new PathCommand("L", new[] { p }));
            return this;
        }

        public PathBuilder AddHorizontalLine(float x) => AddLine(x, _currentPoint.Y);

        public PathBuilder AddVerticalLine(float y) => AddLine(_currentPoint.X, y);

        #endregion

        #region Bezier & Arc

        public PathBuilder AddQuadraticBezier(Vector2 c, Vector2 end, int segments = 8)
        {
            EnsureStarted();
            var sampled = GeometrySampler.SampleQuadraticBezier(_currentPoint, c, end, segments);
            _points.AddRange(sampled[1..]);
            _currentPoint = end;
            _previousControlPoint = c;

            _commands.Add(new PathCommand("Q", new[] { end }, c));
            return this;
        }

        public PathBuilder AddCubicBezier(Vector2 c1, Vector2 c2, Vector2 end, int segments = 8)
        {
            EnsureStarted();
            var sampled = GeometrySampler.SampleCubicBezier(_currentPoint, c1, c2, end, segments);
            _points.AddRange(sampled[1..]);
            _currentPoint = end;
            _previousControlPoint = c2;

            _commands.Add(new PathCommand("C", new[] { end }, c1, c2));
            return this;
        }

        public PathBuilder AddArc(Vector2 end, float rx, float ry, float rotation, bool largeArc, bool sweep, int segments = 8)
        {
            EnsureStarted();
            var sampled = GeometrySampler.SampleArc(_currentPoint, end, rx, ry, rotation, largeArc, sweep, segments);
            _points.AddRange(sampled[1..]);
            _currentPoint = end;
            _previousControlPoint = null;

            _commands.Add(new PathCommand("A", new[] { end }, Rx: rx, Ry: ry, Rotation: rotation, LargeArc: largeArc, Sweep: sweep));
            return this;
        }

        #endregion

        #region Build & Output

        public IReadOnlyList<IReadOnlyList<Vector2>> Build()
        {
            if (_points.Count > 0)
                _polygons.Add(_points.ToArray());

            return _polygons
                .Select(p => (IReadOnlyList<Vector2>)p.ToList().AsReadOnly())
                .ToList()
                .AsReadOnly();
        }

        public Path ToPath() => new(_polygons.ToArray());

        public string ToData()
        {
            var sb = new StringBuilder();
            foreach (var cmd in _commands)
            {
                sb.Append(cmd.Command);
                if (cmd.Points.Length > 0)
                    sb.Append(string.Join(" ", Array.ConvertAll(cmd.Points, p => $"{p.X},{p.Y}")));
                sb.Append(" ");
            }
            return sb.ToString().Trim();
        }

        public override string ToString() => ToData();

        #endregion

        #region Helpers

        private void EnsureStarted()
        {
            if (!_started)
                throw new InvalidOperationException("PathBuilder wurde noch nicht gestartet. Rufe zuerst Start() auf.");
        }

        #endregion
    }
}