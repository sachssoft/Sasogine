using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class TabItem : SelectableItem
    {
        private readonly SelectableItemPresenter _tabPresenter;

        public TabItem()
        {
            _tabPresenter = CreateTabPresenter();
        }

        #region Style Properties

        public StyleProperty<object?> Header
        {
            get => _tabPresenter.Content;
            set => _tabPresenter.Content = value;
        }

        #endregion

        #region Direct Properties

        public Widget? HeaderPresenter
        {
            get => _tabPresenter.ContentPresenter;
        }

        public SelectableItemPresenter TabPresenter => _tabPresenter;

        //public TabControl? Owner { get; internal set; }

        #endregion

        #region Style

        protected override ElementBase CreateCloneInstance()
        {
            return new TabItem();
        }

        #endregion

        //protected override void OnIsSelectedChanged()
        //{
        //    base.OnIsSelectedChanged();

        //    if (Owner != null)
        //    {
        //        foreach(var tabItem in Owner.TabItems)
        //        {
        //            if (tabItem != null)
        //                tabItem.IsSelected = false;
        //        }
        //    }
        //}

        protected virtual SelectableItemPresenter CreateTabPresenter()
        {
            return new SelectableItemPresenter()
            {
                IsSelectable = true,
                IsSelected = false,
                ContentHorizontalAlignment = Visuals.HorizontalAlignment.Stretch,
                ContentVerticalAlignment = Visuals.VerticalAlignment.Stretch,
                Tag = "TabItem"
            };
        }

        protected override SelectableItemPresenter CreateContentPresenter()
        {
            return new SelectableItemPresenter()
            {
                IsSelectable = false,
                IsSelected = false,
                ContentHorizontalAlignment = Visuals.HorizontalAlignment.Stretch,
                ContentVerticalAlignment = Visuals.VerticalAlignment.Stretch
            };
        }
    }
}