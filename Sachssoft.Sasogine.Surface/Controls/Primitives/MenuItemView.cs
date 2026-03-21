using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public class MenuItemView : Widget
    {
        private StyleProperty<bool> _isPressed = new StyleProperty<bool>(false, false);
        private StyleProperty<IBrush?> _pressedBackground = new StyleProperty<IBrush?>(null, false);
        private StyleProperty<MenuItemViewMode> _mode = new StyleProperty<MenuItemViewMode>(MenuItemViewMode.Root, isUserSet: false);
        private StyleProperty<ITextureRegion?> _icon = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
        private StyleProperty<object?> _header = new StyleProperty<object?>(null, isUserSet: false);
        private StyleProperty<Thickness> _iconMargin = new StyleProperty<Thickness>(new Thickness(2), isUserSet: false);
        private StyleProperty<int> _iconWidth = new StyleProperty<int>(24, isUserSet: false);
        private StyleProperty<int> _iconHeight = new StyleProperty<int>(24, isUserSet: false);

        private readonly GridLayout _layout = new GridLayout();
        private readonly Image _iconImage = new Image();
        private readonly DropdownButton _content = new DropdownButton();
        private readonly Image _indicator = new Image();
        private readonly MenuPopup _popup = new MenuPopup();

        public MenuItemView()
        {
            LayoutContainer  = _layout;

            //Padding = Padding.Override(new Thickness(10, 2, 10, 2));
            //Background = Background.Override(Color.Black.ToBrush());
            HoveredBackground = HoveredBackground.Override(Color.Blue.ToBrush());
            PressedBackground = HoveredBackground.Override(Color.Blue.ToBrush());
            HorizontalAlignment = HorizontalAlignment.Override(Visuals.HorizontalAlignment.Stretch);

            ConfigureContent();
            UpdateLayout();
        }

        #region Style Properties

        public StyleProperty<bool> IsPressed
        {
            get => _isPressed;
            set
            {
                if (SetAndNotify(ref _isPressed, value))
                {
                }
            }
        }

        public StyleProperty<IBrush?> PressedBackground
        {
            get => _pressedBackground;
            set => SetAndNotify(ref _pressedBackground, value);
        }

        public StyleProperty<MenuItemViewMode> Mode
        {
            get => _mode;
            set
            {
                if (SetAndNotify(ref _mode, value))
                {
                    UpdateLayout();
                }
            }
        }

        public StyleProperty<object?> Header
        {
            get => _header;
            set
            {
                if (SetAndNotify(ref _header, value))
                {
                    UpdateView();
                }
            }
        }

        public StyleProperty<ITextureRegion?> Icon
        {
            get => _icon;
            set
            {
                if (SetAndNotify(ref _icon, value))
                {
                    UpdateView();
                }
            }
        }

        public StyleProperty<Thickness> IconMargin
        {
            get => _iconMargin;
            set
            {
                if (SetAndNotify(ref _iconMargin, value))
                {
                    UpdateView();
                }
            }
        }

        public StyleProperty<int> IconWidth
        {
            get => _iconWidth;
            set
            {
                if (SetAndNotify(ref _iconWidth, value))
                {
                    UpdateView();
                }
            }
        }

        public StyleProperty<int> IconHeight
        {
            get => _iconHeight;
            set
            {
                if (SetAndNotify(ref _iconHeight, value))
                {
                    UpdateView();
                }
            }
        }

        #endregion

        #region Direct Properties

        public ObservableCollection<MenuItemBase> Items => _popup.Items;

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            style?.Apply(this, (target, sheet, property, value) =>
            {
                switch (property)
                {

                    case nameof(IsPressed):
                        target.IsPressed = target.IsPressed.Override(value.ConvertTo<bool>());
                        break;

                    case nameof(PressedBackground):
                        target.PressedBackground = target.PressedBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;

                    case nameof(Mode):
                        target.Mode = target.Mode.Override(value.ConvertToEnum<MenuItemViewMode>());
                        break;

                    case nameof(Icon):
                        target.Icon = target.Icon.Override(sheet.FindRegion(value.RawValue));
                        break;

                    case nameof(IconMargin):
                        target.IconMargin = target.IconMargin.Override(value.ConvertTo<Thickness>());
                        break;

                    case nameof(IconWidth):
                        target.IconWidth = target.IconWidth.Override(value.ConvertTo<int>());
                        break;

                    case nameof(IconHeight):
                        target.IconHeight = target.IconHeight.Override(value.ConvertTo<int>());
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not MenuItemView source)
                return;

            IsPressed = source.IsPressed;
            PressedBackground = source.PressedBackground;
            Mode = source.Mode;
            Icon = source.Icon;
            IconMargin = source.IconMargin;
            IconWidth = source.IconWidth;
            IconHeight = source.IconHeight;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new MenuItemView();
        }

        #endregion

        public override IBrush? GetCurrentBackground()
        {
            var result = base.GetCurrentBackground();

            if (IsEnabled)
            {
                if (IsPressed && PressedBackground.Value != null) result = PressedBackground.Value;
                else if (UseOverBackground && HoveredBackground.Value != null) result = HoveredBackground.Value;
            }
            else if (DisabledBackground.Value != null)
            {
                result = DisabledBackground.Value;
            }

            return result;
        }

        private void ConfigureContent()
        {
            _content.Width = new StyleProperty<int?>(null, isUserSet: false);
            _content.Height = new StyleProperty<int?>(null, isUserSet: false);
            _content.HorizontalAlignment = new StyleProperty<HorizontalAlignment>(Visuals.HorizontalAlignment.Stretch);
            _content.DropDown = _popup;
            _content.Background = null;
            _content.PressedBackground = PressedBackground;
            _content.HoveredBackground = HoveredBackground;
            _content.FocusedBackground = null;
        }

        private void UpdateLayout()
        {
            Children.Clear();

            _layout.ColumnsProportions.Clear();
            _layout.RowsProportions.Clear();

            if (_mode.Value == MenuItemViewMode.Root)
            {
                Children.Add(_content);
            }
            else if (_mode.Value == MenuItemViewMode.Underlying)
            {
                _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
                _layout.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
                _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
                _layout.ColumnSpacing = 5;

                Grid.SetColumn(_iconImage, 0);
                Grid.SetColumn(_content, 1);
                Grid.SetColumn(_indicator, 2);

                Children.Add(_iconImage);
                Children.Add(_content);
                Children.Add(_indicator);
            }
        }

        private void UpdateView()
        {
            _iconImage.Width = new StyleProperty<int?>(_iconWidth);
            _iconImage.Height = new StyleProperty<int?>(_iconHeight);
            _iconImage.Visual = new StyleProperty<ITextureRegion?>(_icon.Value);
            _iconImage.HoveredVisual = new StyleProperty<ITextureRegion?>(_icon.Value);

            _content.Content = new StyleProperty<object?>(_header.Value);
            _content.VerticalAlignment = new StyleProperty<VerticalAlignment>(Visuals.VerticalAlignment.Center);
            _content.Width = null;
            _content.Height = null;

            if (_content.ContentPresenter != null)
            {
                //_content.Presenter.Margin = new StyleProperty<Thickness>(new Thickness(5, 0, 5, 0));
                _content.ContentPresenter.VerticalAlignment = new StyleProperty<VerticalAlignment>(Visuals.VerticalAlignment.Center);
            }
        }
    }
}
