using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Layouts;
using System;

namespace Sachssoft.Sasogine.Presentation.Layouts
{
    public class StackLayout : LayoutBase
    {
        private Orientation _orientation = Orientation.Vertical;
        private float _spacing = 0f;

        public StackLayout() : this(Orientation.Vertical)
        {
        }

        public StackLayout(Orientation orientation) : this(orientation, 0f)
        {
        }

        public StackLayout(Orientation orientation, float spacing)
        {
            _orientation = orientation;
            _spacing = spacing;
        }

        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (_orientation == value) return;
                _orientation = value;
                Invalidate();
            }
        }

        public float Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing == value) return;
                _spacing = value;
                Invalidate();
            }
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            float width = 0;
            float height = 0;

            int count = Children.Count;

            for (int i = 0; i < count; i++)
            {
                var child = Children[i];
                if (!child.IsVisible)
                    continue;

                child.Measure(availableSize);
                var childSize = child.DesiredSize;

                if (_orientation == Orientation.Vertical)
                {
                    height += childSize.Y;
                    width = MathF.Max(width, childSize.X);
                }
                else
                {
                    width += childSize.X;
                    height = MathF.Max(height, childSize.Y);
                }
            }

            if (count > 1)
            {
                if (_orientation == Orientation.Vertical)
                    height += (count - 1) * _spacing;
                else
                    width += (count - 1) * _spacing;
            }

            return new Vector2(width, height);
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            float offset = 0;

            foreach (var child in Children)
            {
                if (!child.IsVisible)
                    continue;

                var desired = child.DesiredSize;

                if (_orientation == Orientation.Vertical)
                {
                    child.Arrange(new Bounds(
                        0,
                        offset,
                        finalSize.X,
                        desired.Y));

                    offset += desired.Y + _spacing;
                }
                else
                {
                    child.Arrange(new Bounds(
                        offset,
                        0,
                        desired.X,
                        finalSize.Y));

                    offset += desired.X + _spacing;
                }
            }

            return finalSize;
        }
    }
}