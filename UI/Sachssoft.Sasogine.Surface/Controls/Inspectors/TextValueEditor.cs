using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class TextValueEditor : InspectorValueEditorBase
    {
        protected internal override Widget BuildControl()
        {
            var textBox = new TextBox();
            textBox.Text = Convert.ToString(GetValue());

            if (!Property.IsReadOnly)
            {
                if (Property.ValueType == typeof(string))
                {
                    textBox.KeyDown += (s, e) =>
                    {
                        if (e.Data == Microsoft.Xna.Framework.Input.Keys.Enter)
                        {
                            SetValue(textBox.Text);
                        }
                    };
                    textBox.KeyboardFocusLost += (s, e) =>
                    {
                        SetValue(textBox.Text);
                    };
                }
            }
            else
            {
                textBox.IsEnabled = false;
            }


            return textBox;
        }
    }
}
