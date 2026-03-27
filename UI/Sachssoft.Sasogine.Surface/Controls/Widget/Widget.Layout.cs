using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Widget
    {
        private bool _measureDirty = true;
        private bool _arrangeDirty = true;
        private Rectangle _containerBounds;
        private Rectangle _layoutBounds;
        private Transform? _transform;
        private Matrix _inverseMatrix;
        private bool _inverseMatrixDirty = true;
        private readonly LayoutWidgetCollection _layoutChildren = new LayoutWidgetCollection();
        private bool _childrenDirty = true;

        #region Layout

        public ILayout? LayoutContainer { get; set; }

        protected internal IEnumerable<Widget> LayoutChildren
        {
            get
            {
                UpdateChildren();
                return _layoutChildren;
            }
        }

        private void UpdateChildren()
        {
            if (!_childrenDirty)
                return;

            // _layoutChildren vollständig neu befüllen
            _layoutChildren.Clear();
            _layoutChildren.AddRange(Children); // Children ist die ursprüngliche Widget-Collection

            // Cache invalidieren, damit beim Zugriff auf LayoutChildren die Sortierung erfolgt
            _layoutChildren.SortZIndex();

            _childrenDirty = false;
        }

        #endregion

        #region Measure

        public Point Measure(Point availableSize)
        {
            if (!_measureDirty && _lastMeasureAvailableSize == availableSize)
            {
                return _lastMeasureSize;
            }

            Point result;

            // Lerp available size by Width/Height or MaxWidth/MaxHeight
            if (Width.Value.HasValue && availableSize.X > Width.Value)
            {
                availableSize.X = Width.Value.Value;
            }
            else if (MaxWidth.Value.HasValue && availableSize.X > MaxWidth.Value)
            {
                availableSize.X = MaxWidth.Value.Value;
            }

            if (Height.Value.HasValue && availableSize.Y > Height.Value)
            {
                availableSize.Y = Height.Value.Value;
            }
            else if (MaxHeight.Value.HasValue && availableSize.Y > MaxHeight.Value)
            {
                availableSize.Y = MaxHeight.Value.Value;
            }

            availableSize.X -= MBPWidth;
            availableSize.Y -= MBPHeight;

            // Do the actual measure
            // Previously I skipped this step if both Width & Height were set
            // However that raised an issue - custom InternalMeasure stuff(such as in Menu.InternalMeasure) was skipped as well
            // So now InternalMeasure is called every time
            result = InternalMeasure(availableSize);

            result.X += MBPWidth;
            result.Y += MBPHeight;

            // Result lerp
            if (Width.Value.HasValue)
            {
                result.X = Width.Value.Value;
            }
            else
            {
                if (MinWidth.Value.HasValue && result.X < MinWidth.Value)
                {
                    result.X = MinWidth.Value.Value;
                }

                if (MaxWidth.Value.HasValue && result.X > MaxWidth.Value)
                {
                    result.X = MaxWidth.Value.Value;
                }
            }

            if (Height.Value.HasValue)
            {
                result.Y = Height.Value.Value;
            }
            else
            {
                if (MinHeight.Value.HasValue && result.Y < MinHeight.Value)
                {
                    result.Y = MinHeight.Value.Value;
                }

                if (MaxHeight.Value.HasValue && result.Y > MaxHeight.Value)
                {
                    result.Y = MaxHeight.Value.Value;
                }
            }

            _lastMeasureSize = result;
            _lastMeasureAvailableSize = availableSize;
            _measureDirty = false;

            return result;
        }

        // Hilfsmethode für Clamp
        private static int MeasureClamp(int value, int? min, int? max, int? explicitValue)
        {
            if (explicitValue.HasValue)
                return explicitValue.Value;

            if (min.HasValue && value < min.Value)
                value = min.Value;

            if (max.HasValue && value > max.Value)
                value = max.Value;

            return value;
        }

        protected virtual Point MeasureOverride(Point availableSize)
        {
            if (LayoutContainer == null)
                return Mathematics.PointZero;

            return LayoutContainer.Measure(LayoutChildren, availableSize);
        }

        protected virtual Point InternalMeasure(Point availableSize)
        {
            if (LayoutContainer == null)
            {
                return Mathematics.PointZero;
            }

            return LayoutContainer.Measure(LayoutChildren, availableSize);
        }

        public virtual void InvalidateMeasure()
        {
            _measureDirty = true;

            InvalidateArrange();

            if (Parent != null)
            {
                Parent.InvalidateMeasure();
            }
            else if (Desktop != null)
            {
                Desktop.InvalidateLayout();
            }
        }

        #endregion

        #region Arrange

        public void Arrange(Rectangle containerBounds)
        {
            if (!_arrangeDirty && _containerBounds == containerBounds)
            {
                return;
            }

            _arrangeDirty = true;
            _containerBounds = containerBounds;
            UpdateArrange();
        }

        public void UpdateArrange()
        {
            if (!_arrangeDirty)
            {
                return;
            }

            Point size;
            if (HorizontalAlignment.Value != Visuals.HorizontalAlignment.Stretch ||
                    VerticalAlignment.Value != Visuals.VerticalAlignment.Stretch)
            {
                size = Measure(_containerBounds.Size());
            }
            else
            {
                size = _containerBounds.Size();
            }

            if (size.X > _containerBounds.Width)
            {
                size.X = _containerBounds.Width;
            }

            if (size.Y > _containerBounds.Height)
            {
                size.Y = _containerBounds.Height;
            }

            // Resolve possible conflict beetween Alignment set to Streth and Size explicitly set
            var containerSize = _containerBounds.Size();
            if (HorizontalAlignment == Visuals.HorizontalAlignment.Stretch && Width.Value.HasValue && Width.Value < containerSize.X)
            {
                containerSize.X = Width.Value.Value;
            }

            if (VerticalAlignment == Visuals.VerticalAlignment.Stretch && Height.Value.HasValue && Height.Value < containerSize.Y)
            {
                containerSize.Y = Height.Value.Value;
            }

            // Align
            var layoutBounds = LayoutUtils.Align(containerSize, size, HorizontalAlignment, VerticalAlignment);
            layoutBounds.Offset(_containerBounds.Location);

            _layoutBounds = layoutBounds;
            InvalidateTransform();

            InternalArrange();
            ArrangeUpdated?.Invoke(this, EventArgs.Empty);

            _arrangeDirty = false;
        }

        protected virtual Point ArrangeOverride(Point finalSize)
        {
            if (LayoutContainer == null)
                return finalSize;

            LayoutContainer.Arrange(LayoutChildren, new Rectangle(0, 0, finalSize.X, finalSize.Y));
            return finalSize;
        }

        protected virtual void InternalArrange()
        {
            // Weiterhin für spezielle Kinderlayouts nutzbar
            if (LayoutContainer == null) return;
            LayoutContainer.Arrange(LayoutChildren, ActualBounds);
        }


        public void InvalidateArrange()
        {
            _arrangeDirty = true;
        }

        #endregion

        #region Transform

        public Vector2 ToGlobal(Vector2 pos) => Transform.Apply(pos);

        public Point ToGlobal(Point pos) => Transform.Apply(pos);

        public Vector2 ToLocal(Vector2 source)
        {
            if (_inverseMatrixDirty)
            {
                _inverseMatrix = Matrix.Invert(Transform.Matrix);
                _inverseMatrixDirty = false;
            }

            return source.Transform(ref _inverseMatrix);
        }

        public Point ToLocal(Point pos) => ToLocal(new Vector2(pos.X, pos.Y)).ToPoint();

        public bool ContainsGlobalPoint(Point globalPos)
        {
            var localPos = ToLocal(globalPos);
            return BorderBounds.Contains(localPos);
        }

        internal virtual void InvalidateTransform()
        {
            _transform = null;
            _inverseMatrixDirty = true;

            foreach (var child in LayoutChildren)
            {
                child.InvalidateTransform();
            }
        }

        internal Transform Transform
        {
            get
            {
                if (_transform == null)
                {
                    var p = new Point(_layoutBounds.X + Left, _layoutBounds.Y + Top);

                    var localTransform = new Transform(p.ToVector2(),
                        TransformOrigin * _layoutBounds.Size().ToVector2(),
                        Scale,
                        Rotation * float.Pi / 180);

                    if (Parent != null)
                    {
                        var transform = Parent.Transform;
                        transform.AddTransform(ref localTransform);
                        _transform = transform;
                    }
                    else if (Desktop != null)
                    {
                        var transform = Desktop.Transform;
                        transform.AddTransform(ref localTransform);
                        _transform = transform;
                    }
                    else
                    {
                        _transform = localTransform;
                    }
                }

                return _transform.Value;
            }
        }


        #endregion

        #region Bounds

        public int MBPWidth => Margin.Value.Left + Margin.Value.Right +
                    BorderThickness.Value.Left + BorderThickness.Value.Right +
                    Padding.Value.Left + Padding.Value.Right;

        public int MBPHeight => Margin.Value.Top + Margin.Value.Bottom +
                    BorderThickness.Value.Top + BorderThickness.Value.Bottom +
                    Padding.Value.Top + Padding.Value.Bottom;


        /// <summary>
        /// Gesamte Fläche des Widgets inkl. Border & Padding (ohne Margin)
        /// </summary>
        public Rectangle Bounds => new Rectangle(0, 0, _layoutBounds.Width, _layoutBounds.Height);

        public Rectangle ActualBounds => Bounds - _margin.Value - _borderThickness.Value - _padding.Value;

        internal Rectangle BorderBounds => Bounds - _margin.Value;

        protected Rectangle BackgroundBounds => BorderBounds - _borderThickness.Value;


        /// <summary>
        /// Absolute Position auf Desktop / global
        /// </summary>
        public Rectangle AbsoluteBounds
        {
            get
            {
                var translation = Transform.Matrix.Translation;
                return new Rectangle((int)translation.X, (int)translation.Y, _layoutBounds.Width, _layoutBounds.Height);
            }
        }

        /// <summary>
        /// Fläche innerhalb Padding für Kinder-Layout
        /// </summary>
        public Rectangle ContainerBounds => ActualBounds;

        #endregion
    }
}
