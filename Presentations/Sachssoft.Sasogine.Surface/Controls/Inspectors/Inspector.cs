using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasofly.Inspection.Descriptors;
using Sachssoft.Sasogine.Surface.Basic;
using Sachssoft.Sasogine.Surface.Layouts;
using Sachssoft.Sasogine.Surface.Styles;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public class Inspector : Widget
    {
        private StyleProperty<CultureInfo?> _culture = new StyleProperty<CultureInfo?>(UIEnvironment.Culture, isUserSet: false);
        private StyleProperty<bool> _isAdvancedPropertyExpanded = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<bool> _isReadOnlyPropertiesVisible = new StyleProperty<bool>(false, isUserSet: false);
        private StyleProperty<Orientation> _orientation = new StyleProperty<Orientation>(Orientation.Horizontal, isUserSet: false);
        private StyleProperty<int> _labelWidth = new StyleProperty<int>(150, isUserSet: false);
        private StyleProperty<int> _itemHeight = new StyleProperty<int>(25, isUserSet: false);

        private NotifyingObject? _source;
        private IComparer<PropertyCategory>? _categoryComparer;
        private InspectorValueEditorRegistry _editors;
        private IInspectorEditorPreparer? _editorPreparer;
        private PropertyGroup[]? _visibilityGroups;
        private ITemplateFactory<Widget>? _groupTemplate;
        private readonly GridLayout _gridLayout = new();
        private readonly ScrollViewer viewer = new();
        private readonly VerticalStackPanel _panel = new();
        private readonly List<(InspectorValueEditorBase Editor, Widget EditorControl)> _editorListCache = new();

        public Inspector()
        {
            _editors = new InspectorValueEditorRegistry(this);

            LayoutContainer  = _gridLayout;
            Background = new StyleProperty<IBrush?>(DefaultStyle.ContainerBrush);

            _panel.Spacing = 20;
            viewer.IsHorizontalScrollBarVisible = false;
            viewer.CanScrollHorizontal = false;
            viewer.Content = _panel;
            viewer.ContentPresenter.Margin = new Thickness(0, 0, 10, 0);

            Children.Add(viewer);
        }

        #region Style Properties

        public StyleProperty<CultureInfo?> Culture
        {
            get => _culture;
            set
            {
                if (SetAndNotify(ref _culture, value))
                {
                    UpdateGrid();
                }
            }
        }

        public StyleProperty<bool> IsAdvancedPropertiesExpanded
        {
            get => _isAdvancedPropertyExpanded;
            set
            {
                if (SetAndNotify(ref _isAdvancedPropertyExpanded, value))
                {
                    UpdateGrid();
                }
            }
        }

        public StyleProperty<bool> IsReadOnlyPropertiesVisible
        {
            get => _isReadOnlyPropertiesVisible;
            set
            {
                if (SetAndNotify(ref _isReadOnlyPropertiesVisible, value))
                {
                    UpdateGrid();
                }
            }
        }

        public Orientation Orientation
        {
            get => _orientation;
            set
            {
                if (SetAndNotify(ref _orientation, value))
                {
                    UpdateGrid();
                }
            }
        }

        public int LabelWidth
        {
            get => _labelWidth;
            set
            {
                if (SetAndNotify(ref _labelWidth, value))
                {
                    if (_orientation.Value == Orientation.Horizontal)
                        UpdateGrid();
                }
            }
        }

        public int ItemHeight
        {
            get => _itemHeight;
            set
            {
                if (SetAndNotify(ref _itemHeight, value))
                {
                    UpdateGrid();
                }
            }
        }

        public ITemplateFactory<Widget>? GroupTemplate
        {
            get => _groupTemplate;
            set
            {
                if (SetAndNotify(ref _groupTemplate, value))
                {
                    UpdateGrid();
                }
            }
        }

        #endregion

        #region Direct Properties

        public InspectorValueEditorRegistry Editors => _editors;

        public NotifyingObject? Source
        {
            get => _source;
            set
            {
                if (SetAndNotify(ref _source, value))
                {
                    UpdateGrid();
                }
            }
        }

        public PropertyGroup[]? VisibilityGroups
        {
            get => _visibilityGroups;
            set
            {
                if (SetAndNotify(ref _visibilityGroups, value))
                {
                    UpdateGrid();
                }
            }
        }

        public IInspectorEditorPreparer? EditorPreparer
        {
            get => _editorPreparer;
            set
            {
                if (SetAndNotify(ref _editorPreparer, value))
                {
                    UpdateGrid();
                }
            }
        }

        #endregion

        #region Style

        public override void ApplyFromStyle(Style? style)
        {
            base.ApplyFromStyle(style);
        }

        #endregion

        protected override void InternalArrange()
        {
            base.InternalArrange();

            foreach (var editorItem in _editorListCache)
            {
                //Dynamische Breite basierend auf ScrollViewer
                int availableWidth = _orientation.Value == Orientation.Horizontal
                    ? int.Max(0, viewer.Bounds.Width - _labelWidth - 20 * 2) // 20px Puffer
                    : viewer.Bounds.Width - 20 * 2; // vertikal
                editorItem.EditorControl.Width = availableWidth;
            }
        }

        private void UpdateGrid()
        {
            if (!IsLoaded)
                return;

            _panel.Widgets.Clear();

            foreach (var editorCache in _editorListCache)
                editorCache.Editor.Dispose();
            _editorListCache.Clear();

            if (_source == null) return;

            var properties = PropertyRegistry.GetProperties(_source.GetType())
                .Where(p =>
                {
                    if (_visibilityGroups != null && _visibilityGroups.Length > 0)
                    {
                        var metadata = p.GetMetadata(_source.GetType());

                        // Schnittmenge prüfen
                        return metadata.Groups != null &&
                               metadata.Groups.Any(g => _visibilityGroups.Contains(g));
                    }
                    return true;
                });

            var categories = properties.GroupBy(p => p.GetMetadata(_source.GetType()).Category)
                                       .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var category in categories.OrderByDescending(x => x.Key.Order))
            {
                var container = CreateCategoriedPropertyList(category.Key, category.Value, out var propertyCount);
                if (propertyCount > 0)
                    _panel.Widgets.Add(container);
            }
        }

        private Container CreateCategoriedPropertyList(PropertyCategory category, IEnumerable<IProperty> properties, out int count)
        {
            var panel = new VerticalStackPanel();
            panel.Spacing = 5;

            var groupTemplate = _groupTemplate?.Create();
            IGroup group;

            if (groupTemplate is null)
            {
                group = new Foldout();
            }
            else if (groupTemplate is IGroup)
            {
                group = (IGroup)groupTemplate;
            }
            else
            {
                throw new InvalidOperationException("Invalid group template");
            }

            var header = new Label
            {
                Text = category.Title.GetValue(_culture.Value ?? UIEnvironment.Culture)
            };
            header.ApplyFromStyle(Style?.FindStyle(typeof(Label), "header"));

            group.Header = header;
            group.Content = CreatePropertyList(properties, out count);

            panel.Widgets.Add((Widget)group);
            return panel;
        }

        private Container CreatePropertyList(IEnumerable<IProperty> properties, out int count)
        {
            count = 0;
            var panel = new VerticalStackPanel
            {
                Spacing = 5
            };


            ITextureRegion? clearIcon = Stylesheet.Current.FindRegion("inspector-clear-button");

            foreach (var property in properties)
            {
                var metadata = property.GetMetadata(_source.GetType());

                if (property.IsReadOnly && !_isReadOnlyPropertiesVisible ||
                    metadata.Visibility == PropertyVisibility.Hidden ||
                    metadata.Visibility == PropertyVisibility.Collapsed && !_isAdvancedPropertyExpanded)
                    continue;

                var attributes = metadata.Descriptors;
                if (!_editors.TryCreateEditor(property.ValueType, out var editor, attributes))
                    continue;

                var itemContainer = new VerticalStackPanel
                {
                    Spacing = 5
                };

                var itemPanel = new Grid();
                itemPanel.ColumnSpacing = 2;
                itemPanel.RowSpacing = 2;
                itemContainer.Widgets.Add(itemPanel);

                if (_orientation.Value == Orientation.Horizontal)
                {
                    itemPanel.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
                    itemPanel.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
                    itemPanel.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
                }
                else
                {
                    itemPanel.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
                    itemPanel.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
                    itemPanel.RowsProportions.Add(new Proportion(ProportionType.Fill));
                }

                editor ??= _editors.TryCreateEditor(property.ValueType, out var temp, null) ? temp : null;

                if (editor != null)
                {
                    editor.Inspector = this;
                    editor.Source = _source;
                    editor.Property = property;
                    editor.Initialize();
                    EditorPreparer?.Prepare(editor);

                    var displayAttr = metadata.Descriptors.OfType<PropertyDisplayDescriptor>();
                    var labelText = displayAttr.Any()
                        ? displayAttr.First().Display.GetValue(_culture)
                        : metadata.Title.GetValue(_culture);

                    var labelPanel = new HorizontalStackPanel();
                    labelPanel.Spacing = 5;

                    if (editor.AllowNullable)
                    {
                        var nullableCheckButton = new CheckBox()
                        {
                        };

                        nullableCheckButton.IsCheckedChanged += (s, e) =>
                        {
                            if (nullableCheckButton.IsChecked)
                            {
                                _source.SetValue(property, editor.CreateInstance());
                            }
                            else
                            {
                                _source.SetValue(property, null);
                            }
                        };
                        labelPanel.Widgets.Add(nullableCheckButton);
                    }

                    // Label
                    var label = new Label
                    {
                        AutoEllipsisString = "...",
                        AutoEllipsisMethod = FontStashSharp.RichText.AutoEllipsisMethod.Character,
                        HorizontalAlignment = Visuals.HorizontalAlignment.Stretch,
                        Background = null,
                        WrapMode = TextWrapMode.None,
                        Width = _orientation.Value == Orientation.Horizontal ? _labelWidth.Value : 0,
                        Text = labelText
                    };
                    labelPanel.Widgets.Add(label);

                    Grid.SetColumn(labelPanel, 0);
                    Grid.SetRow(labelPanel, 0);
                    itemPanel.Widgets.Add(labelPanel);

                    // Control
                    var editorControl = editor.BuildControl();

                    if (_orientation.Value == Orientation.Horizontal)
                        StackPanel.SetProportionType(editorControl, ProportionType.Fill);

                    if (_orientation.Value == Orientation.Horizontal)
                    {
                        Grid.SetColumn(editorControl, 1);
                        Grid.SetRow(editorControl, 0);
                    }
                    else
                    {
                        Grid.SetColumn(editorControl, 0);
                        Grid.SetRow(editorControl, 1);
                    }
                    itemPanel.Widgets.Add(editorControl);

                    // Reset Button
                    var resetButton = new Button()
                    {
                        Width = 20,
                        Height = 20,
                        Content = new Image()
                        {
                            Visual = new(clearIcon)
                        },
                        Background = new RegionBrush(clearIcon)
                    };

                    resetButton.Click += (s, e) => _source.ClearValue(property);
                    if (_orientation.Value == Orientation.Horizontal)
                    {
                        Grid.SetColumn(resetButton, 2);
                        Grid.SetRow(resetButton, 0);
                    }
                    else
                    {
                        Grid.SetColumn(resetButton, 1);
                        Grid.SetRow(resetButton, 0);
                    }
                    itemPanel.Widgets.Add(resetButton);

                    _editorListCache.Add((Editor: editor, EditorControl: editorControl));

                    // Container At Bottom?
                    var containerAtBottom = editor.BuildContainerAtBottom();
                    if (containerAtBottom != null)
                    {
                        containerAtBottom.Margin = new Thickness(40, 0, 0, 0);
                        itemContainer.Widgets.Add(containerAtBottom);
                    }
                }

                panel.Widgets.Add(itemContainer);
                count++;
            }

            return panel;
        }
    }
}
