using Microsoft.Xna.Framework;
using Sachssoft.Sasofly.Resources;
using Sachssoft.Sasogine.Graphics.Colors;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Visuals;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors;

public class ColorValueEditor : InspectorValueEditorBase
{
    private Action? _refreshDisplay;

    protected internal override bool AllowNullable
    {
        get => Nullable.GetUnderlyingType(Property.ValueType) == typeof(Color);
    }

    protected internal override object? CreateInstance()
    {
        return new Color();
    }

    protected internal override Widget BuildControl()
    {
        if (Property.ValueType != typeof(Color) && Nullable.GetUnderlyingType(Property.ValueType) != typeof(Color))
            throw new InvalidOperationException("Invalid Type.");

        var grid = new Grid { ColumnSpacing = 1 };

        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));

        var labelBox = new Label { Width = 40, Height = 20, Background = Color.Black.ToBrush() };
        Grid.SetColumn(labelBox, 0);
        grid.Widgets.Add(labelBox);

        var textBox = new TextBox { IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch };
        Grid.SetColumn(textBox, 1);
        grid.Widgets.Add(textBox);

        var clearButton = new Button
        {
            Content = new Label
            {
                Text = "X",
                HorizontalAlignment = HorizontalAlignment.Center,
                TextHorizontalAlignment = HorizontalAlignment.Center
            },
            Width = 20
        };
        Grid.SetColumn(clearButton, 2);
        grid.Widgets.Add(clearButton);

        var browseButton = new Button
        {
            Content = new Label
            {
                Text = "...",
                HorizontalAlignment = HorizontalAlignment.Center,
                TextHorizontalAlignment = HorizontalAlignment.Center
            },
            Width = 20
        };
        Grid.SetColumn(browseButton, 3);
        grid.Widgets.Add(browseButton);

        void RefreshDisplay()
        {
            var value = GetValue();
            var noneText = new LocalizedValue<string>("None").GetValue(Inspector.Culture);
            UpdateDisplay(labelBox, textBox, noneText, value);
        }

        _refreshDisplay = RefreshDisplay; // Speichern, damit OnValueChangedBySource darauf zugreifen kann
        RefreshDisplay();

        if (!Property.IsReadOnly)
        {
            clearButton.Click += (s, e) =>
            {
                Source.ClearValue(Property);
                RefreshDisplay();
            };

            browseButton.Click += (s, e) =>
            {
                var dlg = new ColorPickerDialog
                {
                    Color = (Color)(GetValue() ?? Color.Black)
                };

                //dlg.Confirmed = dlgObj =>
                //{
                //    var clrDlg = (ColorPickerDialog)dlgObj;
                //    SetValue(clrDlg.Color);
                //    RefreshDisplay();
                //};

                //dlg.ShowModal(Inspector.Desktop);
            };
        }
        else
        {
            textBox.IsEnabled = false;
            clearButton.IsEnabled = false;
            browseButton.IsEnabled = false;
        }

        return grid;
    }

    private void UpdateDisplay(Label label, TextBox textBox, string? noneText, object? value)
    {
        if (value == null)
        {
            // Falls der Wert null ist, egal ob Nullable<Color> oder normal, zeigen wir <None>
            textBox.Text = $"<{noneText}>";
            label.Background = Color.Transparent.ToBrush(); // statt null
        }
        else
        {
            // value ist entweder Color oder Color?, casten wir sicher
            Color color = value is Color c ? c : ((Color?)value).GetValueOrDefault(Color.Black);
            textBox.Text = color.ToHex();
            label.Background = color.ToBrush();
        }
    }

    protected override void OnValueChangedBySource()
    {
        _refreshDisplay?.Invoke();
    }
}
