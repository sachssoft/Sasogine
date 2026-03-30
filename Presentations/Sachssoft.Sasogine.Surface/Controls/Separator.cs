using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class Separator : Widget
    {
        private StyleProperty<Orientation> _orientation = new StyleProperty<Orientation>(Visuals.Orientation.Horizontal, isUserSet: false);
        private StyleProperty<int> _thickness = new StyleProperty<int>(3, isUserSet: false);

        public Separator()
        {
        }

        #region Style Properties

        public StyleProperty<Orientation> Orientation
        {
            get => _orientation;
            set
            {
                if (SetAndNotify(ref _orientation, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<int> Thickness
        {
            get => _thickness;
            set
            {
                if (SetAndNotify(ref _thickness, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        #endregion

        #region Style

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not Separator source)
                return;

            Orientation = source.Orientation;
            Thickness = source.Thickness;
        }

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(Orientation):
                        target.Orientation = target.Orientation.Override(value.ConvertToEnum<Orientation>());
                        break;
                    case nameof(Thickness):
                        target.Thickness = target.Thickness.Override(value.ConvertTo<int>());
                        break;
                }
            });
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new Separator();
        }

        #endregion

        protected override Point InternalMeasure(Point availableSize)
        {
            if (_orientation.Value == Visuals.Orientation.Horizontal)
            {
                return new Point(0, _thickness.Value);
            }
            else
            {
                return new Point(_thickness.Value, 0);
            }
        }

        public override void InternalRender(RenderContext context, GameTime time)
        {
            if (Background.Value != null)
            {
                var width = (_orientation.Value == Visuals.Orientation.Horizontal) ? ActualBounds.Width : _thickness.Value;
                var height = (_orientation.Value == Visuals.Orientation.Vertical) ? ActualBounds.Height : _thickness.Value;

                Background.Value.Draw(context, new Rectangle(0, 0, width, height), Color.White);
                return;
            }
        }
    }
}
