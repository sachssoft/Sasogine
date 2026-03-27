using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Behaviors;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class InspectorDialog : Dialog
    {
        private readonly Inspector _inspector = new();
        private readonly ListView _groupListView = new();

        public InspectorDialog(NotifyingObject source, PropertyGroup[] groups)
        {
            Width = 600;
            Height = 400;
            Padding = new Thickness(5);

            //ButtonCancelText = "Close";
            //ButtonConfirm.IsVisible = false;

            Title = "Settings";
            //ButtonConfirmText = "OK";

            var grid = new Grid();
            grid.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            grid.VerticalAlignment = Visuals.VerticalAlignment.Stretch;
            grid.ColumnSpacing = 5;
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Pixel, 150));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));

            foreach (var group in groups)
                _groupListView.Items.Add(CreateListItem(group));
            _groupListView.SelectionChanged += List_SelectionChanged;
            _groupListView.SelectedIndices = new int[] { 0 };
            _groupListView.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            _groupListView.VerticalAlignment = Visuals.VerticalAlignment.Stretch;
            Grid.SetColumn(_groupListView, 0);
            grid.Widgets.Add(_groupListView);

            _inspector.Source = source;
            _inspector.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            _inspector.VerticalAlignment = Visuals.VerticalAlignment.Stretch;

            Grid.SetColumn(_inspector, 1);
            grid.Widgets.Add(_inspector);

            Content = grid;
        }

        private void List_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            //var list = (ListView)sender!;
            //var group = (PropertyGroup)list.Items.GetPresenterByIndex(list.SelectedIndices.Value[0]).Tag!;
            //_inspector.VisibilityGroups = [group];
        }

        private Widget CreateListItem(PropertyGroup group)
        {
            var label = new Label()
            {
                Text = group.Title.GetValue(UIEnvironment.Culture),
                HorizontalAlignment = Visuals.HorizontalAlignment.Stretch
            };
            label.Tag = group;
            return label;
        }
    }
}
