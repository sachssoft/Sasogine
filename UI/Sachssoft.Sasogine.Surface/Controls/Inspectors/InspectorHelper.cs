using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    internal static class InspectorHelper
    {

        public static Widget CreateContainedTextBox(out TextBox textBox)
        {
            textBox = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Wrap = true,
                Mode = TextBoxMode.Multiline
            };
            var textBoxContainer = new ScrollViewer();
            textBoxContainer.Content = textBox;
            textBoxContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
            textBoxContainer.CanScrollHorizontal = true;
            textBoxContainer.IsHorizontalScrollBarVisible = false;
            return textBoxContainer;
        }

    }
}
