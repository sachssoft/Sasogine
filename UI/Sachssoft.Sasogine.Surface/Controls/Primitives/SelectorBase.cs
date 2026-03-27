using Sachssoft.Sasogine.Surface.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public abstract class SelectorBase : Widget, ISelectionHost
    {
        private readonly SelectionItemCollection<SelectableItem> _items;

        #region Events

        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        #endregion

        public SelectorBase()
        {

            _items = new SelectionItemCollection<SelectableItem>(this);
        }

        #region Direct Properties

        public abstract bool AllowMultiple { get; }

        public virtual ISelectableItemPresenterCollection Items => _items;

        public bool HasSelectedItems => _items.GetPresents()
                                              .Where(x => x.IsSelected)
                                              .Any();

        IEnumerable<ISelectable> ISelectionHost.Selectables => _items.Where(x => x is ISelectable)
                                                                     .Cast<ISelectable>();

        #endregion

        internal protected virtual void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        internal protected virtual SelectableItem CreateItemPresenter(object? content)
        {
            return new SelectableItem()
            {
                Content = content
            };
        }

        internal protected virtual void ItemPresenterAdded(SelectableItem presenter, int index)
        {
        }

        internal protected virtual void ItemPresenterRemoved(SelectableItem presenter, int index)
        {
        }

        internal protected virtual void CollectionReset()
        {
        }

        internal protected virtual void ApplySelectionIndices(int[] indices)
        {
        }
    }
}
