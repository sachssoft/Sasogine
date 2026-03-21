using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using Sachssoft.Sasogine.Surface.Styles;

namespace Sachssoft.Sasogine.Surface.Visuals
{
    partial class RenderContext
    {
        #region Filled Rectangles

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle to draw.</param>
        /// <param name="color">The fill color.</param>
        public void FillRectangle(Rectangle rectangle, Color color) =>
            FillRectangle(new Vector2(rectangle.X, rectangle.Y), new Vector2(rectangle.Width, rectangle.Height), color);

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="location">The top-left corner.</param>
        /// <param name="size">The width and height.</param>
        /// <param name="color">The fill color.</param>
        public void FillRectangle(Vector2 location, Vector2 size, Color color) =>
            Stylesheet.WhiteRegion.Draw(this,
                new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y),
                color);

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        public void FillRectangle(float x, float y, float width, float height, Color color) =>
            FillRectangle(new Vector2(x, y), new Vector2(width, height), color);

        #endregion

        #region Rectangle Outlines

        /// <summary>
        /// Draws a rectangle outline with specified thickness.
        /// </summary>
        public void DrawRectangle(Rectangle rectangle, Color color, float thickness = 1f)
        {
            var region = Stylesheet.WhiteRegion;
            var t = (int)thickness;
            var c = CrossEngineStuff.MultiplyColor(color, Opacity);

            // Top
            region.Draw(this, new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, t), c);
            // Bottom
            region.Draw(this, new Rectangle(rectangle.X, rectangle.Bottom - t, rectangle.Width, t), c);
            // Left
            region.Draw(this, new Rectangle(rectangle.X, rectangle.Y, t, rectangle.Height), c);
            // Right
            region.Draw(this, new Rectangle(rectangle.Right - t, rectangle.Y, t, rectangle.Height), c);
        }

        /// <summary>
        /// Draws a rectangle outline with specified thickness.
        /// </summary>
        public void DrawRectangle(Vector2 location, Vector2 size, Color color, float thickness = 1f) =>
            DrawRectangle(new Rectangle((int)location.X, (int)location.Y, (int)size.X, (int)size.Y), color, thickness);

        #endregion

        #region Lines & Points

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        public void DrawLine(Vector2 point1, Vector2 point2, Color color, float thickness = 1f)
        {
            var distance = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            DrawLine(point1, distance, angle, color, thickness);
        }

        /// <summary>
        /// Draws a line from a start point, given length and angle.
        /// </summary>
        public void DrawLine(Vector2 point, float length, float angle, Color color, float thickness = 1f)
        {
            var scale = new Vector2(length, thickness);
            point.Y -= (int)(thickness * Math.Cos(angle) / 2.0f);
            Draw(Stylesheet.WhiteRegion.Texture, point, null, color, angle, scale, 0);
        }

        /// <summary>
        /// Draws a line using float coordinates.
        /// </summary>
        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float thickness = 1f) =>
            DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);

        /// <summary>
        /// Draws a single point at a given position.
        /// </summary>
        public void DrawPoint(Vector2 position, Color color, float size = 1f)
        {
            var scale = Vector2.One * size;
            var offset = new Vector2(0.5f) - new Vector2(size * 0.5f);
            Draw(Stylesheet.WhiteRegion.Texture, position + offset, null, color, 0.0f, scale);
        }

        /// <summary>
        /// Draws a single point using float coordinates.
        /// </summary>
        public void DrawPoint(float x, float y, Color color, float size = 1f) =>
            DrawPoint(new Vector2(x, y), color, size);

        #endregion

        #region Polygons

        /// <summary>
        /// Draws a closed polygon from an array of points.
        /// </summary>
        public void DrawPolygon(Vector2 offset, Vector2[] points, Color color, float thickness = 1f)
        {
            if (points.Length == 0) return;
            if (points.Length == 1) { DrawPoint(points[0], color, thickness); return; }

            for (var i = 0; i < points.Length - 1; i++)
                DrawPolygonEdge(points[i] + offset, points[i + 1] + offset, color, thickness);
            DrawPolygonEdge(points[points.Length - 1] + offset, points[0] + offset, color, thickness);
        }

        private void DrawPolygonEdge(Vector2 point1, Vector2 point2, Color color, float thickness)
        {
            var length = Vector2.Distance(point1, point2);
            var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            var scale = new Vector2(length, thickness);
            Draw(Stylesheet.WhiteRegion.Texture, point1, null, color, angle, scale);
        }

        #endregion

        #region Circles & Arcs

        /// <summary>
        /// Draws a circle with given center, radius, sides, color, and thickness.
        /// </summary>
        public void DrawCircle(Vector2 center, float radius, int sides, Color color, float thickness = 1f) =>
            DrawPolygon(center, CreateCircle(radius, sides), color, thickness);

        public void DrawCircle(float x, float y, float radius, int sides, Color color, float thickness = 1f) =>
            DrawPolygon(new Vector2(x, y), CreateCircle(radius, sides), color, thickness);

        /// <summary>
        /// Draws an arc with given center, radius, sides, color, start and end angles, and thickness.
        /// </summary>
        public void DrawArc(Vector2 center, float radius, int sides, Color color, float startAngle, float endAngle, float thickness = 1f) =>
            DrawPolygon(center, CreateArc(radius, sides, startAngle, endAngle), color, thickness);

        public void DrawArc(float x, float y, float radius, int sides, Color color, float startAngle, float endAngle, float thickness = 1f) =>
            DrawPolygon(new Vector2(x, y), CreateArc(radius, sides, startAngle, endAngle), color, thickness);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2[] CreateArcHelper(double radius, int sides, double step, double theta = 0.0)
        {
            var points = new Vector2[sides];
            for (var i = 0; i < sides; i++)
            {
                points[i] = new Vector2((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta)));
                theta += step;
            }
            return points;
        }

        private Vector2[] CreateCircle(double radius, int sides)
        {
            var step = 2.0 * Math.PI / sides;
            return CreateArcHelper(radius, sides, step);
        }

        private Vector2[] CreateArc(double radius, int sides, double startAngle, double endAngle)
        {
            var max = Math.Max(endAngle - startAngle, 0);
            var step = max / sides;
            return CreateArcHelper(radius, sides, step, startAngle);
        }

        #endregion
    }
}
