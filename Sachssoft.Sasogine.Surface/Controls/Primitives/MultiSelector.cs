using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Dynamics;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public abstract class MultiSelector : SelectorBase
    {
        private StyleProperty<int[]> _selectedIndices = new StyleProperty<int[]>(new int[0], isUserSet: false);
        private StyleProperty<SelectionMode> _selectionMode = new StyleProperty<SelectionMode>(Controls.SelectionMode.Single, isUserSet: false);

        #region Style Properties

        public StyleProperty<int[]> SelectedIndices
        {
            get => _selectedIndices;
            set
            {
                var oldIndices = _selectedIndices.Value;

                if (SetAndNotify(ref _selectedIndices, value))
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        var presenter = Items.GetPresenterByIndex(i);
                        presenter.IsSelected = false;

                        foreach (var index in _selectedIndices.Value)
                        {
                            if (index == i)
                            {
                                presenter.IsSelected = true;
                                break;
                            }
                        }
                    }
                    OnSelectionChanged(new Behaviors.SelectionChangedEventArgs(oldIndices, _selectedIndices.Value));
                }
            }
        }

        public StyleProperty<SelectionMode> SelectionMode
        {
            get => _selectionMode;
            set
            {
                if (SetAndNotify(ref _selectionMode, value))
                {
                    if (_selectionMode.Value == Controls.SelectionMode.Single)
                    {
                        var selectedItems = Items.GetPresenters()
                                                 .Where(x => x.IsSelected);
                        bool first = true;

                        foreach (var item in selectedItems)
                        {
                            if (first)
                            {
                                first = false;
                                continue;
                            }

                            using (item.SuppressNotifications())
                                item.IsSelected = false;
                        }
                    }

                    OnSelectionModeChanged(EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Direct Properties

        public override sealed bool AllowMultiple => (_selectionMode.Value == Controls.SelectionMode.Multiple);

        public IEnumerable<object?> SelectedItems
        {
            get
            {
                foreach (var presenter in Items.GetPresenters())
                {
                    if (presenter.IsSelected)
                        yield return presenter.Content.Value ?? presenter;
                }
            }
        }

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {

                    case nameof(SelectedIndices):
                        //target.SelectedIndices = target.SelectedIndices.Override(value.ConvertTo<int[]>());
                        break;

                    case nameof(SelectionMode):
                        target.SelectionMode = target.SelectionMode.Override(value.ConvertToEnum<SelectionMode>());
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not MultiSelector source)
                return;

            SelectedIndices = source.SelectedIndices;
            SelectionMode = source.SelectionMode;
        }

        #endregion

        #region Single Selection Helper

        // Ohne Auslöser an alle Items
        protected virtual void PutSelectedIndices(int[] indices)
        {
            var oldIndices = _selectedIndices;
            _selectedIndices = new StyleProperty<int[]>(indices);
            OnSelectionChanged(new Behaviors.SelectionChangedEventArgs(oldIndices, _selectedIndices.Value));
        }

        public int GetSelectedIndex()
        {
            ThrowIfMultiple();

            if (_selectedIndices.Value.Length == 0)
                return -1;

            return _selectedIndices.Value[0];
        }

        public void SelectItemByIndex(int index)
        {
            if (index < 0 || index >= Items.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var old = _selectedIndices.Value;
            if (old.Length == 1 && old[0] == index)
                return; // keine Änderung nötig

            SelectedIndices = new int[] { index };
        }

        public void EnsureSelection(int index = 0)
        {
            if (index < 0 || index >= Items.Count)
                return;

            if (_selectedIndices.Value.Length == 0 && Items.Count > 0)
            {
                SelectedIndices = new int[] { index };
            }
        }

        public T? GetSelectedContent<T>()
        {
            ThrowIfMultiple();

            if (_selectedIndices.Value.Length == 0)
                return default;

            return (T?)Items[_selectedIndices.Value[0]];
        }

        public IEnumerable<T> GetSelectedContents<T>()
        {
            foreach (var presenter in Items.GetPresenters())
            {
                if (presenter.IsSelected && presenter.Content.Value is T t)
                    yield return t;
            }
        }

        public object? GetSelectedContent()
        {
            ThrowIfMultiple();

            if (_selectedIndices.Value.Length == 0)
                return default;

            return Items[_selectedIndices.Value[0]];
        }

        public SelectableItem? GetSelectedPresenter()
        {
            ThrowIfMultiple(); ;

            if (_selectedIndices.Value.Length == 0)
                return null;

            return Items.GetPresenterByIndex(_selectedIndices.Value[0]);
        }

        #endregion

        #region Helpers

        private void ThrowIfMultiple()
        {
            if (_selectionMode.Value != Controls.SelectionMode.Single)
                throw new InvalidOperationException("This operation is only valid in single selection mode.");
        }

        #endregion

        protected internal override void ApplySelectionIndices(int[] indices)
        {
            var oldIndices = _selectedIndices.Value;
            _selectedIndices = new StyleProperty<int[]>(indices, isUserSet: true);
            OnSelectionChanged(new Behaviors.SelectionChangedEventArgs(oldIndices, indices));
        }

        protected virtual void OnSelectionModeChanged(EventArgs e) { }
    }
}
