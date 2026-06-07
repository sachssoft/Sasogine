// -----------------------------------------------------------------------------------
// Portions of this SVG Path parser are derived from Avalonia (https://github.com/AvaloniaUI/Avalonia)
// License: MIT License (MIT)
//
// MIT License
//
// Copyright (c) The Avalonia Project (https://github.com/AvaloniaUI/Avalonia)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// -----------------------------------------------------------------------------------
//
// Custom modifications for MonoGame by Tobias Sachs (Sachssoft), 2025
// -----------------------------------------------------------------------------------

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Sasogine.Geometry
{
    public static class PathParser
    {
        /// <summary>
        /// Parses an SVG-style path string into an array of Vector2 arrays (polygons).
        /// Each 'M' command starts a new polygon.
        /// </summary>
        public static Path Parse(string pathData, int curveSegments = 8)
        {
            if (string.IsNullOrWhiteSpace(pathData))
                return new Path();

            var parser = new Parser(pathData.AsSpan(), curveSegments);
            return new Path(parser.Parse());
        }

        private ref struct Parser
        {
            private ReadOnlySpan<char> _span;
            private readonly int _curveSegments;

            private Vector2 _currentPoint;
            private Vector2? _previousControlPoint;
            private List<Vector2> _currentPolygon;
            private readonly List<Vector2[]> _polygons;

            public Parser(ReadOnlySpan<char> span, int curveSegments)
            {
                _span = span;
                _curveSegments = curveSegments;
                _currentPoint = Vector2.Zero;
                _previousControlPoint = null;
                _currentPolygon = new List<Vector2>();
                _polygons = new List<Vector2[]>();
            }

            public Vector2[][] Parse()
            {
                while (!IsEmpty(_span))
                {
                    if (!ReadCommand(out char command, out bool relative))
                        break;

                    bool first = true;
                    do
                    {
                        if (!first)
                            _span = ReadSeparator(_span);

                        switch (command)
                        {
                            case 'M':
                            case 'm':
                                StartMove(relative);
                                command = relative ? 'l' : 'L';
                                break;
                            case 'L':
                            case 'l':
                                AddLine(relative);
                                break;
                            case 'H':
                            case 'h':
                                AddHorizontalLine(relative);
                                break;
                            case 'V':
                            case 'v':
                                AddVerticalLine(relative);
                                break;
                            case 'C':
                            case 'c':
                                AddCubicBezier(relative);
                                break;
                            case 'S':
                            case 's':
                                AddSmoothCubicBezier(relative);
                                break;
                            case 'Q':
                            case 'q':
                                AddQuadraticBezier(relative);
                                break;
                            case 'T':
                            case 't':
                                AddSmoothQuadraticBezier(relative);
                                break;
                            case 'Z':
                            case 'z':
                                ClosePolygon();
                                break;
                            case 'A':
                            case 'a':
                                AddArc(relative);
                                break;
                            default:
                                throw new InvalidOperationException($"Unsupported command: {command}");
                        }

                        first = false;
                    } while (PeekArgument(_span));
                }

                if (_currentPolygon.Count > 0)
                    _polygons.Add(_currentPolygon.ToArray());

                return _polygons.ToArray();
            }

            #region Commands
            private void StartMove(bool relative)
            {
                _currentPoint = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();

                // neues Polygon beginnen
                if (_currentPolygon.Count > 0)
                    _polygons.Add(_currentPolygon.ToArray());
                _currentPolygon = new List<Vector2> { _currentPoint };

                // nachfolgende Koordinaten werden zu Linien
                while (PeekArgument(_span))
                {
                    _span = ReadSeparator(_span);
                    AddLine(relative);
                }

                _previousControlPoint = null;
            }

            private void AddLine(bool relative)
            {
                var next = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();
                _currentPolygon.Add(next);
                _currentPoint = next;
                _previousControlPoint = null;
            }

            private void AddHorizontalLine(bool relative)
            {
                float x = (float)ReadDouble();
                var next = relative ? new Vector2(_currentPoint.X + x, _currentPoint.Y) : new Vector2(x, _currentPoint.Y);
                _currentPolygon.Add(next);
                _currentPoint = next;
                _previousControlPoint = null;
            }

            private void AddVerticalLine(bool relative)
            {
                float y = (float)ReadDouble();
                var next = relative ? new Vector2(_currentPoint.X, _currentPoint.Y + y) : new Vector2(_currentPoint.X, y);
                _currentPolygon.Add(next);
                _currentPoint = next;
                _previousControlPoint = null;
            }

            private void AddCubicBezier(bool relative)
            {
                var c1 = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();
                _span = ReadSeparator(_span);
                var c2 = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();
                _span = ReadSeparator(_span);
                var p = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();

                var points = GeometrySampler.SampleCubicBezier(_currentPoint, c1, c2, p, _curveSegments);
                _currentPolygon.AddRange(points[1..]);

                _previousControlPoint = c2;
                _currentPoint = p;
            }

            private void AddSmoothCubicBezier(bool relative)
            {
                var c1 = _previousControlPoint.HasValue ? Reflect(_previousControlPoint.Value, _currentPoint) : _currentPoint;
                var c2 = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();
                _span = ReadSeparator(_span);
                var p = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();

                var points = GeometrySampler.SampleCubicBezier(_currentPoint, c1, c2, p, _curveSegments);
                _currentPolygon.AddRange(points[1..]);

                _previousControlPoint = c2;
                _currentPoint = p;
            }

            private void AddQuadraticBezier(bool relative)
            {
                var c = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();
                _span = ReadSeparator(_span);
                var p = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();

                var points = GeometrySampler.SampleQuadraticBezier(_currentPoint, c, p, _curveSegments);
                _currentPolygon.AddRange(points[1..]);

                _previousControlPoint = c;
                _currentPoint = p;
            }

            private void AddSmoothQuadraticBezier(bool relative)
            {
                var c = _previousControlPoint.HasValue ? Reflect(_previousControlPoint.Value, _currentPoint) : _currentPoint;
                var p = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();

                var points = GeometrySampler.SampleQuadraticBezier(_currentPoint, c, p, _curveSegments);
                _currentPolygon.AddRange(points[1..]);

                _previousControlPoint = c;
                _currentPoint = p;
            }

            private void AddArc(bool relative)
            {
                float rx = (float)ReadDouble();
                _span = ReadSeparator(_span);
                float ry = (float)ReadDouble();
                _span = ReadSeparator(_span);
                float rotation = (float)ReadDouble();
                _span = ReadSeparator(_span);
                bool largeArc = ReadBool();
                _span = ReadSeparator(_span);
                bool sweep = ReadBool();
                _span = ReadSeparator(_span);
                var p = relative ? ReadRelativePoint(_currentPoint) : ReadPoint();

                var points = GeometrySampler.SampleArc(_currentPoint, p, rx, ry, rotation, largeArc, sweep, _curveSegments);
                _currentPolygon.AddRange(points[1..]);

                _currentPoint = p;
                _previousControlPoint = null;
            }

            private void ClosePolygon()
            {
                if (_currentPolygon.Count > 0 && _currentPoint != _currentPolygon[0])
                    _currentPolygon.Add(_currentPolygon[0]);
                if (_currentPolygon.Count > 0)
                    _polygons.Add(_currentPolygon.ToArray());

                _currentPolygon = new List<Vector2>();
                _previousControlPoint = null;
            }
            #endregion

            #region Helpers
            private static Vector2 Reflect(Vector2 control, Vector2 center) => center * 2 - control;

            private static bool IsEmpty(ReadOnlySpan<char> span) => span.IsEmpty;

            private static ReadOnlySpan<char> ReadSeparator(ReadOnlySpan<char> span)
            {
                span = SkipWhitespace(span);
                if (!span.IsEmpty && span[0] == ',') span = span.Slice(1);
                return span;
            }

            private static ReadOnlySpan<char> SkipWhitespace(ReadOnlySpan<char> span)
            {
                int i = 0;
                while (i < span.Length && char.IsWhiteSpace(span[i])) i++;
                return span.Slice(i);
            }

            private bool PeekArgument(ReadOnlySpan<char> span)
            {
                span = SkipWhitespace(span);
                return !span.IsEmpty && (char.IsDigit(span[0]) || span[0] == '-' || span[0] == '.');
            }

            private Vector2 ReadPoint()
            {
                float x = (float)ReadDouble();
                _span = ReadSeparator(_span);
                float y = (float)ReadDouble();
                return new Vector2(x, y);
            }

            private Vector2 ReadRelativePoint(Vector2 origin)
            {
                float x = (float)ReadDouble();
                _span = ReadSeparator(_span);
                float y = (float)ReadDouble();
                return new Vector2(origin.X + x, origin.Y + y);
            }

            private bool ReadBool()
            {
                _span = SkipWhitespace(_span);
                if (_span.IsEmpty) throw new FormatException("Invalid boolean in path.");
                var result = _span[0] == '1';
                _span = _span.Slice(1);
                return result;
            }

            private double ReadDouble()
            {
                _span = SkipWhitespace(_span);
                if (_span.IsEmpty) throw new FormatException("Unexpected end of path data.");

                int i = 0;

                if (_span[i] == '-' || _span[i] == '+') i++;

                bool hasDigits = false;
                while (i < _span.Length && char.IsDigit(_span[i]))
                {
                    i++;
                    hasDigits = true;
                }

                if (i < _span.Length && _span[i] == '.')
                {
                    i++;
                    while (i < _span.Length && char.IsDigit(_span[i]))
                    {
                        i++;
                        hasDigits = true;
                    }
                }

                if (i < _span.Length && (_span[i] == 'e' || _span[i] == 'E'))
                {
                    i++;
                    if (i < _span.Length && (_span[i] == '-' || _span[i] == '+')) i++;
                    bool hasExpDigits = false;
                    while (i < _span.Length && char.IsDigit(_span[i]))
                    {
                        i++;
                        hasExpDigits = true;
                    }
                    if (!hasExpDigits)
                        throw new FormatException("Invalid exponent in path number.");
                }

                if (!hasDigits)
                    throw new FormatException("Expected number in path data.");

                var s = _span.Slice(0, i).ToString();
                _span = _span.Slice(i);

                return double.Parse(s, CultureInfo.InvariantCulture);
            }

            private bool ReadCommand(out char command, out bool relative)
            {
                _span = SkipWhitespace(_span);
                if (_span.IsEmpty)
                {
                    command = default;
                    relative = false;
                    return false;
                }

                command = _span[0];
                relative = char.IsLower(command);
                _span = _span.Slice(1);
                return true;
            }
            #endregion
        }
    }
}
