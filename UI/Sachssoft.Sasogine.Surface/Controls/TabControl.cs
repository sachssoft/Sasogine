using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class TabControl : SingleSelector
    {
        private StyleProperty<TabItemPlacement> _placement = new StyleProperty<TabItemPlacement>(TabItemPlacement.TopLeft, false);
        private StyleProperty<int> _tabItemSpacing = new StyleProperty<int>(10, false);

        //private ContentControl _content = new ContentControl();
        private readonly Grid _contentLayout = new Grid();
        private StackPanel? _tabPanel;
        //private ITemplateFactory<TabItem>? _tabItemTemplate = null;
        private readonly GridLayout _layout = new GridLayout();
        private readonly SelectionItemCollection<TabItem> _items;
        private readonly List<SelectableItemPresenter> _tabPresenters = new List<SelectableItemPresenter>();

        public TabControl()
        {
            _items = new SelectionItemCollection<TabItem>(this);
            LayoutContainer = _layout;

            HorizontalAlignment = HorizontalAlignment.Override(Visuals.HorizontalAlignment.Stretch);
            VerticalAlignment = VerticalAlignment.Override(Visuals.VerticalAlignment.Stretch);

            UpdateLayout();
        }

        #region Style Properties

        public StyleProperty<TabItemPlacement> Placement
        {
            get => _placement;
            set
            {
                if (SetAndNotify(ref _placement, value))
                {
                    UpdateLayout();
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<int> TabItemSpacing
        {
            get => _tabItemSpacing;
            set
            {
                if (SetAndNotify(ref _tabItemSpacing, value))
                {
                    UpdateLayout();
                    InvalidateMeasure();
                }
            }
        }

        #endregion

        #region Direct Properties

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public override ISelectableItemPresenterCollection Items => _items;

        public SelectionItemCollection<TabItem> TabItems => _items;

        //public ITemplateFactory<TabItem>? TabItemTemplate
        //{
        //    get => _tabItemTemplate;
        //    set
        //    {
        //        if (SetAndNotify(ref _tabItemTemplate, value))
        //        {

        //        }
        //    }
        //}

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            // Style für das Content-Label anwenden
            var contentStyle = style?.FindStyle(typeof(ContentControl), "content");
            _contentLayout.ApplyFromStyle(contentStyle);

            foreach (var item in Items.GetPresenters())
            {
                if (item is TabItem tabItem)
                {
                    ApplyTabItemHeaderStyle(tabItem);
                    ApplyTabItemContentStyle(tabItem);
                }
            }
        }

        private void ApplyTabItemHeaderStyle(TabItem tabItem)
        {
            if (Style != null)
                tabItem.TabPresenter.ApplyFromStyle(Style.FindStyle(typeof(Button), "tab-header"));
        }

        private void ApplyTabItemContentStyle(TabItem tabItem)
        {
            if (Style != null)
                tabItem.ContentPresenter.ApplyFromStyle(Style.FindStyle(typeof(ContentControl), "tab-content"));
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not TabControl source)
                return;

            Placement = source.Placement;
            TabItemSpacing = source.TabItemSpacing;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new TabControl();
        }

        #endregion

        protected internal override SelectableItem CreateItemPresenter(object? content)
        {
            return new TabItem();
        }

        private void UpdateTabItemLayout()
        {
            if (_placement.Value == TabItemPlacement.LeftBottom || _placement.Value == TabItemPlacement.LeftTop ||
                _placement.Value == TabItemPlacement.RightBottom || _placement.Value == TabItemPlacement.RightTop)
            {
                foreach (var tabPresenter in _tabPresenters)
                {
                    tabPresenter.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
                }
            }
            else if (_placement.Value == TabItemPlacement.BottomLeft || _placement.Value == TabItemPlacement.TopLeft)
            {
                foreach (var tabPresenter in _tabPresenters)
                {
                    tabPresenter.HorizontalAlignment = Visuals.HorizontalAlignment.Left;
                }
            }
            else if (_placement.Value == TabItemPlacement.BottomRight || _placement.Value == TabItemPlacement.TopRight)
            {
                foreach (var tabPresenter in _tabPresenters)
                {
                    tabPresenter.HorizontalAlignment = Visuals.HorizontalAlignment.Right;
                }
            }
        }

        private void UpdateLayout()
        {
            Children.Clear();

            _layout.RowsProportions.Clear();
            _layout.ColumnsProportions.Clear();

            switch (_placement.Value)
            {
                case TabItemPlacement.None:
                    _tabPanel = null;
                    break;
                case TabItemPlacement.TopLeft:
                    _tabPanel = new HorizontalStackPanel();
                    _tabPanel.HorizontalAlignment = Visuals.HorizontalAlignment.Left;
                    _layout.RowsProportions.Add(new Proportion(ProportionType.Auto));
                    _layout.RowsProportions.Add(new Proportion(ProportionType.Fill));
                    Grid.SetColumn(_tabPanel, 0);
                    Grid.SetColumn(_contentLayout, 0);
                    Grid.SetRow(_tabPanel, 0);
                    Grid.SetRow(_contentLayout, 1);
                    break;
                case TabItemPlacement.TopRight:
                    _tabPanel = new HorizontalStackPanel();
                    _tabPanel.HorizontalAlignment = Visuals.HorizontalAlignment.Right;
                    _layout.RowsProportions.Add(new Proportion(ProportionType.Auto));
                    _layout.RowsProportions.Add(new Proportion(ProportionType.Fill));
                    Grid.SetColumn(_tabPanel, 0);
                    Grid.SetColumn(_contentLayout, 0);
                    Grid.SetRow(_tabPanel, 0);
                    Grid.SetRow(_contentLayout, 1);
                    break;
                case TabItemPlacement.BottomLeft:
                    _tabPanel = new HorizontalStackPanel();
                    _tabPanel.HorizontalAlignment = Visuals.HorizontalAlignment.Left;
                    _layout.RowsProportions.Add(new Proportion(ProportionType.Fill));
                    _layout.RowsProportions.Add(new Proportion(ProportionType.Auto));
                    Grid.SetColumn(_tabPanel, 0);
                    Grid.SetColumn(_contentLayout, 0);
                    Grid.SetRow(_contentLayout, 0);
                    Grid.SetRow(_tabPanel, 1);
                    break;
                case TabItemPlacement.BottomRight:
                    _tabPanel = new HorizontalStackPanel();
                    _tabPanel.HorizontalAlignment = Visuals.HorizontalAlignment.Right;
                    _layout.RowsProportions.Add(new Proportion(ProportionType.Fill));
                    _layout.RowsProportions.Add(new Proportion(ProportionType.Auto));
                    Grid.SetColumn(_tabPanel, 0);
                    Grid.SetColumn(_contentLayout, 0);
                    Grid.SetRow(_contentLayout, 0);
                    Grid.SetRow(_tabPanel, 1);
                    break;
                case TabItemPlacement.LeftTop:
                    _tabPanel = new VerticalStackPanel();
                    _tabPanel.VerticalAlignment = Visuals.VerticalAlignment.Top;
                    _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
                    _layout.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
                    Grid.SetColumn(_tabPanel, 0);
                    Grid.SetColumn(_contentLayout, 1);
                    Grid.SetRow(_tabPanel, 0);
                    Grid.SetRow(_contentLayout, 0);
                    break;
                case TabItemPlacement.LeftBottom:
                    _tabPanel = new VerticalStackPanel();
                    _tabPanel.VerticalAlignment = Visuals.VerticalAlignment.Bottom;
                    _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
                    _layout.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
                    Grid.SetColumn(_tabPanel, 0);
                    Grid.SetColumn(_contentLayout, 1);
                    Grid.SetRow(_tabPanel, 0);
                    Grid.SetRow(_contentLayout, 0);
                    break;
                case TabItemPlacement.RightTop:
                    _tabPanel = new VerticalStackPanel();
                    _tabPanel.VerticalAlignment = Visuals.VerticalAlignment.Top;
                    _layout.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
                    _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
                    Grid.SetColumn(_contentLayout, 0);
                    Grid.SetColumn(_tabPanel, 1);
                    Grid.SetRow(_tabPanel, 0);
                    Grid.SetRow(_contentLayout, 0);
                    break;
                case TabItemPlacement.RightBottom:
                    _tabPanel = new VerticalStackPanel();
                    _tabPanel.VerticalAlignment = Visuals.VerticalAlignment.Bottom;
                    _layout.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
                    _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
                    Grid.SetColumn(_contentLayout, 0);
                    Grid.SetColumn(_tabPanel, 1);
                    Grid.SetRow(_tabPanel, 0);
                    Grid.SetRow(_contentLayout, 0);
                    break;
            }            

            _contentLayout.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            _contentLayout.VerticalAlignment = Visuals.VerticalAlignment.Stretch;

            Children.Add(_contentLayout);
            if (_tabPanel != null)
            {
                _tabPanel.Spacing = _tabItemSpacing.Value;
                _tabPanel.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
                _tabPanel.VerticalAlignment = Visuals.VerticalAlignment.Stretch;
                Children.Add(_tabPanel);
            }

            UpdateTabItemLayout();
        }

        protected internal override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (e.NewIndices.Length > 0)
            {
                SelectContent();
            }
        }

        protected internal override void ItemPresenterAdded(SelectableItem item, int index)
        {
            base.ItemPresenterAdded(item, index);

            if (_tabPanel != null && item is TabItem tabItem)
            {
                tabItem.Owner = this;
                _tabPanel.Widgets.Insert(index, CreateTabItemPresenter(tabItem));
                SelectContent(); // Content aktualisieren
                UpdateTabItemLayout();
            }
        }

        protected internal override void ItemPresenterRemoved(SelectableItem presenter, int index)
        {
            base.ItemPresenterRemoved(presenter, index);

            if (presenter is TabItem tabItem)
            {
                if (_tabPanel != null)
                    _tabPanel.Widgets.RemoveAt(index);

                using (SuppressNotifications())
                    SelectedIndex = -1;

                tabItem.Owner = null;
            }
        }

        protected internal override void CollectionReset()
        {
            base.CollectionReset();

            if (_tabPanel != null)
                _tabPanel.Widgets.Clear();

            _tabPresenters.Clear();

            using (SuppressNotifications())
                SelectedIndex = -1;
        }

        private Widget CreateTabItemPresenter(TabItem tabItem)
        {
            var presenter = tabItem.TabPresenter;
            presenter.IsSelected = (_items.Count == 1); // erster Tab
            presenter.IsSelectedChanged += Presenter_IsSelectedChanged;

            _tabPresenters.Add(presenter);

            // Setze SelectedIndex nur, falls es noch keinen gibt
            if (SelectedIndex.Value == -1)
            {
                using (SuppressNotifications())
                    SelectedIndex = 0;
            }

            ApplyTabItemHeaderStyle(tabItem);

            return presenter;
        }

        private void Presenter_IsSelectedChanged(object? sender, System.EventArgs e)
        {
            foreach (var item in _tabPresenters)
            {
                item.IsSelectedChanged -= Presenter_IsSelectedChanged;
                item.IsSelected = (item == sender);
                item.IsSelectedChanged += Presenter_IsSelectedChanged;
            }

            SelectedIndex = _tabPresenters.IndexOf((SelectableItemPresenter)sender!);
        }

        private void SelectContent()
        {
            var selectedIndex = SelectedIndex.Value;

            if (selectedIndex >= 0 && selectedIndex < _items.Count)
            {
                _contentLayout.Widgets.Clear();

                var item = _items.GetPresenterByIndex(selectedIndex);

                if (item.ContentPresenter != null)
                {
                    _contentLayout.Widgets.Add(item.ContentPresenter);
                    ApplyTabItemContentStyle(item);
                }
            }
        }
    }
}