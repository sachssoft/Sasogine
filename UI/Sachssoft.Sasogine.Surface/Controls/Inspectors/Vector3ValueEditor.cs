using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Visuals;
using System;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class Vector3ValueEditor : NumericValueEditorBase
    {
        protected internal override bool AllowNullable
        {
            get => Nullable.GetUnderlyingType(Property.ValueType) == typeof(Vector3);
        }

        protected internal override object? CreateInstance()
        {
            return new Vector3();
        }

        protected internal override Widget BuildControl()
        {
            var grid = new Grid();
            grid.ColumnSpacing = 5;
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Part));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Part));
            grid.Background = ColorUtils.Parse("#505050").ToBrush();

            var vector = (Vector3)GetValue()!;

            var labelX = CreateLabel("X");
            var labelY = CreateLabel("Y");
            var labelZ = CreateLabel("Z");
            var spinX = CreateSpinner(vector.X, SaveXValue);
            var spinY = CreateSpinner(vector.Y, SaveYValue);
            var spinZ = CreateSpinner(vector.Z, SaveZValue);

            Grid.SetColumn(labelX, 0);
            Grid.SetRow(labelX, 0);
            Grid.SetColumn(spinX, 1);
            Grid.SetRow(spinX, 0);
            Grid.SetColumn(labelY, 0);
            Grid.SetRow(labelY, 1);
            Grid.SetColumn(spinY, 1);
            Grid.SetRow(spinY, 1);
            Grid.SetColumn(labelZ, 0);
            Grid.SetRow(labelZ, 2);
            Grid.SetColumn(spinZ, 1);
            Grid.SetRow(spinZ, 2);

            grid.Widgets.Add(labelX);
            grid.Widgets.Add(spinX);
            grid.Widgets.Add(labelY);
            grid.Widgets.Add(spinY);
            grid.Widgets.Add(labelZ);
            grid.Widgets.Add(spinZ);

            return grid;
        }

        private void SaveXValue(NumberSpinner spinButton)
        {
            SaveValue<Vector3>(spinButton, (v, oldV) => new Vector3(v, oldV.Y, oldV.Z));
        }

        private void SaveYValue(NumberSpinner spinButton)
        {
            SaveValue<Vector3>(spinButton, (v, oldV) => new Vector3(oldV.X, v, oldV.Z));
        }

        private void SaveZValue(NumberSpinner spinButton)
        {
            SaveValue<Vector3>(spinButton, (v, oldV) => new Vector3(oldV.X, oldV.Y, v));
        }
    }
}
