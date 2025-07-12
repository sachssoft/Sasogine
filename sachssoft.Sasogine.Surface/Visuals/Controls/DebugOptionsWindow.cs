using System;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

public partial class DebugOptionsWindow
{
    public bool ShowDebugInfo { get; set; }

    public DebugOptionsWindow()
    {
        Title = "Surface Debug Options";

        BuildUI();

        _checkBoxWidgetFrames.IsChecked = UIEnvironment.DrawWidgetsFrames;
        _checkBoxWidgetFrames.IsCheckedChanged += (s, a) =>
        {
            UIEnvironment.DrawWidgetsFrames = _checkBoxWidgetFrames.IsChecked;
        };

        _checkBoxKeyboardFocusedWidgetFrame.IsChecked = UIEnvironment.DrawKeyboardFocusedWidgetFrame;
        _checkBoxKeyboardFocusedWidgetFrame.IsCheckedChanged += (s, a) =>
        {
            UIEnvironment.DrawKeyboardFocusedWidgetFrame = _checkBoxKeyboardFocusedWidgetFrame.IsChecked;
        };

        _checkBoxMouseInsideWidgetFrame.IsChecked = UIEnvironment.DrawMouseHoveredWidgetFrame;
        _checkBoxMouseInsideWidgetFrame.IsCheckedChanged += (s, a) =>
        {
            UIEnvironment.DrawMouseHoveredWidgetFrame = _checkBoxMouseInsideWidgetFrame.IsChecked;
        };

        _checkBoxGlyphFrames.IsChecked = UIEnvironment.DrawTextGlyphsFrames;
        _checkBoxGlyphFrames.IsCheckedChanged += (s, a) =>
        {
            UIEnvironment.DrawTextGlyphsFrames = _checkBoxGlyphFrames.IsChecked;
        };

        _checkBoxDisableClipping.IsChecked = UIEnvironment.DisableClipping;
        _checkBoxDisableClipping.IsCheckedChanged += (s, a) =>
        {
            UIEnvironment.DisableClipping = _checkBoxDisableClipping.IsChecked;
        };

        _checkBoxSmoothText.IsChecked = UIEnvironment.SmoothText;
        _checkBoxSmoothText.IsCheckedChanged += (s, a) =>
        {
            UIEnvironment.SmoothText = _checkBoxSmoothText.IsChecked;
        };
    }

    public void AddOption(string text, Action onEnabled, Action onDisabled)
    {
        var optionsCheckBox = new CheckButton
        {
            IsEnabled = true,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            IsVisible = true,
            Content = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Text = text
            },
        };
        Grid.SetRow(optionsCheckBox, Children.Count);

        optionsCheckBox.IsCheckedChanged += (s, a) =>
        {
            if (optionsCheckBox.IsChecked)
            {
                onEnabled();
            }
            else
            {
                onDisabled();
            }
        };

        Root.Widgets.Add(optionsCheckBox);
    }
}