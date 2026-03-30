using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasofly.Resources;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class AssociationValueEditor : InspectorValueEditorBase
    {
        private readonly Dictionary<Type, Func<IIdentifiable, IReference>> _factoryRegistry = new();
        private int _assetIdCounter = 0;

        public AssociationValueEditor()
        {
            // Register default factories
            //RegisterType((id) => new Reference<DataAsset, IAssetSourceCollection>(id.ID));
            //RegisterType((id) => new Reference<MusicAsset>(id.ID));
            //RegisterType((id) => new Reference<SoundAsset>(id.ID));
            //RegisterType((id) => new Reference<Texture2DAsset>(id.ID));
        }

        public IEnumerable<IReference>? Associations { get; set; }

        public INotifyingEnumerable<IIdentifiable>? ItemsSource { get; set; }

        public Func<IIdentifiable, string?>? ItemLabelCallback { get; set; }

        public void RegisterType<T>(Func<IIdentifiable, T> factory) where T : IReference
        {
            _factoryRegistry[typeof(T)] = (id) => factory.Invoke(id);
        }

        protected internal override Widget BuildControl()
        {
            //if (!(Property.ValueType.IsAssignableTo(typeof(IReference)) ||
            //      Property.ValueType.IsAssignableTo(typeof(IGuidAssociation))))
            //{
            //    throw new InvalidOperationException(
            //        "Property.ValueType must implement IAssociation or IGuidAssociation.");
            //}

            var grid = new Grid
            {
                ColumnSpacing = 1
            };
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));

            // TextBox
            var textBoxContainer = InspectorHelper.CreateContainedTextBox(out var textBox);
            textBox.IsReadOnly = true;
            Grid.SetColumn(textBoxContainer, 0);
            grid.Widgets.Add(textBoxContainer);

            // Clear Button
            var clearButton = new Button
            {
                Width = 20,
                Content = new Label
                {
                    Text = "X",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextHorizontalAlignment = HorizontalAlignment.Center
                }
            };
            Grid.SetColumn(clearButton, 1);
            grid.Widgets.Add(clearButton);

            // Browse Button
            var browseButton = new Button
            {
                Width = 20,
                Content = new Label
                {
                    Text = "...",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextHorizontalAlignment = HorizontalAlignment.Center
                }
            };
            Grid.SetColumn(browseButton, 2);
            grid.Widgets.Add(browseButton);

            // Initialize TextBox
            var reference = GetValue() as IReference;
            var noneText = new LocalizedValue<string>("None").GetValue(Inspector.Culture);
            IIdentifiable? item = null;

            if (reference != null)
            {
                reference.ItemsSource = ItemsSource;
                item = reference.Result;
                textBox.Text = GetDisplayText(item);
            }

            if (!Property.IsReadOnly)
            {
                clearButton.Click += (s, e) =>
                {
                    Source.ClearValue(Property);
                    textBox.Text = $"<{noneText}>";
                };

                browseButton.Click += (s, e) =>
                {
                    //if (ItemsSource == null)
                    //    throw new NullReferenceException("No ItemsSource Source");

                    //if (CollectionSource == null || !CollectionSource.Any()) return;

                    var dlg = new AssociationSelectionDialog(this, ItemsSource, ItemLabelCallback);
                    //dlg.Confirmed = (dlgInstance) =>
                    //{
                    //    if (dlgInstance is not AssociationSelectionDialog asDlg) return;

                    //    var item = asDlg.CollectionSource?.ElementAtOrDefault(asDlg.SelectedSourceIndex);
                    //    if (item != null)
                    //    {
                    //        // ID sicherstellen
                    //        if (string.IsNullOrEmpty(item.ID))
                    //            item.ID = $"asset{_assetIdCounter++}";

                    //        textBox.Text = GetDisplayText(item);
                    //        SetValue(CreateAssociation(Property.ValueType, item));
                    //    }
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

        internal IReference CreateAssociation(Type type, IIdentifiable id)
        {
            if (_factoryRegistry.TryGetValue(type, out var factory))
            {
                return factory.Invoke(id);
            }

            throw new NotSupportedException($"No factory registered for type '{type.FullName}'.");
        }

        internal string GetDisplayText(IIdentifiable? item)
        {
            if (item == null || string.IsNullOrEmpty(item.ID))
            {
                var unknownText = new LocalizedValue<string>("Unknown").GetValue(Inspector.Culture);
                return $"<{unknownText}>";
            }

            var text = ItemLabelCallback?.Invoke(item) ?? item.ID;
            return string.IsNullOrEmpty(text) ? $"<{item.GetType().Name}>" : text;
        }
    }
}
