using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.ComponentModel;
using System.Xml;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class MenuSeparator : MenuItemBase
    {
        private readonly Separator _presenter = new Separator();

        public MenuSeparator()
        {
            UpdateSeparator();
        }

        internal protected override Widget Presenter => _presenter;

        protected override void OnOwnerPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnOwnerPropertyChanged(e);
            UpdateSeparator();
        }

        private void UpdateSeparator()
        {
            _presenter.Background = Color.Red.ToBrush();
            _presenter.VerticalAlignment = VerticalAlignment.Center;
            _presenter.HorizontalAlignment = HorizontalAlignment.Center;

            if (Owner is Menu menu)
            {
                if (menu.Orientation.Value == Orientation.Vertical)
                {
                    //_presenter.Thickness = 2;
                    _presenter.Orientation = Orientation.Vertical;
                    _presenter.VerticalAlignment = VerticalAlignment.Stretch;
                }
                else
                {
                    //_presenter.Thickness = 2;
                    _presenter.Orientation = Orientation.Horizontal;
                    _presenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                }
            }
        }

        public override void UpdateLayout(Rectangle containerBounds)
        {
            if (_presenter.Orientation.Value == Orientation.Vertical)
            {
                _presenter.Height = containerBounds.Height;
            }
            else
            {
                _presenter.Width = containerBounds.Width;
            }
        }

        #region Style

        protected override ElementBase CreateCloneInstance()
        {
            return new MenuSeparator();
        }

        #endregion
    }
}
