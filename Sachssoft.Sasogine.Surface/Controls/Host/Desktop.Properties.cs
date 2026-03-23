using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Utility;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public partial class Desktop
    {
        private StyleProperty<IBrush?> _background = new StyleProperty<IBrush?>(null, isUserSet: false);
        private StyleProperty<float> _opacity = new StyleProperty<float>(1.0f, isUserSet: false);
        private StyleProperty<Vector2> _scale = new StyleProperty<Vector2>(Vector2.One, isUserSet: false);
        private StyleProperty<Vector2> _transformOrigin = new StyleProperty<Vector2>(Vector2.Zero, isUserSet: false);
        private StyleProperty<float> _rotation = new StyleProperty<float>(0f, isUserSet: false);
       
        private SceneBase? _scene;
        private Game _game;

        #region Style Properties

        public StyleProperty<float> Opacity
        {
            get => _opacity;
            set => SetAndNotify(ref _opacity, value);
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

        public Vector2 TransformOrigin
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

        public StyleProperty<IBrush?> Background
        {
            get => _background;
            set => SetAndNotify(ref _background, value);
        }

        #endregion

        #region Direct Properties

        public CultureInfo? Culture
        {
            get => _culture;
            set
            {
                if (_culture != value)
                {
                    _culture = value;
                    CultureChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public SceneBase? Scene
        {
            get => _scene;
            set
            {
                _scene = value;

                if (value != null)
                {
                    _view_set = true;
                    RootElement = value.Container;
                    _view_set = false;
                }
                else
                {
                    RootElement = null;
                }

                SceneChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public IPresentationHostElement? RootElement
        {
            get
            {
                if (Widgets.Count == 0)
                {
                    return null;
                }

                return Widgets[0];
            }

            set
            {
                if (Scene != null && !_view_set)
                {
                    throw new InvalidOperationException("Scenes set");
                }

                if (RootElement == value)
                {
                    return;
                }

                ClosePopup();
                HideTooltip();
                Widgets.Clear();

                if (value != null)
                {
                    Widgets.Add((Widget)value);
                }
            }
        }

        internal Transform Transform
        {
            get
            {
                if (_transform == null)
                {
                    _transform = new Transform(_bounds.Location.ToVector2(),
                        TransformOrigin * _bounds.Size().ToVector2(),
                        Scale,
                        Rotation * float.Pi / 180);
                }

                return _transform.Value;
            }
        }

        #endregion
    }
}
