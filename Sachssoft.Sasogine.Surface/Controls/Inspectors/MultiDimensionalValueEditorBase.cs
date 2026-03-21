using Sachssoft.Sasofly.Resources;
using Sachssoft.Sasogine.Surface.Visuals;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public abstract class NumericValueEditorBase : InspectorValueEditorBase
    {
        protected Label CreateLabel(string title)
        {
            return new Label()
            {
                Text = LocalizedValue<string>.Create(title).GetValue(Inspector.Culture),
                TextHorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        protected TextBox CreateTextBox(float initialValue, Action<TextBox> saveAction)
        {
            var textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            textBox.KeyboardFocusLost += (s, e) => TryParse(textBox.Text);
            textBox.KeyDown += (s, e) => TryParse(textBox.Text);

            return textBox;
        }

        protected NumberSpinner CreateSpinner(float initialValue, Action<NumberSpinner> saveAction)
        {
            var spinButton = new NumberSpinner
            {
                //Integer = false,
                //Nullable = false,
                //Value = initialValue,
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                //SmallIncrement = 0.01f,
                //LargeIncrement = 1f,
                //DecimalPlaces = 3
            };

            if (!Property.IsReadOnly)
            {
                //spinButton.ValueChanged += (s, e) => saveAction(spinButton);
                spinButton.KeyDown += (s, e) =>
                {
                    if (e.Data == Microsoft.Xna.Framework.Input.Keys.Enter)
                        saveAction(spinButton);
                };
                spinButton.KeyboardFocusLost += (s, e) => saveAction(spinButton);
            }
            else
            {
                spinButton.IsEnabled = false;
            }

            return spinButton;
        }

        protected void SaveValue<T>(NumberSpinner spinButton, Func<float, T, T> setFunc)
        {
            //try
            //{
            //    if (!spinButton.Value.HasValue) return;

            //    var result = spinButton.Value.Value;
            //    var old = (T)GetValue()!;
            //    SetValue(setFunc(result, old));
            //}
            //catch (Exception ex)
            //{
            //    // Alte Value wiederherstellen
            //    var value = (T)GetValue()!;
            //    spinButton.Value = spinButton == null ? 0 : 0; // optional: hier könnte man old.X/Y setzen
            //    Console.WriteLine($"Invalid _value: {ex}");
            //}
        }

        private void TryParse(string? text)
        {

        }
    }
}
