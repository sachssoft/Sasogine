using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Surface.Controls.Primitives
{
    public class MenuItemPresenter : ButtonBase
    {
        private StyleProperty<ITextureRegion?> _icon = new StyleProperty<ITextureRegion?>(null, isUserSet: false);
        private StyleProperty<object?> _header = new StyleProperty<object?>(null, isUserSet: false);
        private StyleProperty<Thickness> _iconMargin = new StyleProperty<Thickness>(new Thickness(2), isUserSet: false);
        private StyleProperty<int> _iconWidth = new StyleProperty<int>(24, isUserSet: false);
        private StyleProperty<int> _iconHeight = new StyleProperty<int>(24, isUserSet: false);
        private StyleProperty<string?> _shortcutText = new StyleProperty<string?>(null, isUserSet: false);

        private readonly GridLayout _layout = new GridLayout();
        private readonly Image _iconImage = new Image();
        private readonly DropdownButton _content = new DropdownButton();
        private readonly Label _shortcutLabel = new Label();
        private readonly MenuPopup _popup = new MenuPopup();

        public MenuItemPresenter()
        {
            LayoutContainer  = _layout;

            _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            _layout.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            _layout.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            _layout.ColumnSpacing = 5;

            Grid.SetColumn(_iconImage, 0);
            Grid.SetColumn(_content, 1);
            Grid.SetColumn(_shortcutLabel, 2);

            _content.VerticalAlignment = _content.VerticalAlignment.Override(Visuals.VerticalAlignment.Center);
            _shortcutLabel.VerticalAlignment = _shortcutLabel.VerticalAlignment.Override(Visuals.VerticalAlignment.Center);

            Children.Add(_iconImage);
            Children.Add(_content);
            Children.Add(_shortcutLabel);

            //Background = Background.Override(Color.Gray.ToBrush());
            Padding = Padding.Override(new Thickness(5, 2, 5, 2));    
            HoveredBackground = HoveredBackground.Override(Color.Blue.ToBrush());
            HorizontalAlignment = HorizontalAlignment.Override(Visuals.HorizontalAlignment.Stretch);

            ConfigureContent();
        }


        #region Style Properties

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

        public StyleProperty<string?> ShortcutText
        {
            get => _shortcutText;
            set
            {
                if (SetAndNotify(ref _shortcutText, value))
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

                    case nameof(ShortcutText):
                        target.ShortcutText = target.ShortcutText.Override(value.RawValue);
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not MenuItemPresenter source)
                return;

            Icon = source.Icon;
            IconMargin = source.IconMargin;
            IconWidth = source.IconWidth;
            IconHeight = source.IconHeight;
            ShortcutText = source.ShortcutText;
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new MenuItemPresenter();
        }

        #endregion

        internal protected override void OnTouchLeft()
        {
            base.OnTouchLeft();
            SetValueByUser(false);
        }

        protected override void InternalOnTouchUp()
        {
            SetValueByUser(false);
            _popup.Close();
        }

        protected override void InternalOnTouchDown()
        {
            SetValueByUser(true);
        }

        public override void OnKeyDown(Keys k)
        {
            base.OnKeyDown(k);

            if (!IsEnabled)
            {
                return;
            }

            if (k == Keys.Space)
            {
                // Emulate click
                //DoClick();
            }
        }

        private void DesktopTouchUp(object? sender, EventArgs args)
        {
            IsPressed = false;
        }

        private void ConfigureContent()
        {
            _content.Width = new StyleProperty<int?>(null, isUserSet: false);
            _content.Height = new StyleProperty<int?>(null, isUserSet: false);
            _content.DropDown = _popup;
            _content.Background = null;
            _content.PressedBackground = null;
            _content.HoveredBackground = null;
            _content.FocusedBackground = null;
        }

        private void UpdateView()
        {
            _iconImage.Width = new StyleProperty<int?>(_iconWidth);
            _iconImage.Height = new StyleProperty<int?>(_iconHeight);
            _iconImage.Visual = new StyleProperty<ITextureRegion?>(_icon.Value);
            _iconImage.HoveredVisual = new StyleProperty<ITextureRegion?>(_icon.Value);

            _content.Content = new StyleProperty<object?>(_header.Value);

            _shortcutLabel.Text = _shortcutText.Value;
        }
    }
}
