using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class ComboBox : SingleSelector, IScrollViewerContent
    {
        private StyleProperty<bool> _isHorizontalScrollBarVisible = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<bool> _isVerticalScrollBarVisible = new StyleProperty<bool>(true, isUserSet: false);
        private StyleProperty<bool> _canScrollHorizontal = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<bool> _canScrollVertical = new StyleProperty<bool>(true, isUserSet: false);
        private StyleProperty<Point> _scrollPosition = new StyleProperty<Point>(Point.Zero, isUserSet: false);
        private StyleProperty<int> _itemHeight = new StyleProperty<int>(25, isUserSet: false);
        private StyleProperty<int> _maxDropDownWidth = new StyleProperty<int>(0, isUserSet: false);
        private StyleProperty<int> _maxDropDownHeight = new StyleProperty<int>(150, isUserSet: false);
        private StyleProperty<IndicatorPosition> _indicatorPosition = new StyleProperty<IndicatorPosition>(Primitives.IndicatorPosition.Left, isUserSet: false);
        private StyleProperty<int> _indicatorWidth = new StyleProperty<int>(16, isUserSet: false);
        private StyleProperty<object?> _noSelectionDisplay = new StyleProperty<object?>(null, isUserSet: false);
        private StyleProperty<object?> _multiSelectionDisplay = new StyleProperty<object?>(null, isUserSet: false);

        private readonly GridLayout _layout = new GridLayout();
        private readonly DropdownButton _dropDownButton = new DropdownButton();
        private readonly StateIndicator _indicator = new StateIndicator();
        private readonly ListView _listView = new ListView();
        private readonly ContentControl _display = new ContentControl();

        public ComboBox()
        {
            //Background = Color.Blue.ToBrush();

            AcceptsKeyboardFocus = AcceptsKeyboardFocus.Override(true);
            //ClipToBounds = ClipToBounds.Override(true);

            _layout.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));

            Grid.SetColumn(_display, 0);
            Grid.SetColumn(_indicator, 1);
            Grid.SetColumn(_dropDownButton, 0);
            Grid.SetColumnSpan(_dropDownButton, 2);

            LayoutContainer = _layout;
            Children.Add(_display);
            Children.Add(_indicator);
            Children.Add(_dropDownButton);

            //_indicator.MinWidth = 16;

            _listView.TouchUp += ListView_TouchUp;
            _listView.SelectionChanged += ListView_SelectionChanged;

            _display.Background = null;
            _display.ClipToBounds = true;
            //_display.Content = "Test";
            _display.VerticalAlignment = _display.VerticalAlignment.Override(Visuals.VerticalAlignment.Center);
            _display.HorizontalAlignment = _display.HorizontalAlignment.Override(Visuals.HorizontalAlignment.Left);

            // Unsichtbar bleiben, nur als Popup-Funktion
            _dropDownButton.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            _dropDownButton.ContentHorizontalAlignment = Visuals.HorizontalAlignment.Left;
            _dropDownButton.DropDown = PopupBase.Create(_listView);
            _dropDownButton.Content = _display;
            _dropDownButton.Background = null;
            _dropDownButton.PressedBackground = null;
            _dropDownButton.HoveredBackground = null;
            _dropDownButton.FocusedBackground = null;
            _dropDownButton.Width = null;
            _dropDownButton.Opacity = 0f;

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

        public StyleProperty<int> ItemHeight
        {
            get => _itemHeight;
            set
            {
                if (SetAndNotify(ref _itemHeight, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<int> MaxDropDownWidth
        {
            get => _maxDropDownWidth;
            set
            {
                if (SetAndNotify(ref _maxDropDownWidth, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<int> MaxDropDownHeight
        {
            get => _maxDropDownHeight;
            set
            {
                if (SetAndNotify(ref _maxDropDownHeight, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<IndicatorPosition> IndicatorPosition
        {
            get => _indicatorPosition;
            set
            {
                if (SetAndNotify(ref _indicatorPosition, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<int> IndicatorWidth
        {
            get => _indicatorWidth;
            set
            {
                if (SetAndNotify(ref _indicatorWidth, value))
                {
                    InvalidateMeasure();
                }
            }
        }

        public StyleProperty<object?> NoSelectionDisplay
        {
            get => _noSelectionDisplay;
            set
            {
                if (SetAndNotify(ref _noSelectionDisplay, value))
                {
                }
            }
        }

        public StyleProperty<object?> MultiSelectionText
        {
            get => _multiSelectionDisplay;
            set
            {
                if (SetAndNotify(ref _multiSelectionDisplay, value))
                {
                }
            }
        }

        #endregion


        #region Direct Properties

        public override ISelectableItemPresenterCollection Items => _listView.Items;

        public Container Panel
        {
            get => _listView.Container;
            set
            {
                var panel = _listView.Container;
                if (SetAndNotify<Container>(ref panel, value))
                {
                    _listView.Container = panel;
                }
            }
        }

        #endregion

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateDisplay();
        }

        protected override void InternalArrange()
        {
            base.InternalArrange();

            _listView.Width = int.Max(MaxDropDownWidth.Value, Bounds.Width);
            _listView.Height = int.Max(MaxDropDownHeight.Value, Bounds.Height);

            //_display.Width = Bounds.Width; // - IndicatorWidth.Value;
            //_display.Left = (IndicatorPosition.Value == Primitives.IndicatorPosition.Left) ?
            //    0 : IndicatorWidth.Value;

            //_dropDownButton.Width = Bounds.Width;0
            //_dropDownButton.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            _dropDownButton.DropDownPosition = new Point(-_dropDownButton.ContainerBounds.Left, Bounds.Height);
        }

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            var displayStyle = style?.FindStyle(typeof(ContentControl), "display");
            _display.ApplyFromStyle(displayStyle);

            _dropDownButton.Background = null;
            _dropDownButton.PressedBackground = null;
            _dropDownButton.HoveredBackground = null;
            _dropDownButton.FocusedBackground = null;

            //style?.Apply<ComboBox>(this, (target, sheet, property, value) =>
            //{
            //    switch (property)
            //    {
            //    }
            //});
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not ComboBox source)
                return;

            IsHorizontalScrollBarVisible = source.IsHorizontalScrollBarVisible;
            IsVerticalScrollBarVisible = source.IsVerticalScrollBarVisible;
            CanScrollHorizontal = source.CanScrollHorizontal;
            CanScrollVertical = source.CanScrollVertical;
            ScrollPosition = source.ScrollPosition;
            ItemHeight = source.ItemHeight;
            MaxDropDownWidth = source.MaxDropDownWidth;
            MaxDropDownHeight = source.MaxDropDownHeight;
            IndicatorPosition = source.IndicatorPosition;
            IndicatorWidth = source.IndicatorWidth;
            NoSelectionDisplay = source.NoSelectionDisplay;
            MultiSelectionText = source.MultiSelectionText;

            // Anzeige- und DropDown-bezogene Unterelemente
            _display.ApplyFrom(source._display);
            _indicator.ApplyFrom(source._indicator);
            _listView.ApplyFrom(source._listView);
            _dropDownButton.ApplyFrom(source._dropDownButton);

            UpdateScrollViewer();
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new ComboBox();
        }

        #endregion

        private void ListView_TouchUp(object? sender, EventArgs e)
        {
            _dropDownButton.DropDown.Close();
            _dropDownButton.IsPressed = false;
        }

        private void ListView_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            var list = new List<object?>();
            foreach (var item in _listView.SelectedItems)
                list.Add(item);

            if (list.Count == 0)
            {
                _display.Content = NoSelectionDisplay.Value;
            }
            else if (list.Count == 1)
            {
                var item = list[0];
                _display.Content = new StyleProperty<object?>(item.CloneOrSelf());
            }
            else
            {
                _display.Content = MultiSelectionText.Value;
            }
        }

        private void UpdateScrollViewer()
        {
            // Alle Scroll-bezogenen Eigenschaften auf den ScrollViewer anwenden.
            _listView.IsHorizontalScrollBarVisible = _isHorizontalScrollBarVisible;
            _listView.IsVerticalScrollBarVisible = _isVerticalScrollBarVisible;
            _listView.CanScrollHorizontal = _canScrollHorizontal;
            _listView.CanScrollVertical = _canScrollVertical;
            _listView.ScrollPosition = _scrollPosition;
        }


        #region IScrollViewerContent

        bool IScrollViewerContent.IsHorizontalScrollBarVisible
        {
            get => _listView.IsHorizontalScrollBarVisible;
            set => _listView.IsHorizontalScrollBarVisible = value;
        }

        bool IScrollViewerContent.IsVerticalScrollBarVisible
        {
            get => _listView.IsVerticalScrollBarVisible;
            set => _listView.IsVerticalScrollBarVisible = value;
        }

        bool IScrollViewerContent.CanScrollHorizontal
        {
            get => _listView.CanScrollHorizontal;
            set => _listView.CanScrollHorizontal = value;
        }

        bool IScrollViewerContent.CanScrollVertical
        {
            get => _listView.CanScrollVertical;
            set => _listView.CanScrollVertical = value;
        }

        Point IScrollViewerContent.ScrollPosition
        {
            get => _listView.ScrollPosition;
            set => _listView.ScrollPosition = value;
        }

        #endregion
    }
}
