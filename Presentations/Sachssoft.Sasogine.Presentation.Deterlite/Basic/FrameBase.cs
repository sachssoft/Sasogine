using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Deterlite.Basic;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;

namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public abstract class FrameBase : IFrameChildHostInternal
    {
        private readonly FrameCollection _childFrames;
        private IFrameChildHostInternal? _parent = null;

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
        private FlowDirection _layoutDirection = FlowDirection.LeftToRight;

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

        // Größe
        public float Width
        {
            get => _width;
            set
            {
                if (_width == value) return;
                _width = value;
                Invalidate();
            }
        }

        public float MinWidth
        {
            get => _minWidth;
            set
            {
                if (_minWidth == value) return;
                _minWidth = value;
                Invalidate();
            }
        }

        public float MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (_maxWidth == value) return;
                _maxWidth = value;
                Invalidate();
            }
        }

        public float Height
        {
            get => _height;
            set
            {
                if (_height == value) return;
                _height = value;
                Invalidate();
            }
        }

        public float MinHeight
        {
            get => _minHeight;
            set
            {
                if (_minHeight == value) return;
                _minHeight = value;
                Invalidate();
            }
        }

        public float MaxHeight
        {
            get => _maxHeight;
            set
            {
                if (_maxHeight == value) return;
                _maxHeight = value;
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

        // Layout / Measure / Arrange
        public void Measure(Vector2 availableSize)
        {
            // 1) Host selbst messen
            _desiredSize = MeasureOverride(availableSize);

            // 2) Kinder messen
            if (_childFrames.Count > 0)
            {
                Vector2 maxChildSize = Vector2.Zero;
                foreach (var child in _childFrames)
                {
                    child.Measure(availableSize);
                    maxChildSize.X = float.Max(maxChildSize.X, child.DesiredSize.X);
                    maxChildSize.Y = float.Max(maxChildSize.Y, child.DesiredSize.Y);
                }

                // 3) Größe des Hosts ggf. an Kinder anpassen
                _desiredSize.X = float.Max(_desiredSize.X, maxChildSize.X);
                _desiredSize.Y = float.Max(_desiredSize.Y, maxChildSize.Y);
            }
        }

        protected virtual Vector2 MeasureOverride(Vector2 availableSize) => Vector2.Zero;

        public void Arrange(Bounds finalBounds)
        {
            _bounds = ArrangeOverride(finalBounds);

            float offsetX = _padding.Left;
            float offsetY = _padding.Top;

            _contentBounds = new Bounds(
                _bounds.X + offsetX,
                _bounds.Y + offsetY,
                _bounds.Width - _padding.Horizontal,
                _bounds.Height - _padding.Vertical
            );

            foreach (var child in _childFrames.VisibleSorted)
            {
                Bounds childBounds = new Bounds(
                    _contentBounds.X + child.Margin.Left,
                    _contentBounds.Y + child.Margin.Top,
                    float.IsNaN(child.Width) ? _contentBounds.Width - child.Margin.Horizontal : child.Width,
                    float.IsNaN(child.Height) ? _contentBounds.Height - child.Margin.Vertical : child.Height
                );

                child.Arrange(childBounds);
            }

            _shouldInvalidate = false;
        }

        protected virtual Bounds ArrangeOverride(Bounds finalBounds) => finalBounds;

        // Invalidierung
        public void Invalidate()
        {
            if (_shouldInvalidate) return;
            _shouldInvalidate = true;
        }

        internal virtual protected void Render(GameTime gameTime, FrameContext context) { }

        internal protected virtual void LayoutUpdated() { }
    }
}