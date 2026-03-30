using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Deterlite.Basic;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;
using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using System;

namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public abstract class FrameBase : IFrameChildHostInternal
    {
        private readonly FrameCollection _childFrames;
        private IFrameChildHostInternal? _parent = null;

        private bool _isMeasureValid;
        private bool _isArrangeValid;
        private bool _shouldInvalidate;

        private bool _isVisible = true;
        private Vector2 _desiredSize = Vector2.Zero;
        private Bounds _bounds = Bounds.Zero;
        private Bounds _contentBounds = Bounds.Zero;

        private Insets _padding = Insets.None;
        private Insets _margin = Insets.None;

        private FrameLayer _layer = FrameLayer.Content;
        private int _zIndex = 0;
        private bool _clipToBounds = false;

        private float _width = float.NaN;
        private float _minWidth = float.NaN;
        private float _maxWidth = float.NaN;
        private float _height = float.NaN;
        private float _minHeight = float.NaN;
        private float _maxHeight = float.NaN;

        private Alignment _horizontalAlignment = Alignment.Near;
        private Alignment _verticalAlignment = Alignment.Near;
        private bool _horizontalStretch = false;
        private bool _verticalStretch = false;
        private FrameSizeBehavior _widthBehavior = FrameSizeBehavior.Auto;
        private FrameSizeBehavior _heightBehavior = FrameSizeBehavior.Auto;

        private FlowDirection _layoutDirection = FlowDirection.LeftToRight;

        private IBrush? _backgroundBrush;
        private IBrush? _foregroundBrush;

        public FrameBase()
        {
            _childFrames = new FrameCollection(this);
        }

        internal protected FrameCollection ChildFrames => _childFrames;

        FrameCollection IFrameChildHost.ChildFrames => _childFrames;

        public Vector2 DesiredSize => _desiredSize;

        public Bounds Bounds => _bounds;

        public Bounds ContentBounds => _contentBounds;

        public bool ShouldInvalidate => _shouldInvalidate;

        // ---- Performance-unkritische Felder, daher keine Benachrichtigung bei Änderung nötig

        public string? Id { get; set; }

        public string? Class { get; set; }

        public object? Tag { get; set; } // für beliebige Daten

        // ---- Ende der performance-unkritischen Felder

        public FrameBase? Parent
        {
            get => _parent as FrameBase;
            internal set => _parent = value;
        }

        IFrameChildHost? IFrameChildHostInternal.Parent
        {
            get => _parent;
            set => _parent = value as IFrameChildHostInternal;
        }

        // Layer z.B. Content, Overlay, Popup, Tooltip etc.
        // Bestimmt die Renderreihenfolge bzw. logische UI-Gruppierung
        public FrameLayer Layer
        {
            get => _layer;
            protected init
            {
                if (_layer == value) return;   // kein Equals nötig bei Enum
                _layer = value;

                // Reihenfolge der Frames hat sich geändert → Cache neu sortieren
                Invalidate();
            }
        }

        // Sichtbarkeit
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;
                Invalidate();
            }
        }

        // Z-Index (Renderreihenfolge)
        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (Equals(_zIndex, value)) return;
                _zIndex = value;
                Invalidate();
            }
        }

        // Clippen von Inhalten außerhalb der Bounds
        // Falls true, wird Scissor-Rect gesetzt und Inhalte außerhalb der Bounds werden nicht gerendert
        public bool ClipToBounds
        {
            get => _clipToBounds;
            set
            {
                if (Equals(_clipToBounds, value)) return;
                _clipToBounds = value;
                Invalidate();
            }
        }

        // Padding / Margin
        public Insets Padding
        {
            get => _padding;
            set
            {
                if (Equals(_padding, value)) return;
                _padding = value;
                Invalidate();
            }
        }

        public Insets Margin
        {
            get => _margin;
            set
            {
                if (Equals(_margin, value)) return;
                _margin = value;
                Invalidate();
            }
        }
        public float Width
        {
            get => _width;
            set
            {
                if (_width == value) return;
                _width = LayoutMath.ResolveDimension(value); // NaN bleibt Auto, negativ -> 0
                Invalidate();
            }
        }

        public float MinWidth
        {
            get => _minWidth;
            set
            {
                if (_minWidth == value) return;
                _minWidth = LayoutMath.ResolveDimensionMinimum(value);

                // Min darf Max nicht übersteigen
                if (_minWidth > _maxWidth)
                    _maxWidth = _minWidth;

                Invalidate();
            }
        }

        public float MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (_maxWidth == value) return;
                _maxWidth = LayoutMath.ResolveDimensionMaximum(value);

                // Max darf Min nicht unterschreiten
                if (_maxWidth < _minWidth)
                    _minWidth = _maxWidth;

                Invalidate();
            }
        }

        public float Height
        {
            get => _height;
            set
            {
                if (_height == value) return;
                _height = LayoutMath.ResolveDimension(value);
                Invalidate();
            }
        }

        public float MinHeight
        {
            get => _minHeight;
            set
            {
                if (_minHeight == value) return;
                _minHeight = LayoutMath.ResolveDimensionMinimum(value);

                if (_minHeight > _maxHeight)
                    _maxHeight = _minHeight;

                Invalidate();
            }
        }

        public float MaxHeight
        {
            get => _maxHeight;
            set
            {
                if (_maxHeight == value) return;
                _maxHeight = LayoutMath.ResolveDimensionMaximum(value);

                if (_maxHeight < _minHeight)
                    _minHeight = _maxHeight;

                Invalidate();
            }
        }

        // Alignment
        public Alignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                if (_horizontalAlignment == value) return;
                _horizontalAlignment = value;
                Invalidate();
            }
        }

        public Alignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                if (_verticalAlignment == value) return;
                _verticalAlignment = value;
                Invalidate();
            }
        }

        public bool HorizontalStretch
        {
            get => _horizontalStretch;
            set
            {
                if (_horizontalStretch == value) return;
                _horizontalStretch = value;
                Invalidate();
            }
        }

        public bool VerticalStretch
        {
            get => _verticalStretch;
            set
            {
                if (_verticalStretch == value) return;
                _verticalStretch = value;
                Invalidate();
            }
        }

        public FrameSizeBehavior WidthBehavior
        {
            get => _widthBehavior;
            set
            {
                if (_widthBehavior == value) return;
                _widthBehavior = value;
                Invalidate();
            }
        }

        public FrameSizeBehavior HeightBehavior
        {
            get => _heightBehavior;
            set
            {
                if (_heightBehavior == value) return;
                _heightBehavior = value;
                Invalidate();
            }
        }

        public FlowDirection LayoutDirection
        {
            get => _layoutDirection;
            set
            {
                if (_layoutDirection == value) return;
                _layoutDirection = value;
                Invalidate();
            }
        }

        public IBrush? BackgroundBrush
        {
            get => _backgroundBrush;
            set => _backgroundBrush = value;
        }

        public IBrush? ForegroundBrush
        {
            get => _foregroundBrush;
            set => _foregroundBrush = value;
        }

        // Layout / Measure / Arrange
        public virtual void Measure(Vector2 availableSize)
        {
            if (float.IsNaN(availableSize.X) || float.IsNaN(availableSize.Y))
                throw new InvalidOperationException("Cannot call Measure using a size with NaN values.");

            if (_isMeasureValid)
                return;

            if (!_isVisible)
            {
                _desiredSize = new Vector2();
                return;
            }

            // Min- und Max-Größen berechnen
            var minSize = new Vector2(
                LayoutMath.ResolveDimensionMinimum(_minWidth),
                LayoutMath.ResolveDimensionMinimum(_minHeight)
            );

            var maxSize = new Vector2(
                LayoutMath.ResolveDimensionMaximum(_maxWidth),
                LayoutMath.ResolveDimensionMaximum(_maxHeight)
            );

            // Feste Width/Height berücksichtigen (nur wenn nicht NaN)
            var fixedSize = new Vector2(
                LayoutMath.ResolveDimension(_width),
                LayoutMath.ResolveDimension(_height)
            );

            // constrained: übergebe entweder die feste Größe oder AvailableSize, innerhalb von Min/Max
            var constrained = new Vector2(
                float.IsNaN(fixedSize.X)
                    ? float.Max(minSize.X, float.Min(availableSize.X, maxSize.X))
                    : LayoutMath.Clamp(fixedSize.X, minSize.X, maxSize.X),
                float.IsNaN(fixedSize.Y)
                    ? float.Max(minSize.Y, float.Min(availableSize.Y, maxSize.Y))
                    : LayoutMath.Clamp(fixedSize.Y, minSize.Y, maxSize.Y)
            );

            // Kind messen
            var measured = MeasureOverride(constrained);

            // Wenn feste Width/Height gesetzt sind, überschreiben sie measured
            if (!float.IsNaN(fixedSize.X))
                measured.X = fixedSize.X;

            if (!float.IsNaN(fixedSize.Y))
                measured.Y = fixedSize.Y;

            // Ergebnis innerhalb von Min/Max clampen
            var width = LayoutMath.Clamp(measured.X, minSize.X, maxSize.X);
            var height = LayoutMath.Clamp(measured.Y, minSize.Y, maxSize.Y);

            // Margin berücksichtigen
            width += _margin.Left + _margin.Right;
            height += _margin.Top + _margin.Bottom;

            // Nicht größer als availableSize und nicht negativ
            width = float.Max(0, float.Min(width, availableSize.X));
            height = float.Max(0, float.Min(height, availableSize.Y));

            var desiredSize = new Vector2(width, height);

            if (!LayoutMath.IsValidMeasure(desiredSize))
                throw new InvalidOperationException("Invalid size returned for Measure.");

            _desiredSize = desiredSize;
            _isMeasureValid = true;
        }

        protected virtual Vector2 MeasureOverride(Vector2 availableSize)
        {
            var width = 0f;
            var height = 0f;

            var childFrames = ChildFrames;
            var frameCount = childFrames.Count;

            for (var i = 0; i < frameCount; i++)
            {
                FrameBase frame = childFrames[i];

                frame.Measure(availableSize);
                var childSize = frame.DesiredSize;

                if (childSize.X > width)
                    width = childSize.X;

                if (childSize.Y > height)
                    height = childSize.Y;
            }

            return new Vector2(width, height);
        }

        public virtual void Arrange(Bounds finalBounds)
        {
            if (!LayoutMath.IsValidArrange(finalBounds))
                throw new InvalidOperationException("Invalid Arrange bounds.");

            if (!_isMeasureValid)
                Measure(finalBounds.Size);

            if (_isArrangeValid || !_isVisible)
                return;

            // Ursprung unter Berücksichtigung von Margin
            var originX = finalBounds.X + _margin.Left;
            var originY = finalBounds.Y + _margin.Top;

            var availableWidthMinusMargins = float.Max(finalBounds.Width - _margin.Left - _margin.Right, 0f);
            var availableHeightMinusMargins = float.Max(finalBounds.Height - _margin.Top - _margin.Bottom, 0f);
            var availableSizeMinusMargins = new Vector2(availableWidthMinusMargins, availableHeightMinusMargins);

            // -------------------------------
            // Width/Height auflösen
            var resolvedWidth = LayoutMath.ResolveDimension(_width);
            var resolvedHeight = LayoutMath.ResolveDimension(_height);

            float newWidth = resolvedWidth;
            float newHeight = resolvedHeight;

            // WidthBehavior
            if (float.IsNaN(resolvedWidth) || float.IsPositiveInfinity(resolvedWidth))
            {
                newWidth = _widthBehavior switch
                {
                    FrameSizeBehavior.Auto => _horizontalStretch ? availableWidthMinusMargins : 0f,
                    FrameSizeBehavior.Stretch => availableWidthMinusMargins,
                    FrameSizeBehavior.Zero => 0f,
                    _ => throw new InvalidOperationException("Invalid WidthBehavior.")
                };
            }

            // HeightBehavior
            if (float.IsNaN(resolvedHeight) || float.IsPositiveInfinity(resolvedHeight))
            {
                newHeight = _heightBehavior switch
                {
                    FrameSizeBehavior.Auto => _verticalStretch ? availableHeightMinusMargins : 0f,
                    FrameSizeBehavior.Stretch => availableHeightMinusMargins,
                    FrameSizeBehavior.Zero => 0f,
                    _ => throw new InvalidOperationException("Invalid HeightBehavior.")
                };
            }

            // -------------------------------
            // Min/Max korrigieren
            float minWidth = LayoutMath.ResolveDimensionMinimum(_minWidth);
            float maxWidth = LayoutMath.ResolveDimensionMaximum(_maxWidth);
            if (minWidth > maxWidth) maxWidth = minWidth;

            float minHeight = LayoutMath.ResolveDimensionMinimum(_minHeight);
            float maxHeight = LayoutMath.ResolveDimensionMaximum(_maxHeight);
            if (minHeight > maxHeight) maxHeight = minHeight;

            // -------------------------------
            // Clamp Width/Height an Min/Max und AvailableSize
            newWidth = LayoutMath.Clamp(newWidth, minWidth, float.Min(maxWidth, availableWidthMinusMargins));
            newHeight = LayoutMath.Clamp(newHeight, minHeight, float.Min(maxHeight, availableHeightMinusMargins));

            var size = new Vector2(newWidth, newHeight);

            // ArrangeOverride
            size = LayoutMath.Constrain(ArrangeOverride(size), size);

            // -------------------------------
            // Alignment
            if (!_horizontalStretch)
            {
                switch (_horizontalAlignment)
                {
                    case Alignment.Center:
                        originX += (availableWidthMinusMargins - size.X) / 2f;
                        break;
                    case Alignment.Far:
                        originX += availableWidthMinusMargins - size.X;
                        break;
                }
            }

            if (!_verticalStretch)
            {
                switch (_verticalAlignment)
                {
                    case Alignment.Center:
                        originY += (availableHeightMinusMargins - size.Y) / 2f;
                        break;
                    case Alignment.Far:
                        originY += availableHeightMinusMargins - size.Y;
                        break;
                }
            }

            var origin = new Vector2(originX, originY);

            // -------------------------------
            // Bounds setzen (sicher gegen NaN/Infinity)
            _bounds = new Bounds(
                new Vector2(
                    float.IsNaN(origin.X) || float.IsInfinity(origin.X) ? 0f : origin.X,
                    float.IsNaN(origin.Y) || float.IsInfinity(origin.Y) ? 0f : origin.Y
                ),
                new Vector2(
                    float.IsNaN(size.X) || float.IsInfinity(size.X) ? 0f : size.X,
                    float.IsNaN(size.Y) || float.IsInfinity(size.Y) ? 0f : size.Y
                )
            );

            _contentBounds = new Bounds(
                _bounds.X + _padding.Left,
                _bounds.Y + _padding.Top,
                float.Max(0, _bounds.Width - _padding.Horizontal),
                float.Max(0, _bounds.Height - _padding.Vertical)
            );

            _isArrangeValid = true;
            _shouldInvalidate = false;
        }

        protected virtual Vector2 ArrangeOverride(Vector2 finalSize)
        {
            var arrangeBounds = new Bounds(Vector2.Zero, finalSize);

            var childFrames = ChildFrames;
            var frameCount = childFrames.Count;

            for (var i = 0; i < frameCount; i++)
            {
                FrameBase frame = childFrames[i];

                frame.Arrange(arrangeBounds);
            }

            return finalSize;
        }

        // Invalidierung
        public void Invalidate()
        {
            if (_shouldInvalidate) return;

            _shouldInvalidate = true;
            _isMeasureValid = false;
            _isArrangeValid = false;
        }

        internal virtual protected void Render(FrameContext context)
        {
            var render = context.Render;

            // optional Hintergrund
            _backgroundBrush?.Render(_bounds, render);
        }

        internal protected virtual void LayoutUpdated() { }

        public override string ToString()
        {
            return $"{GetType()} (Id={Id})";
        }
    }
}