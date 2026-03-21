using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public abstract class SingleSelector : SelectorBase
    {
        private StyleProperty<int> _selectedIndex = new StyleProperty<int>(-1, isUserSet: false);

        #region Style Properties

        public StyleProperty<int> SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                var oldIndex = _selectedIndex.Value;

                if (SetAndNotify(ref _selectedIndex, value))
                {
                    // Nur ein Item selektieren
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var presenter = Items.GetPresenterByIndex(i);
                        presenter.IsSelected = (i == _selectedIndex.Value);
                    }

                    var oldIndices = new int[0];
                    if (oldIndex >= 0)
                        oldIndices = new int[] { oldIndex };

                    var newIndices = new int[0];
                    if (_selectedIndex.Value >= 0)
                        newIndices = new int[] { _selectedIndex.Value };

                    OnSelectionChanged(new Behaviors.SelectionChangedEventArgs(oldIndices, newIndices));
                }
            }
        }

        #endregion

        #region Direct Properties

        public override sealed bool AllowMultiple => false;

        public object? SelectedItem
        {
            get
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var presenter = Items.GetPresenterByIndex(i);
                    if (presenter.IsSelected)
                        return presenter.Content.Value ?? presenter;
                }

                return null;
            }
        }

        public bool IsSelected => Items.Count > 0 && 
            _selectedIndex.Value >= 0 && _selectedIndex.Value < Items.Count;

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(SelectedIndex):
                        target.SelectedIndex = target.SelectedIndex.Override(value.ConvertTo<int>());
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not SingleSelector source)
                return;

            SelectedIndex = source.SelectedIndex;
        }

        #endregion

        protected internal override void ApplySelectionIndices(int[] indices)
        {
            var oldIndex = _selectedIndex.Value;
            var newIndices = new int[0];

            if (indices.Length > 0)
            {
                _selectedIndex = indices[0];
                newIndices = new int[] { _selectedIndex };
            }
            else
            {
                _selectedIndex = -1;
            }

            OnSelectionChanged(new Behaviors.SelectionChangedEventArgs(new[] { oldIndex }, newIndices));
        }
    }
}
