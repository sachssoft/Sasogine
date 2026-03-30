using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Visuals;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Layouts
{
    public class WrapPanelLayout : ILayout
    {
        private int _spacing;

        public Orientation Orientation { get; private set; }

        /// <summary>
        /// Abstand zwischen Widgets (horizontal oder vertikal)
        /// </summary>
        public int Spacing
        {
            get => _spacing;
            set => _spacing = value < 0 ? 0 : value;
        }

        public WrapPanelLayout(Orientation orientation)
        {
            Orientation = orientation;
            Spacing = 0;
        }

        public Point Measure(IEnumerable<Widget> widgets, Point availableSize)
        {
            if (Orientation == Orientation.Horizontal)
                return MeasureHorizontal(widgets, availableSize);
            else
                return MeasureVertical(widgets, availableSize);
        }

        public void Arrange(IEnumerable<Widget> widgets, Rectangle bounds)
        {
            if (Orientation == Orientation.Horizontal)
                ArrangeHorizontal(widgets, bounds);
            else
                ArrangeVertical(widgets, bounds);
        }

        private Point MeasureHorizontal(IEnumerable<Widget> widgets, Point availableSize)
        {
            int lineWidth = 0;
            int lineHeight = 0;
            int totalHeight = 0;
            int maxWidth = 0;

            foreach (var widget in widgets)
            {
                var size = widget.Measure(availableSize); // Widget gibt gewünschte Größe zurück

                if (lineWidth + size.X > availableSize.X && lineWidth > 0)
                {
                    // Umbruch
                    totalHeight += lineHeight + Spacing;
                    maxWidth = System.Math.Max(maxWidth, lineWidth);

                    lineWidth = 0;
                    lineHeight = 0;
                }

                lineWidth += size.X + Spacing;
                lineHeight = System.Math.Max(lineHeight, size.Y);
            }

            // letzte Zeile hinzufügen
            if (lineWidth > 0)
            {
                totalHeight += lineHeight;
                maxWidth = System.Math.Max(maxWidth, lineWidth);
            }

            return new Point(maxWidth, totalHeight);
        }

        private Point MeasureVertical(IEnumerable<Widget> widgets, Point availableSize)
        {
            int lineHeight = 0;
            int lineWidth = 0;
            int totalWidth = 0;
            int maxHeight = 0;

            foreach (var widget in widgets)
            {
                var size = widget.Measure(availableSize);

                if (lineHeight + size.Y > availableSize.Y && lineHeight > 0)
                {
                    // Umbruch
                    totalWidth += lineWidth + Spacing;
                    maxHeight = System.Math.Max(maxHeight, lineHeight);

                    lineHeight = 0;
                    lineWidth = 0;
                }

                lineHeight += size.Y + Spacing;
                lineWidth = System.Math.Max(lineWidth, size.X);
            }

            if (lineHeight > 0)
            {
                totalWidth += lineWidth;
                maxHeight = System.Math.Max(maxHeight, lineHeight);
            }

            return new Point(totalWidth, maxHeight);
        }

        private void ArrangeHorizontal(IEnumerable<Widget> widgets, Rectangle bounds)
        {
            int x = bounds.X;
            int y = bounds.Y;
            int lineHeight = 0;

            foreach (var widget in widgets)
            {
                var desired = widget.Measure(bounds.Size);

                if (x + desired.X > bounds.Right && x > bounds.X)
                {
                    // Umbruch
                    x = bounds.X;
                    y += lineHeight + Spacing;
                    lineHeight = 0;
                }

                var rect = new Rectangle(x, y, desired.X, desired.Y);
                widget.Arrange(rect);

                x += desired.X + Spacing;
                lineHeight = System.Math.Max(lineHeight, desired.Y);
            }
        }

        private void ArrangeVertical(IEnumerable<Widget> widgets, Rectangle bounds)
        {
            int x = bounds.X;
            int y = bounds.Y;
            int lineWidth = 0;

            foreach (var widget in widgets)
            {
                var desired = widget.Measure(bounds.Size);

                if (y + desired.Y > bounds.Bottom && y > bounds.Y)
                {
                    // Umbruch
                    y = bounds.Y;
                    x += lineWidth + Spacing;
                    lineWidth = 0;
                }

                var rect = new Rectangle(x, y, desired.X, desired.Y);
                widget.Arrange(rect);

                y += desired.Y + Spacing;
                lineWidth = System.Math.Max(lineWidth, desired.X);
            }
        }
    }
}
