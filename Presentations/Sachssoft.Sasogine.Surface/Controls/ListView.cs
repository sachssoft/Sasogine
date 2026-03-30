using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class ListView : MultiSelector, IScrollViewerContent
    {
        private StyleProperty<bool> _isHorizontalScrollBarVisible = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<bool> _isVerticalScrollBarVisible = new StyleProperty<bool>(true, isUserSet: false);
        private StyleProperty<bool> _canScrollHorizontal = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<bool> _canScrollVertical = new StyleProperty<bool>(true, isUserSet: false);
        private StyleProperty<Point> _scrollPosition = new StyleProperty<Point>(Point.Zero, isUserSet: false);

        private readonly ScrollViewer _scrollViewer = new ScrollViewer();
        private Container _listPanel = new VerticalStackPanel();
        private Container? _oldPanelCache;

        public ListView()
        {
            AcceptsKeyboardFocus = AcceptsKeyboardFocus.Override(true);

            LayoutContainer = new SingleItemLayout<ScrollViewer>(this, _scrollViewer);

            _listPanel.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            //_listPanel.Background = Color.Red.ToBrush();
            _scrollViewer.Content = _listPanel;

            UpdateScrollViewer();
        }

        #region Style Properties  

        public StyleProperty<bool> IsHorizontalScrollBarVisible
        {
            get => _isHorizontalScrollBarVisible;
            set
            {
                if (SetAndNotify(ref _isHorizontalScrollBarVisible, value))
                    UpdateScrollViewer();
            }
        }

        public StyleProperty<bool> IsVerticalScrollBarVisible
        {
            get => _isVerticalScrollBarVisible;
            set
            {
                if (SetAndNotify(ref _isVerticalScrollBarVisible, value))
                    UpdateScrollViewer();
            }
        }

        public StyleProperty<bool> CanScrollHorizontal
        {
            get => _canScrollHorizontal;
            set
            {
                if (SetAndNotify(ref _canScrollHorizontal, value))
                    UpdateScrollViewer();
            }
        }

        public StyleProperty<bool> CanScrollVertical
        {
            get => _canScrollVertical;
            set
            {
                if (SetAndNotify(ref _canScrollVertical, value))
                    UpdateScrollViewer();
            }
        }

        public StyleProperty<Point> ScrollPosition
        {
            get => _scrollPosition;
            set
            {
                if (SetAndNotify(ref _scrollPosition, value))
                    UpdateScrollViewer();
            }
        }

        #endregion


        #region Direct Properties

        public Container Container
        {
            get => _listPanel;
            set
            {
                _oldPanelCache = _listPanel;

                if (SetAndNotify<Container>(ref _listPanel, value))
                {
                    if (_listPanel == null)
                        throw new InvalidOperationException("Null not allowed");

                    UpdatePanel();
                }
                else
                {
                    _oldPanelCache = null;
                }
            }
        }

        #endregion

        internal protected override void OnMouseWheel(float delta)
        {
            base.OnMouseWheel(delta);
            _scrollViewer.OnMouseWheel(delta);
        }

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            foreach (var item in Items.GetPresenters())
            {
                ApplyItemStyle(item);
            }

            style?.Apply<ListView>(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(IsHorizontalScrollBarVisible):
                        target.IsHorizontalScrollBarVisible = target.IsHorizontalScrollBarVisible.Override(value.ConvertTo<bool>());
                        break;
                    case nameof(IsVerticalScrollBarVisible):
                        target.IsVerticalScrollBarVisible = target.IsVerticalScrollBarVisible.Override(value.ConvertTo<bool>());
                        break;
                    case nameof(CanScrollHorizontal):
                        target.CanScrollHorizontal = target.CanScrollHorizontal.Override(value.ConvertTo<bool>());
                        break;
                    case nameof(CanScrollVertical):
                        target.CanScrollVertical = target.CanScrollVertical.Override(value.ConvertTo<bool>());
                        break;
                    case nameof(ScrollPosition):
                        target.ScrollPosition = target.ScrollPosition.Override(value.ConvertTo<Point>());
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not ListView source)
                return;

            IsHorizontalScrollBarVisible = source.IsHorizontalScrollBarVisible;
            IsVerticalScrollBarVisible = source.IsVerticalScrollBarVisible;
            CanScrollHorizontal = source.CanScrollHorizontal;
            CanScrollVertical = source.CanScrollVertical;
            ScrollPosition = source.ScrollPosition;

            // Panel übernehmen
            //if (source.Container != null)
            //{
            //    Container = (Container)source.Container.Clone();
            //}
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new ListView();
        }

        #endregion

        protected internal override void ItemPresenterAdded(SelectableItem item, int index)
        {
            base.ItemPresenterAdded(item, index);

            if (_listPanel != null)
            {
                _listPanel.Widgets.Insert(index, CreateItemPresenter(item));
            }
        }

        protected internal override void ItemPresenterRemoved(SelectableItem item, int index)
        {
            base.ItemPresenterRemoved(item, index);
            _listPanel.Widgets.RemoveAt(index);

            item.ContentPresenter.IsSelectedChanged -= Presenter_IsSelectedChanged;
        }

        protected internal override void CollectionReset()
        {
            base.CollectionReset();
            _listPanel.Widgets.Clear();
        }

        private void UpdatePanel()
        {
            if (_oldPanelCache == null)
                return;

            // Widgets vom alten Panel in das neue Panel übernehmen
            _listPanel.Widgets.Clear();
            foreach (var widget in _oldPanelCache.Widgets)
            {
                _listPanel.Widgets.Add(widget);
            }

            // ScrollViewer aktualisieren
            _scrollViewer.Content = _listPanel;

            // Alte Panel-Referenz nicht mehr benötigt
            _oldPanelCache = null;
        }

        private void UpdateScrollViewer()
        {
            // Alle Scroll-bezogenen Eigenschaften auf den ScrollViewer anwenden.
            _scrollViewer.IsHorizontalScrollBarVisible = _isHorizontalScrollBarVisible;
            _scrollViewer.IsVerticalScrollBarVisible = _isVerticalScrollBarVisible;
            _scrollViewer.CanScrollHorizontal = _canScrollHorizontal;
            _scrollViewer.CanScrollVertical = _canScrollVertical;
            _scrollViewer.ScrollPosition = _scrollPosition;
        }

        private Widget CreateItemPresenter(SelectableItem item)
        {
            var presenter = item.ContentPresenter;
            presenter.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            presenter.ContentHorizontalAlignment = Visuals.HorizontalAlignment.Left;
            presenter.IsSelectedChanged += Presenter_IsSelectedChanged;

            ApplyItemStyle(item);

            return presenter;
        }

        private void ApplyItemStyle(SelectableItem item)
        {
            if (Style != null)
                item.ContentPresenter.ApplyFromStyle(Style.FindStyle(typeof(SelectableItemPresenter), "item"));
        }

        private void Presenter_IsSelectedChanged(object? sender, Behaviors.ValueChangedEventArgs<bool> e)
        {
            var selectedIndices = new List<int>();

            if (SelectionMode.Value == Controls.SelectionMode.Single)
            {
                for (int i = 0; i < _listPanel.Widgets.Count; i++)
                {
                    if (_listPanel.Widgets[i] is ISelectable selectable)
                    {
                        selectable.IsSelectedChanged -= Presenter_IsSelectedChanged;

                        if (selectable == sender)
                        {
                            selectable.IsSelected = true;
                            selectedIndices.Add(i);
                        }
                        else
                        {
                            selectable.IsSelected = false;
                        }

                        selectable.IsSelectedChanged += Presenter_IsSelectedChanged;
                    }
                }

            }
            else
            {
                for (int i = 0; i < _listPanel.Widgets.Count; i++)
                {
                    var item = _listPanel.Widgets[i] as ISelectable;
                    if (item != null)
                        selectedIndices.Add(i);
                }
            }

            PutSelectedIndices(new StyleProperty<int[]>(selectedIndices.ToArray()));
            //SelectedIndices = new StyleProperty<int[]>(selectedIndices.ToArray());
        }

        protected override void OnSelectionModeChanged(EventArgs e)
        {
            base.OnSelectionModeChanged(e);

            ISelectable? lastSelected = null;

            foreach (var item in _listPanel.Widgets)
            {
                if (item is ISelectable selectable)
                {
                    if (selectable.IsSelected && lastSelected == null)
                    {
                        lastSelected = selectable;
                    }
                    else
                    {
                        selectable.IsSelectedChanged -= Presenter_IsSelectedChanged;
                        selectable.IsSelected = false;
                        selectable.IsSelectedChanged += Presenter_IsSelectedChanged;
                    }
                }
            }
        }

        #region IScrollViewerContent

        bool IScrollViewerContent.IsHorizontalScrollBarVisible
        {
            get => _scrollViewer.IsHorizontalScrollBarVisible;
            set => _scrollViewer.IsHorizontalScrollBarVisible = value;
        }

        bool IScrollViewerContent.IsVerticalScrollBarVisible
        {
            get => _scrollViewer.IsVerticalScrollBarVisible;
            set => _scrollViewer.IsVerticalScrollBarVisible = value;
        }

        bool IScrollViewerContent.CanScrollHorizontal
        {
            get => _scrollViewer.CanScrollHorizontal;
            set => _scrollViewer.CanScrollHorizontal = value;
        }

        bool IScrollViewerContent.CanScrollVertical
        {
            get => _scrollViewer.CanScrollVertical;
            set => _scrollViewer.CanScrollVertical = value;
        }

        Point IScrollViewerContent.ScrollPosition
        {
            get => _scrollViewer.ScrollPosition;
            set => _scrollViewer.ScrollPosition = value;
        }

        #endregion

    }
}
