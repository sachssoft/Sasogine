using Sachssoft.Sasogine.Interactions;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class Toolbar : CommandBarBase<Toolbar, ToolbarItemBase>
    {
        private StyleProperty<int> _spacing = new StyleProperty<int>(2, isUserSet: false);
        private StyleProperty<int> _separatorSpacing = new StyleProperty<int>(4, isUserSet: false);

        private HorizontalWrapPanel _container;

        public Toolbar()
        {
        }

        #region Style Property

        public StyleProperty<int> Spacing
        {
            get => _spacing;
            set
            {
                if (SetAndNotify(ref _spacing, value))
                {
                    _container.Spacing = value;
                }
            }
        }

        public StyleProperty<int> SeparatorSpacing
        {
            get => _separatorSpacing;
            set
            {
                if (SetAndNotify(ref _separatorSpacing, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            // Button-spezifische Properties
            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(Spacing):
                        target.Spacing = target.Spacing.Override(value.ConvertTo<int>());
                        break;

                    case nameof(SeparatorSpacing):
                        target.SeparatorSpacing = target.SeparatorSpacing.Override(value.ConvertTo<int>());
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not Toolbar source)
                return;

            Spacing = source.Spacing;
            SeparatorSpacing = source.SeparatorSpacing;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new Toolbar();
        }

        #endregion

        protected override Container CreateContainer()
        {
            _container = new HorizontalWrapPanel();
            return _container;
        }
    }
}
