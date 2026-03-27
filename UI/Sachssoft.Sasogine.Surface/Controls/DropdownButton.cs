using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Controls.Primitives;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using System;
using System.Runtime.CompilerServices;

namespace Sachssoft.Sasogine.Surface.Controls
{
    public class DropdownButton : ButtonBase
    {
        private StyleProperty<int> _indicatorContentSpacing = new StyleProperty<int>(0, isUserSet: false);
        private StyleProperty<IndicatorPosition> _indicatorPosition = new StyleProperty<IndicatorPosition>(Primitives.IndicatorPosition.Right, isUserSet: false);
        private StyleProperty<bool> _hasSplit = new StyleProperty<bool>(false, isUserSet: false);

        private Point _dropDownPosition;
        private readonly StackPanelLayout _layout = new StackPanelLayout(Orientation.Horizontal);

        public DropdownButton()
        {
            _dropDownPosition = PopupPosition;
        }

        #region Style Properties

        public StyleProperty<IndicatorPosition> IndicatorPosition
        {
            get => _indicatorPosition;
            set
            {
                if (SetAndNotify(ref _indicatorPosition, value))
                    UpdateLayout();
            }
        }

        public StyleProperty<int> IndicatorContentSpacing
        {
            get => _indicatorContentSpacing;
            set
            {
                if (SetAndNotify(ref _indicatorContentSpacing, value))
                    UpdateLayoutProperties();
            }
        }

        public StyleProperty<bool> HasSplit
        {
            get => _hasSplit;
            set
            {
                if (SetAndNotify(ref _hasSplit, value))
                    UpdateLayoutProperties();
            }
        }

        #endregion

        #region Direct Properties

        public PopupBase DropDown
        {
            get => Popup ??= PopupBase.Create(new Widget());
            set => Popup = value ??= PopupBase.Create(new Widget());
        }

        public Point DropDownPosition
        {
            get => _dropDownPosition;
            set => SetAndNotify(ref _dropDownPosition, value);
        }

        protected override Point PopupPosition => ToGlobal(_dropDownPosition);

        public override Desktop Desktop
        {
            get => base.Desktop;
            internal set
            {
                if (Desktop != null)
                    Desktop.TouchUp -= DesktopTouchUp;

                base.Desktop = value;

                if (Desktop != null)
                    Desktop.TouchUp += DesktopTouchUp;
            }
        }

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);

            // Style für das Content-Label anwenden
            var contentLabelStyle = style?.FindStyle(typeof(Label), "content");
            if (ContentPresenter is Label label)
                label.ApplyFromStyle(contentLabelStyle);

            // Button-spezifische Properties
            style?.Apply<DropdownButton>(this, (target, sheet, property, value) =>
            {
                switch (property)
                {
                    case nameof(Background):
                        target.Background = target.Background.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(HoveredBackground):
                        target.HoveredBackground = target.HoveredBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                    case nameof(PressedBackground):
                        target.PressedBackground = target.PressedBackground.Override(new RegionBrush(sheet.FindRegion(value.RawValue)));
                        break;
                }
            });
        }

        public override void ApplyFrom(ElementBase other)
        {
            base.ApplyFrom(other);

            if (other is not DropdownButton source)
                return;

            IndicatorPosition = source.IndicatorPosition;
            IndicatorContentSpacing = source.IndicatorContentSpacing;
            HasSplit = source.HasSplit;
            DropDownPosition = source.DropDownPosition;

            if (source.DropDown != null)
                DropDown = source.DropDown.Clone<PopupBase>();

            UpdateLayoutProperties();
        }

        protected override ElementBase CreateCloneInstance()
        {
            return new DropdownButton();
        }

        #endregion

        #region Input Logic

        protected override void InternalOnTouchDown()
        {
            if (!IsEnabled)
                return;

            // Toggle-Zustand
            SetValueByUser(!IsPressed);

            if (IsPressed)
            {
                OpenPopup();
            }
            else
            {
                ClosePopup();
            }
        }

        protected override void InternalOnTouchUp()
        {
            // kein direktes Verhalten
        }

        protected override void OnPressedChanged(EventArgs e)
        {
            // Kein Popup-Schalten hier – reine visuelle Aktualisierung
            base.OnPressedChanged(e);
        }

        private void DesktopTouchUp(object? sender, EventArgs args)
        {
            //// Klick außerhalb schließt Popup
            //if (Popup != null && Popup.IsOpen && (IsMouseInside || IsTouchInside))
            //{
            //    ClosePopup();
            //    SetValueByUser(false);
            //}
        }

        internal protected override void OnTouchLeft()
        {
            base.OnTouchLeft();
            // Nur schließen, wenn Popup nicht offen
            if (Popup == null || !Popup.IsOpen)
                SetValueByUser(false);
        }

        protected override void OnPropertyChanging([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanging(propertyName);

            switch (propertyName)
            {
                case nameof(Popup):
                    if (Popup != null)
                    {
                        Popup.Opened -= OnPopupOpened;
                        Popup.Closed -= OnPopupClosed;
                    }
                    break;
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            switch (propertyName)
            {
                case nameof(Popup):
                    if (Popup != null)
                    {
                        Popup.Opened += OnPopupOpened;
                        Popup.Closed += OnPopupClosed;
                    }
                    break;
            }
        }

        #endregion

        #region Popup Logic

        private void OpenPopup()
        {
            if (Popup == null)
                return;

            //var position = ToGlobal(new Point(0, Bounds.Height));
            Popup.Open(this, PopupPosition);
        }

        private void ClosePopup()
        {
            Popup?.Close();
        }

        private void OnPopupOpened(object? sender, EventArgs e)
        {
            // Nur sync – kein neues SetValueByUser!
            if (!IsPressed)
                IsPressed = true;
        }

        private void OnPopupClosed(object? sender, EventArgs e)
        {
            // Nur sync
            if (IsPressed)
                IsPressed = false;
        }

        #endregion

        #region Layout

        protected override void InternalArrange()
        {
            base.InternalArrange();

            if (Popup?.Content != null)
            {
                Popup.Content.MinWidth = int.Max(
                    Popup.Content.Width.Value.GetValueOrDefault(),
                    Bounds.Width
                );

                _dropDownPosition = new Point(0, Bounds.Height);
            }
        }

        private void UpdateLayoutProperties()
        {
            _layout.Spacing = _indicatorContentSpacing.Value;
        }

        #endregion
    }
}
