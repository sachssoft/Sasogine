using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class BooleanValueEditor : InspectorValueEditorBase
    {
        private readonly ToggleButton _onButton = new ToggleButton();
        private readonly ToggleButton _offButton = new ToggleButton();
        private bool _toggleUsed = false;

        protected internal override bool AllowNullable
        {
            get => Nullable.GetUnderlyingType(Property.ValueType) == typeof(bool);
        }

        protected internal override object? CreateInstance()
        {
            return new bool();
        }

        protected internal override Widget BuildControl()
        {
            var panel = new HorizontalStackPanel();

            _onButton.Content = new Label()
            {
                Text = "1",
                TextHorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _onButton.Width = 30;
            _onButton.Height = 20;
            _onButton.IsCheckedChanged += (s, e) =>
            {
                if (_onButton.IsChecked)
                {
                    _offButton.IsChecked = false;
                    SetValue(true);
                }
            };
            panel.Widgets.Add(_onButton);

            _offButton.Content = new Label()
            {
                Text = "0",
                TextHorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _offButton.Width = 30;
            _offButton.Height = 20;
            _offButton.IsCheckedChanged += (s, e) =>
            {
                if (_offButton.IsChecked)
                {
                    _onButton.IsChecked = false;
                    SetValue(false);
                }
            };
            panel.Widgets.Add(_offButton);

            UpdateToggles();

            return panel;
        }

        private void UpdateToggles()
        {
            var val = GetEffectiveValue() ?? false;

            _onButton.IsChecked = val;
            _offButton.IsChecked = !val;
        }

        private bool? GetEffectiveValue()
        {
            if (AllowNullable)
            {
                return (bool?)GetValue();
            }
            else
            {
                return (bool)GetValue()!;
            }
        }

        protected override void OnValueChangedBySource()
        {
            UpdateToggles();
        }
    }
}
