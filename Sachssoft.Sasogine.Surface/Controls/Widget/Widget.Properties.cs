using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Interactions;
using Sachssoft.Sasogine.Surface.Styles;
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
        private StyleProperty<MouseCursorType?> _mouseCursorType = new StyleProperty<MouseCursorType?>(null, isUserSet: false);
        private StyleProperty<Thickness> _margin = new StyleProperty<Thickness>(new Thickness(), isUserSet: false);
        private StyleProperty<Thickness> _borderThickness = new StyleProperty<Thickness>(new Thickness(), isUserSet: false);
        private StyleProperty<Thickness> _padding = new StyleProperty<Thickness>(new Thickness(), isUserSet: false);
        private StyleProperty<int> _left = new StyleProperty<int>(0, isUserSet: false);
        private StyleProperty<int> _top = new StyleProperty<int>(0, isUserSet: false);
        private StyleProperty<int?> _minWidth = new StyleProperty<int?>(null, isUserSet: false);
        private StyleProperty<int?> _minHeight = new StyleProperty<int?>(null, isUserSet: false);
        private StyleProperty<int?> _maxWidth = new StyleProperty<int?>(null, isUserSet: false);
        private StyleProperty<int?> _maxHeight = new StyleProperty<int?>(null, isUserSet: false);
        private StyleProperty<int?> _width = new StyleProperty<int?>(null, isUserSet: false);
        private StyleProperty<int?> _height = new StyleProperty<int?>(null, isUserSet: false);
        private StyleProperty<int> _zIndex = new StyleProperty<int>(0, isUserSet: false);
        private StyleProperty<HorizontalAlignment> _horizontalAlignment = new StyleProperty<HorizontalAlignment>(Visuals.HorizontalAlignment.Left, isUserSet: false);
        private StyleProperty<VerticalAlignment> _verticalAlignment = new StyleProperty<VerticalAlignment>(Visuals.VerticalAlignment.Top, isUserSet: false);
        private StyleProperty<float> _opacity = new StyleProperty<float>(1.0f, isUserSet: false);
        private StyleProperty<bool> _isKeyboardFocused = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<Vector2> _scale = new StyleProperty<Vector2>(Vector2.One, isUserSet: false);
        private StyleProperty<Vector2> _transformOrigin = new StyleProperty<Vector2>(new Vector2(0.5f, 0.5f), isUserSet: false);
        private StyleProperty<float> _rotation = new StyleProperty<float>(0.0f, isUserSet: false);
        private StyleProperty<DragDirection> _dragDirection = new StyleProperty<DragDirection>(Controls.DragDirection.None, isUserSet: false);
        private StyleProperty<IBrush?> _background = new StyleProperty<IBrush?>(null, isUserSet: false);
        private StyleProperty<IBrush?> _hoveredBackground = new StyleProperty<IBrush?>(null, isUserSet: false);
        private StyleProperty<IBrush?> _disabledBackground = new StyleProperty<IBrush?>(null, isUserSet: false);
        private StyleProperty<IBrush?> _focusedBackground = new StyleProperty<IBrush?>(null, isUserSet: false);
        private StyleProperty<IBrush?> _border = new StyleProperty<IBrush?>(null, isUserSet: false);
        private StyleProperty<IBrush?> _hoveredBorder = new StyleProperty<IBrush?>(null, isUserSet: false);
        private StyleProperty<IBrush?> _disabledBorder = new StyleProperty<IBrush?>(null, isUserSet: false);
        private StyleProperty<IBrush?> _focusedBorder = new StyleProperty<IBrush?>(null, isUserSet: false);
        private StyleProperty<bool> _clipToBounds = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<bool> _acceptsKeyboardFocus = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<HorizontalAlignment> _textHorizontalAlignment = new StyleProperty<HorizontalAlignment>(Visuals.HorizontalAlignment.Left, isUserSet: false);
        private StyleProperty<VerticalAlignment> _textVerticalAlignment = new StyleProperty<VerticalAlignment>(Visuals.VerticalAlignment.Top, isUserSet: false);
        private StyleProperty<Font> _font = new StyleProperty<Font>(DefaultStyle.TextFont, isUserSet: false);
        private StyleProperty<Color> _textColor = new StyleProperty<Color>(DefaultStyle.TextColor, isUserSet: false);
        private StyleProperty<Color> _disabledTextColor = new StyleProperty<Color>(DefaultStyle.DisabledTextColor, isUserSet: false);
        private StyleProperty<Color> _overTextColor = new StyleProperty<Color>(DefaultStyle.OverTextColor, isUserSet: false);
        private StyleProperty<Color> _pressedTextColor = new StyleProperty<Color>(DefaultStyle.PressedTextColor, isUserSet: false);
        private StyleProperty<bool> _isHitTestVisible = new StyleProperty<bool>(true, isUserSet: false);

        public StyleProperty<bool> IsHitTestVisible
        {
            get => _isHitTestVisible;
            set => SetAndNotify(ref _isHitTestVisible, value);
        }

        public StyleProperty<Font> Font
        {
            get => _font;
            set
            {
                if (SetAndNotify(ref _font, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<HorizontalAlignment> TextHorizontalAlignment
        {
            get => _textHorizontalAlignment;
            set
            {
                if (SetAndNotify(ref _textHorizontalAlignment, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<VerticalAlignment> TextVerticalAlignment
        {
            get => _textVerticalAlignment;
            set
            {
                if (SetAndNotify(ref _textVerticalAlignment, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<Color> TextColor
        {
            get => _textColor;
            set => SetAndNotify(ref _textColor, value);
        }

        public StyleProperty<Color> DisabledTextColor
        {
            get => _disabledTextColor;
            set => SetAndNotify(ref _disabledTextColor, value);
        }

        public StyleProperty<Color> OverTextColor
        {
            get => _overTextColor;
            set => SetAndNotify(ref _overTextColor, value);
        }

        public StyleProperty<Color> PressedTextColor
        {
            get => _pressedTextColor;
            set => SetAndNotify(ref _pressedTextColor, value);
        }

        public StyleProperty<int> Left
        {
            get => _left;
            set
            {
                if (SetAndNotify(ref _left, value))
                {
                    InvalidateTransform();
                    FireLocationChanged();
                }
            }
        }

        public StyleProperty<int> Top
        {
            get => _top;
            set
            {
                if (SetAndNotify(ref _top, value))
                {
                    InvalidateTransform();
                    FireLocationChanged();
                }
            }
        }

        public StyleProperty<int?> MinWidth
        {
            get => _minWidth;
            set
            {
                if (SetAndNotify(ref _minWidth, value))
                {
                    InvalidateMeasure();
                    FireSizeChanged();
                }
            }
        }

        public StyleProperty<int?> MaxWidth
        {
            get => _maxWidth;
            set
            {
                if (SetAndNotify(ref _maxWidth, value))
                {
                    InvalidateMeasure();
                    FireSizeChanged();
                }
            }
        }

        public StyleProperty<int?> Width
        {
            get => _width;
            set
            {
                if (SetAndNotify(ref _width, value))
                {
                    InvalidateMeasure();
                    FireSizeChanged();
                }
            }
        }

        public StyleProperty<int?> MinHeight
        {
            get => _minHeight;
            set
            {
                if (SetAndNotify(ref _minHeight, value))
                {
                    InvalidateMeasure();
                    FireSizeChanged();
                }
            }
        }

        public StyleProperty<int?> MaxHeight
        {
            get => _maxHeight;
            set
            {
                if (SetAndNotify(ref _maxHeight, value))
                {
                    InvalidateMeasure();
                    FireSizeChanged();
                }
            }
        }

        public StyleProperty<int?> Height
        {
            get => _height;
            set
            {
                if (SetAndNotify(ref _height, value))
                {
                    InvalidateMeasure();
                    FireSizeChanged();
                }
            }
        }

        public StyleProperty<Thickness> Margin
        {
            get => _margin;
            set
            {
                if (SetAndNotify(ref _margin, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<IBrush?> Border
        {
            get => _border;
            set => SetAndNotify(ref _border, value);
        }

        public StyleProperty<Thickness> BorderThickness
        {
            get => _borderThickness;
            set
            {
                if (SetAndNotify(ref _borderThickness, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<Thickness> Padding
        {
            get => _padding;
            set
            {
                if (SetAndNotify(ref _padding, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<HorizontalAlignment> HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                if (SetAndNotify(ref _horizontalAlignment, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<VerticalAlignment> VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                if (SetAndNotify(ref _verticalAlignment, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<DragDirection> DragDirection
        {
            get => _dragDirection;
            set => SetAndNotify(ref _dragDirection, value);
        }

        public StyleProperty<int> ZIndex
        {
            get => _zIndex;
            set
            {
                if (SetAndNotify(ref _zIndex, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<MouseCursorType?> MouseCursor
        {
            get => _mouseCursorType;
            set
            {
                if (SetAndNotify(ref _mouseCursorType, value))
                {
                    _mouseCursorType = value;
                    foreach (var child in Children)
                    {
                        child.MouseCursor = value;
                    }
                }
            }
        }

        public StyleProperty<Vector2> Scale
        {
            get => _scale;
            set
            {
                if (SetAndNotify(ref _scale, value))
                {
                    InvalidateTransform();
                }
            }
        }

        public StyleProperty<Vector2> TransformOrigin
        {
            get => _transformOrigin;
            set
            {
                if (SetAndNotify(ref _transformOrigin, value))
                {
                    InvalidateTransform();
                }
            }
        }

        public StyleProperty<float> Rotation
        {
            get => _rotation;
            set
            {
                if (SetAndNotify(ref _rotation, value))
                {
                    InvalidateTransform();
                }
            }
        }

        public StyleProperty<float> Opacity
        {
            get => _opacity;
            set => SetAndNotify(ref _opacity, float.Clamp(value, 0.0f, 1.0f));
        }

        public StyleProperty<IBrush?> Background
        {
            get => _background;
            set => SetAndNotify(ref _background, value);
        }

        public StyleProperty<IBrush?> HoveredBackground
        {
            get => _hoveredBackground;
            set => SetAndNotify(ref _hoveredBackground, value);
        }

        public StyleProperty<IBrush?> DisabledBackground
        {
            get => _disabledBackground;
            set => SetAndNotify(ref _disabledBackground, value);
        }

        public StyleProperty<IBrush?> FocusedBackground
        {
            get => _focusedBackground;
            set => SetAndNotify(ref _focusedBackground, value);
        }

        public StyleProperty<IBrush?> HoveredBorder
        {
            get => _hoveredBorder;
            set => SetAndNotify(ref _hoveredBorder, value);
        }

        public StyleProperty<IBrush?> DisabledBorder
        {
            get => _disabledBorder;
            set => SetAndNotify(ref _disabledBorder, value);
        }

        public StyleProperty<IBrush?> FocusedBorder
        {
            get => _focusedBorder;
            set => SetAndNotify(ref _focusedBorder, value);
        }

        public virtual StyleProperty<bool> ClipToBounds
        {
            get => _clipToBounds;
            set => SetAndNotify(ref _clipToBounds, value);
        }

        public virtual StyleProperty<bool> AcceptsKeyboardFocus
        {
            get => _acceptsKeyboardFocus;
            set => SetAndNotify(ref _acceptsKeyboardFocus, value);
        }
    }
}
