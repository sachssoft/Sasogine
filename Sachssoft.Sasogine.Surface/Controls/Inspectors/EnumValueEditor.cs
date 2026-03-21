using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class EnumValueEditor : InspectorValueEditorBase
    {
        protected internal override Widget BuildControl()
        {
            // Enum-Typ bestimmen
            var enumType = Property.ValueType;
            if (!enumType.IsEnum)
                throw new InvalidOperationException("EnumValueEditor kann nur für Common verwendet werden.");

            var grid = new Grid();
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));

            // ComboBox erstellen
            var comboBox = new ComboBox();
            comboBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            var fields = Enum.GetNames(enumType);

            foreach (var field in fields)
                comboBox.Items.Add(CreateItem(field));

            // Aktuellen Wert auswählen
            var currentValue = GetValue();
            if (currentValue != null)
                comboBox.SelectedIndex = Array.IndexOf(Enum.GetNames(enumType), currentValue.ToString());

            if (!Property.IsReadOnly)
            {
                comboBox.SelectionChanged += (s, e) =>
                {
                    //var item = comboBox.Items.GetPresenterByIndex(comboBox.SelectedIndex);
                    //var selectedName = ((Label)item.Content!).Text;
                    //if (selectedName != null)
                    //{
                    //    var enumValue = Enum.Parse(enumType, selectedName);
                    //    SetValue(enumValue);
                    //}
                };
            }
            else
            {
                comboBox.IsEnabled = false;
            }

            Grid.SetColumn(comboBox, 0);
            grid.Widgets.Add(comboBox);

            return grid;
        }

        private Label CreateItem(string name)
        {
            return new Label()
            {
                Text = name
            };
        }
    }
}
