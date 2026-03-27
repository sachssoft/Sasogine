using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Visuals;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class Vector2ValueEditor : NumericValueEditorBase
    {
        protected internal override bool AllowNullable
        {
            get => Nullable.GetUnderlyingType(Property.ValueType) == typeof(Vector2);
        }

        protected internal override object? CreateInstance()
        {
            return new Vector2();
        }

        protected internal override Widget BuildControl()
        {
            var grid = new Grid();
            grid.ColumnSpacing = 5;
            grid.RowSpacing = 5;
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Part));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.Background = ColorUtils.Parse("#505050").ToBrush();

            var vector = (Vector2)GetValue()!;

            var labelX = CreateLabel("X");
            var labelY = CreateLabel("Y");
            var valueX = CreateSpinner(vector.X, SaveXValue);
            var valueY = CreateSpinner(vector.Y, SaveYValue);

            Grid.SetColumn(labelX, 0);
            Grid.SetRow(labelX, 0);
            Grid.SetColumn(valueX, 1);
            Grid.SetRow(valueX, 0);
            Grid.SetColumn(labelY, 0);
            Grid.SetRow(labelY, 1);
            Grid.SetColumn(valueY, 1);
            Grid.SetRow(valueY, 1);

            grid.Widgets.Add(labelX);
            grid.Widgets.Add(valueX);
            grid.Widgets.Add(labelY);
            grid.Widgets.Add(valueY);

            return grid;
        }

        private void SaveXValue(NumberSpinner spinButton)
        {
            SaveValue<Vector2>(spinButton, (v, oldV) => new Vector2(v, oldV.Y));
        }

        private void SaveYValue(NumberSpinner spinButton)
        {
            SaveValue<Vector2>(spinButton, (v, oldV) => new Vector2(oldV.X, v));
        }

    }
}
