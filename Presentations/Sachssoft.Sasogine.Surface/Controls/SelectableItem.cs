using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class SelectableItem : ElementBase
    {
        private readonly SelectableItemPresenter _contentPresenter;

        public event EventHandler? IsSelectedChanged;

        public SelectableItem()
        {
            _contentPresenter = CreateContentPresenter();
        }

        #region Style Properties

        public StyleProperty<object?> Content
        {
            get => _contentPresenter.Content;
            set => _contentPresenter.Content = value;
        }

        public StyleProperty<bool> IsSelected
        {
            get => _contentPresenter.IsSelected;
            set => _contentPresenter.IsSelected = value;
        }

        public StyleProperty<bool> IsSelectable
        {
            get => _contentPresenter.IsSelectable;
            set => _contentPresenter.IsSelectable = value;
        }

        #endregion

        #region Direct Properties

        public SelectableItemPresenter ContentPresenter => _contentPresenter;

        public ISelectionHost? Owner { get; internal set; }

        #endregion

        #region Style

        protected override ElementBase CreateCloneInstance()
        {
            return new SelectableItem();
        }

        #endregion

        protected virtual SelectableItemPresenter CreateContentPresenter() => new SelectableItemPresenter();

        protected virtual void OnIsSelectedChanged()
        {
            IsSelectedChanged?.Invoke(this, EventArgs.Empty);

            if (Owner != null)
            {
                foreach (var selectableItem in Owner.Selectables)
                {
                    if (selectableItem != this)
                        selectableItem.IsSelected = false;
                }
            }
        }
    }
}
