using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasofly.Resources;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    internal class AssociationSelectionDialog : Dialog
    {
        private readonly ListView _listView = new();
        private readonly AssociationValueEditor _editor;
        private readonly Func<IIdentifiable, string?>? _itemLabelFunc;

        public IEnumerable<IIdentifiable>? CollectionSource { get; private set; }

        public int SelectedSourceIndex { get; private set; }

        public AssociationSelectionDialog(AssociationValueEditor editor, IEnumerable<IIdentifiable>? collectionSource, Func<IIdentifiable, string?>? itemLabelFunc)
        {
            _editor = editor;

            Width = 400;
            Height = 400;
            Padding = new Thickness(5);
            Title = LocalizedValue<string>.Create("Select an asset")
                                          .GetValue(_editor.Inspector.Culture);

            _listView.HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
            _listView.VerticalAlignment = Visuals.VerticalAlignment.Stretch;
            _listView.Margin = new Thickness(5);
            Content = _listView;

            _itemLabelFunc = itemLabelFunc;

            if (collectionSource != null)
            {
                foreach (var entry in collectionSource)
                    AddItem(entry);
            }

            CollectionSource = collectionSource;
            SelectedSourceIndex = -1;

            //ButtonConfirmText = "Select";
            //ButtonCancelText = "Cancel";
        }

        private void AddItem(IIdentifiable id)
        {
            _listView.Items.Add(_editor.GetDisplayText(id));
        }

        protected override void OnConfirmed()
        {
            if (_listView.SelectedIndices.Value.Length == 0)
                SelectedSourceIndex = -1;

            SelectedSourceIndex = _listView.SelectedIndices.Value[0];
        }
    }
}
