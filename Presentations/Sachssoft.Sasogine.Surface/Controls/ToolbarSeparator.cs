using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Visuals;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class ToolbarSeparator : ToolbarItemBase
    {
        private readonly Separator _presenter = new Separator();

        public ToolbarSeparator()
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
            _presenter.Background = null;
            _presenter.VerticalAlignment = VerticalAlignment.Center;
            _presenter.HorizontalAlignment = HorizontalAlignment.Center;

            if (Owner is Toolbar toolbar)
            {
                if (toolbar.Orientation.Value == Orientation.Horizontal)
                {
                    _presenter.Orientation = Orientation.Vertical;
                    _presenter.Padding = new Thickness(0, 4);
                }
                else
                {
                    _presenter.Orientation = Orientation.Horizontal;
                    _presenter.Padding = new Thickness(4, 0);
                }

                _presenter.Thickness = toolbar.SeparatorSpacing;
                _presenter.Height = toolbar.BarSize;
            }
        }

        #region Style

        protected override ElementBase CreateCloneInstance()
        {
            return new ToolbarSeparator();
        }

        #endregion
    }
}
