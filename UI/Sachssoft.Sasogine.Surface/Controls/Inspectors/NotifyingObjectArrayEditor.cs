using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasofly.Resources;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class NotifyingObjectArrayEditor : InspectorValueEditorBase
    {
        private readonly TextBox _textBox = new();
        private readonly VerticalStackPanel _containerAtBottom = new();

        protected internal override Widget BuildControl()
        {
            Check();

            var grid = new Grid();
            grid.ColumnSpacing = 1;
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));

            // ComboBox erstellen
            _textBox.IsReadOnly = true;
            _textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            Grid.SetColumn(_textBox, 0);
            grid.Widgets.Add(_textBox);

            var addButton = new Button();
            addButton.Content = new Label()
            {
                Text = "+",
                HorizontalAlignment = HorizontalAlignment.Center,
                TextHorizontalAlignment = HorizontalAlignment.Center
            };
            addButton.Width = 20;
            Grid.SetColumn(addButton, 1);
            grid.Widgets.Add(addButton);

            UpdateTextDisplay();

            return grid;
        }

        protected internal override Container? BuildContainerAtBottom()
        {
            _containerAtBottom.Spacing = 5;

            var items = (IEnumerable?)GetValue();
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item is NotifyingObject obj)
                        AddItem(obj);
                }
            }

            return _containerAtBottom;
        }

        private void AddItem(NotifyingObject obj)
        {
            var insp = new Inspector();
            insp.HorizontalAlignment = HorizontalAlignment.Stretch;
            insp.VerticalAlignment = VerticalAlignment.Stretch;
            insp.Source = obj;
            _containerAtBottom.Widgets.Add(insp);
        }

        private void UpdateTextDisplay()
        {
            var value = GetValue();
            var emptyText = new LocalizedValue<string>("Empty").GetValue(Inspector.Culture);
            var itemText = new LocalizedValue<string>("Item").GetValue(Inspector.Culture);
            var itemsText = new LocalizedValue<string>("Children").GetValue(Inspector.Culture);

            if (value == null || value is IEnumerable enumerable && !enumerable.Cast<object>().Any())
            {
                _textBox.Text = $"<{emptyText}>";
            }
            else
            {
                // Anzahl ermitteln
                int count = 0;
                if (value is IEnumerable e)
                {
                    foreach (var _ in e) count++;
                }

                // Pluralisierung
                string displayText = count == 1 ? $"<{count} {itemText}>" : $"<{count} {itemsText}>";
                _textBox.Text = displayText;
            }
        }

        private void Check()
        {
            if (!(Property.ValueType.IsArray ||
                    Property.ValueType.IsAssignableTo(typeof(IEnumerable))))
            {
                throw new InvalidOperationException(
                    "Property.ValueType must be array or IEnumerable.");
            }

            Type? elementType = null;

            // Array
            if (Property.ValueType.IsArray)
            {
                elementType = Property.ValueType.GetElementType();
            }
            // IEnumerable<T>
            else if (Property.ValueType.IsGenericType &&
                     typeof(IEnumerable).IsAssignableFrom(Property.ValueType))
            {
                elementType = Property.ValueType.GetGenericArguments()[0];
            }

            // Falls kein Elementtyp ermittelbar
            if (elementType == null)
            {
                throw new InvalidOperationException(
                    $"Cannot determine element type for {Property.ValueType}.");
            }

            // Item-Type prüfen
            if (!typeof(NotifyingObject).IsAssignableFrom(elementType))
            {
                throw new InvalidOperationException(
                    $"Element type {elementType} must derive from NotifyingObject.");
            }
        }
    }
}
