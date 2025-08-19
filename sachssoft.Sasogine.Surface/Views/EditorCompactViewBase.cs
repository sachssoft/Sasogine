/* 
 * © 2024 Tobias Sachs
 * EditorViewBase
 * 26.10.2024 
 * Update 23.05.2025
*/

using System;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Controls;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System.Collections.Generic;
using Sachssoft.Sasogine.Surface.Views.Editor;
using Sachssoft.Sasogine.Interactions;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;

namespace Sachssoft.Sasogine.Surface.Views;

public abstract class EditorCompactViewBase<TRuntime> : SurfaceViewBase where TRuntime : RuntimeBase
{
    private bool _initialized;

    protected const string COLOR_MENU = "#404040";
    protected const string COLOR_SIDE = "#606060";
    protected const string COLOR_CONTENT = "#404040";
    protected const string COLOR_SELECTION = COLOR_SIDE;

    private readonly List<Tuple<ToggleButton, Container?>> _side_tab_items = new();

    private readonly IEditorToolbar _toolbar;
    private readonly IEditorToolbox _toolbox;
    private readonly IEditorMenu _menubar;
    private readonly IEditorTabContainer _side_tab_container;
    private readonly IEditorTabContainer _drawer_tab_container;
    private readonly Grid _surface = new();
    private Container? _content;

    private ushort _icon_size = 28;
    private ushort _side_width = 320;
    private ushort _drawer_height = 200;
    private ushort _container_margin = 10;

    public EditorCompactViewBase(ViewSwitchMode view_switch_mode = ViewSwitchMode.Restart)
        : base(view_switch_mode)
    {
        _toolbar = new EditorViewToolbar(this);
        _toolbox = new EditorViewToolbox(this);
        _menubar = new EditorViewMenu(this);
        _side_tab_container = new EditorTabContainer<VerticalStackPanel>(this);
        _drawer_tab_container = new EditorTabContainer<HorizontalStackPanel>(this);
    }

    protected override void OnLoad()
    {
        base.Runtime = CreateRuntime();

        OnBuilding();
        BuildLayout();
        BuildCommandBar();
        BuildToolbox();
        BuildSide();
        BuildDrawer();
        BuildContent();

        _initialized = true;

        base.Runtime?.Load();
    }

    protected override void OnUnload()
    {
        _initialized = false;

        _surface.ColumnsProportions.Clear();
        _surface.RowsProportions.Clear();
        _surface.Widgets.Clear();

        _content?.Widgets.Clear();
        _content = null;

        _toolbar.Clear();
        _toolbox.Clear();
        _menubar.Clear();
        _drawer_tab_container.Clear();
        _side_tab_container.Clear();

        base.Runtime?.Unload();
        base.Runtime = null;
    }

    private void BuildContent()
    {
        _content = CreateContent();

        if (_content != null)
        {
            Grid.SetRow(_content, 1);
            Grid.SetColumn(_content, 1);

            _surface.Widgets.Add(_content);
        }
    }

    protected virtual void OnBuilding()
    {
    }

    protected virtual Container? CreateContent()
    {
        return null;
    }

    public abstract TRuntime CreateRuntime();

    public new TRuntime Runtime => (TRuntime)base.Runtime!;

    protected ushort IconSize
    {
        get => _icon_size;
        set
        {
            CheckWasInitialized();
            _icon_size = value;
        }
    }

    protected ushort SideWidth
    {
        get => _side_width;
        set
        {
            CheckWasInitialized();
            _side_width = value;
        }
    }

    protected ushort DrawerHeight
    {
        get => _drawer_height;
        set
        {
            CheckWasInitialized();
            _drawer_height = value;
        }
    }

    protected ushort ContainerMargin
    {
        get => _container_margin;
        set
        {
            CheckWasInitialized();
            _container_margin = value;
        }
    }

    protected IEditorToolbar Toolbar => _toolbar;

    protected IEditorToolbox Toolbox => _toolbox;

    protected IEditorMenu Menu => _menubar;

    protected IEditorTabContainer Drawer => _drawer_tab_container;

    protected IEditorTabContainer Side => _side_tab_container;

    private void CheckWasInitialized()
    {
        if (_initialized)
        {
            throw new InvalidOperationException("Can not add an control element before building");
        }
    }

    private void BuildLayout()
    {
        // Layout
        _surface.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        _surface.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
        _surface.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        _surface.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
        _surface.RowsProportions.Add(new(ProportionType.Auto));
        _surface.RowsProportions.Add(new(ProportionType.Fill));
        _surface.RowsProportions.Add(new(ProportionType.Auto));
        _surface.RowsProportions.Add(new(ProportionType.Auto));

        Widgets.Add(_surface);
    }

    private void BuildCommandBar()
    {
        var container = new Grid();
        container.RowsProportions.Add(new Proportion(ProportionType.Auto));
        container.RowsProportions.Add(new Proportion(ProportionType.Auto));

        var menu = ((EditorViewMenu)_menubar)._menu;
        var toolbar = ((EditorViewToolbar)_toolbar)._panel;

        toolbar.Padding = new Thickness(10, 5, 10, 5);
        toolbar.Background = new SolidBrush(COLOR_MENU);

        Grid.SetRow(menu, 0);
        Grid.SetRow(toolbar, 1);

        container.Widgets.Add(menu);
        container.Widgets.Add(toolbar);

        // Menüleiste
        //menu.Height = 30;

        Grid.SetRow(container, 0);
        Grid.SetColumn(container, 0);
        Grid.SetColumnSpan(container, 3);

        //_toolbar.Widgets.Add(menu);
        _surface.Widgets.Add(container);
    }

    private void BuildToolbox()
    {
        var panel = ((EditorViewToolbox)_toolbox)._panel;

        panel.Padding = new Thickness(10);
        panel.Background = new SolidBrush(COLOR_MENU);

        Grid.SetRow(panel, 1);
        Grid.SetRowSpan(panel, 3);
        Grid.SetColumn(panel, 0);

        _surface.Widgets.Add(panel);
    }

    private void BuildSide()
    {
        var panel = ((EditorTabContainer<VerticalStackPanel>)_side_tab_container)._panel;
        var container = ((EditorTabContainer<VerticalStackPanel>)_side_tab_container)._container;

        // Tab
        panel.Padding = new Thickness(10, 5, 0, 0);
        panel.Spacing = 5;
        panel.Background = new SolidBrush(COLOR_MENU);

        Grid.SetRow(panel, 1);
        Grid.SetRowSpan(panel, 3);
        Grid.SetColumn(panel, 2);

        _surface.Widgets.Add(panel);

        // Content
        container.Width = _side_width;
        container.Background = new SolidBrush(COLOR_SIDE);
        container.Padding = new Thickness(_container_margin);

        Grid.SetRow(container, 0);
        Grid.SetRowSpan(container, 4);
        Grid.SetColumn(container, 3);

        _surface.Widgets.Add(container);
    }

    private void BuildDrawer()
    {
        var panel = ((EditorTabContainer<HorizontalStackPanel>)_drawer_tab_container)._panel;
        var container = ((EditorTabContainer<HorizontalStackPanel>)_drawer_tab_container)._container;

        // Tab
        panel.Padding = new Thickness(0, 10, 0, 0);
        panel.Spacing = 5;
        panel.Background = new SolidBrush(COLOR_MENU);

        Grid.SetRow(panel, 2);
        Grid.SetColumn(panel, 1);

        _surface.Widgets.Add(panel);

        // Content
        container.Height = _drawer_height;
        container.Background = new SolidBrush(COLOR_SIDE);
        container.Padding = new Thickness(_container_margin);

        Grid.SetRow(container, 3);
        Grid.SetColumn(container, 1);

        _surface.Widgets.Add(container);
    }

    private static class EditorViewUtils
    {
        internal static Image CreateIcon(Texture2D? tex, EditorViewIconSizes icon_size) => new Image
        {
            Width = icon_size == EditorViewIconSizes.Small ? 16 : 32,
            Height = icon_size == EditorViewIconSizes.Small ? 16 : 32,
            Renderable = tex != null ? new TextureRegion(tex) : null
        };

        internal static ToggleButton AddToolboxButton(StackPanel panel, string? title, Texture2D icon, EditorViewIconSizes icon_size, Action<ToggleButton> selection, bool selected = false)
        {
            var toggle = new ToggleButton()
            {
                Padding = new Thickness(5),
                IsToggled = selected,
                PressedBackground = new SolidBrush(COLOR_SELECTION),
                Content = EditorViewUtils.CreateIcon(icon, icon_size)
            };

            if (panel is HorizontalStackPanel)
            {
                //toggle.Margin = new Thickness(0, 0, separator ? 5 : 0, 0);
            }
            else if (panel is VerticalStackPanel)
            {
                //toggle.Margin = new Thickness(0, 0, 0, separator ? 5 : 0);
            }

            toggle.PressedChangingByUser += (s, e) =>
            {
                foreach (var w in panel.Widgets)
                {
                    if (w is ToggleButton t)
                    {
                        t.IsToggled = false;
                    }
                }

                toggle.IsToggled = true;
                selection.Invoke(toggle);
            };

            panel.Widgets.Add(toggle);
            return toggle;
        }
    }

    private class EditorViewToolbar : IEditorToolbar
    {
        internal HorizontalStackPanel _panel;
        private ObservableCollection<(ToolboxItemTypes Type, string? Label, Texture2D? Icon, ICommand? Command, string? Group, bool IsChecked, Widget? CustomWidget)> _items;
        private Dictionary<string, List<ToggleButton>> _group_buttons;
        private EditorViewIconSizes _icon_size = EditorViewIconSizes.Small;
        private short _spacing = 0;
        private ViewBase _owner;

        private enum ToolboxItemTypes
        {
            Separator,
            Button,
            ToggleButton,
            GroupButton,
            Icon,
            Label,
            Custom // Widget
        }

        internal EditorViewToolbar(ViewBase owner)
        {
            _owner = owner;
            Clear();
        }

        public bool IsVisible
        {
            get => _panel.IsVisible;
            set => _panel.IsVisible = value;
        }

        public EditorViewIconSizes IconSize
        {
            get => _icon_size;
            set
            {
                if (_icon_size != value)
                {
                    _icon_size = value;
                    Rebuild();
                }
            }
        }

        public short Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    Rebuild();
                }
            }
        }

        public void Clear()
        {
            if (_items != null)
                _items.CollectionChanged -= ItemsCollectionChanged;

            _panel = new HorizontalStackPanel();
            _items = new();
            _items.CollectionChanged += ItemsCollectionChanged;
            _group_buttons = new();
        }

        private void ItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            => Rebuild();

        private void Rebuild()
        {
            _panel.Widgets.Clear();
            _panel.Spacing = _spacing;

            _group_buttons.Clear(); // Wichtiger Reset für Gruppentasten

            foreach (var item in _items)
            {
                var widget = CreateWidget(item);
                _panel.Widgets.Add(widget);
            }
        }

        private Widget CreateWidget((ToolboxItemTypes Type, string? Label, Texture2D? Icon, ICommand? Command, string? Group, bool IsChecked, Widget? CustomWidget) item)
        {
            switch (item.Type)
            {
                case ToolboxItemTypes.Button:
                    return new Button
                    {
                        Content = EditorViewUtils.CreateIcon(item.Icon, _icon_size),
                        Padding = new Thickness(5),
                        Command = item.Command,
                        ViewOwner = _owner,
                        //TooltipText = item.Label
                    };

                case ToolboxItemTypes.ToggleButton:
                    var toggle = new ToggleButton
                    {
                        Content = EditorViewUtils.CreateIcon(item.Icon, _icon_size),
                        Padding = new Thickness(5),
                        IsToggled = item.IsChecked,
                        Command = item.Command,
                        CommandParameter = item.IsChecked,
                        ViewOwner = _owner,
                        //TooltipText = item.Label
                    };
                    toggle.IsToggledChanged += (s, e) =>
                    {
                        toggle.CommandParameter = toggle.IsToggled;
                    };
                    return toggle;

                case ToolboxItemTypes.GroupButton:
                    var group = new ToggleButton
                    {
                        Content = EditorViewUtils.CreateIcon(item.Icon, _icon_size),
                        Padding = new Thickness(5),
                        IsToggled = item.IsChecked,
                        Command = item.Command,
                        CommandParameter = item.IsChecked,
                        ViewOwner = _owner,
                        //TooltipText = item.Label
                    };

                    if (!string.IsNullOrEmpty(item.Group))
                    {
                        if (!_group_buttons.TryGetValue(item.Group, out var buttons))
                        {
                            buttons = new List<ToggleButton>();
                            _group_buttons[item.Group] = buttons;
                        }

                        buttons.Add(group);

                        group.IsToggledChanged += (s, e) =>
                        {
                            if (group.IsToggled)
                            {
                                foreach (var btn in buttons)
                                {
                                    if (btn != group)
                                        btn.IsToggled = false;
                                }
                            }

                            group.CommandParameter = group.IsToggled;
                        };
                    }

                    return group;

                case ToolboxItemTypes.Separator:
                    return new VerticalSeparator
                    {
                        Height = _panel.Height > 0 ? _panel.Height : 20,
                        Width = 5,
                        Background = null
                    };

                case ToolboxItemTypes.Label:
                    return new Label
                    {
                        Text = item.Label ?? "",
                        Padding = new Thickness(5, 0, 5, 0),
                        VerticalAlignment = VerticalAlignment.Center,
                        //ViewOwner = _owner
                    };

                case ToolboxItemTypes.Icon:
                    var icon_container = new HorizontalStackPanel();
                    var icon = EditorViewUtils.CreateIcon(item.Icon, _icon_size);
                    icon.VerticalAlignment = VerticalAlignment.Center;
                    icon_container.Widgets.Add(icon);
                    icon_container.Margin = new Thickness(5, 0, 5, 0);
                    icon_container.VerticalAlignment = VerticalAlignment.Center;
                    return icon_container;

                case ToolboxItemTypes.Custom:
                    if (item.CustomWidget != null)
                    {
                        return item.CustomWidget;
                    }
                    else
                        return new Label { Text = "?" };

                default:
                    return new Label { Text = "?" };
            }
        }

        // Öffentliche Methoden für Items

        public void AddButton(string label, Texture2D icon, ICommand command)
        {
            _items.Add((ToolboxItemTypes.Button, label, icon, command, null, false, null));
        }

        public void AddToggleButton(string label, Texture2D icon, ICommand command, bool is_checked = false)
        {
            _items.Add((ToolboxItemTypes.ToggleButton, label, icon, command, null, is_checked, null));
        }

        public void AddGroupButton(string label, Texture2D icon, string group, ICommand command, bool is_checked = false)
        {
            _items.Add((ToolboxItemTypes.GroupButton, label, icon, command, group, is_checked, null));
        }

        public void AddSeparator()
        {
            _items.Add((ToolboxItemTypes.Separator, null, null, null, null, false, null));
        }

        public void AddLabel(string text)
        {
            _items.Add((ToolboxItemTypes.Label, text, null, null, null, false, null));
        }

        public void AddIcon(Texture2D icon)
        {
            _items.Add((ToolboxItemTypes.Icon, null, icon, null, null, false, null));
        }

        public void AddWidget(Widget widget)
        {
            _items.Add((ToolboxItemTypes.Custom, null, null, null, null, false, widget));
        }
    }

    private class EditorViewToolbox : IEditorToolbox
    {
        internal VerticalStackPanel _panel;
        private ObservableCollection<(ToolboxItemTypes Type, string? Label, Texture2D? Icon, ICommand? Command, string Group, bool IsChecked)> _items;
        private Dictionary<string, List<ToggleButton>> _group_buttons;
        private short _columns = 2; // Standard: 2 Spalten
        private HorizontalStackPanel? _current_row;
        private int _current_column_index;
        private EditorViewIconSizes _icon_size = EditorViewIconSizes.Large;
        private short _spacing = 10;
        private ViewBase _owner;

        private enum ToolboxItemTypes
        {
            Break,
            Separator,
            Button,
            ToggleButton,
            GroupButton
        }

        internal EditorViewToolbox(ViewBase owner)
        {
            _owner = owner;

            Clear();
        }

        public bool IsVisible
        {
            get => _panel.IsVisible;
            set => _panel.IsVisible = value;
        }

        public short Columns
        {
            get => _columns;
            set
            {
                _columns = short.Max(1, value);
                Rebuild();
            }
        }

        public EditorViewIconSizes IconSize
        {
            get => _icon_size;
            set
            {
                if (_icon_size != value)
                {
                    _icon_size = value;
                    Rebuild();
                }
            }
        }

        public short Spacing
        {
            get => _spacing;
            set
            {
                if (_spacing != value)
                {
                    _spacing = value;
                    Rebuild();
                }
            }
        }

        public void Clear()
        {
            if (_items != null)
                _items.CollectionChanged -= ItemsCollectionChanged;

            _panel = new VerticalStackPanel();
            _items = new();
            _items.CollectionChanged += ItemsCollectionChanged;
            _group_buttons = new();

            StartNewRow();
        }

        private void ItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            => Rebuild();

        private void StartNewRow()
        {
            _current_row = new HorizontalStackPanel()
            {
                Spacing = _spacing
            };
            _panel.Widgets.Add(_current_row);
            _current_column_index = 0;
        }

        private void Rebuild()
        {
            _panel.Widgets.Clear();
            _panel.Spacing = _spacing;
            StartNewRow();

            foreach (var item in _items)
            {
                AddItemToUI(item);
            }
        }

        private void AddItemToUI((ToolboxItemTypes Type, string? Label, Texture2D? Icon, ICommand? Command, string Group, bool IsChecked) item)
        {
            if (item.Type == ToolboxItemTypes.Break && _columns > 1)
            {
                StartNewRow();
                return;
            }

            Widget widget = CreateWidget(item);

            _current_row?.Widgets.Add(widget);
            _current_column_index++;

            if (_current_column_index >= _columns)
            {
                StartNewRow();
            }
        }

        private Widget CreateWidget((ToolboxItemTypes Type, string? Label, Texture2D? Icon, ICommand? Command, string? Group, bool IsChecked) item)
        {
            var label = item.Label ?? "";

            switch (item.Type)
            {
                case ToolboxItemTypes.Button:
                    return new Button
                    {
                        Content = EditorViewUtils.CreateIcon(item.Icon, _icon_size),
                        Padding = new Thickness(5),
                        Command = item.Command,
                        ViewOwner = _owner
                    };

                case ToolboxItemTypes.ToggleButton:
                    var toggle = new ToggleButton
                    {
                        Content = EditorViewUtils.CreateIcon(item.Icon, _icon_size),
                        Padding = new Thickness(5),
                        IsToggled = item.IsChecked,
                        Command = item.Command,
                        CommandParameter = item.IsChecked,
                        ViewOwner = _owner
                    };
                    toggle.IsToggledChanged += (s, e) =>
                    {
                        ((ToggleButton)s!).CommandParameter = ((ToggleButton)s).IsToggled;
                    };
                    return toggle;

                case ToolboxItemTypes.GroupButton:
                    {
                        var group = new ToggleButton
                        {
                            Content = EditorViewUtils.CreateIcon(item.Icon, _icon_size),
                            Padding = new Thickness(5),
                            IsToggled = item.IsChecked,
                            Command = item.Command,
                            CommandParameter = item.IsChecked,
                            ViewOwner = _owner
                        };

                        if (!_group_buttons.TryGetValue(item.Group!, out var buttons))
                        {
                            buttons = new List<ToggleButton>();
                            _group_buttons[item.Group!] = buttons;
                        }

                        buttons.Add(group);

                        group.IsToggledChanged += (s, e) =>
                        {
                            var toggled = (ToggleButton)s!;

                            // Wenn dieser Button jetzt aktiv ist, alle anderen deaktivieren
                            if (toggled.IsToggled)
                            {
                                foreach (var btn in buttons)
                                {
                                    if (btn != toggled)
                                    {
                                        btn.IsToggled = false;
                                    }
                                }
                            }

                            // Immer Parameter setzen
                            toggled.CommandParameter = toggled.IsToggled;
                        };

                        return group;
                    }

                case ToolboxItemTypes.Separator:
                    return new HorizontalSeparator
                    {
                        Height = 10,
                        Width = _panel.Width,
                        Background = null
                    };

                default:
                    return new Label { Text = "?" };
            }
        }

        public void AddBreak()
        {
            _items.Add((ToolboxItemTypes.Break, null, null, null, "", false));
        }

        public void AddButton(string label, Texture2D icon, ICommand command)
        {
            _items.Add((ToolboxItemTypes.Button, label, icon, command, "", false));
        }

        public void AddToggleButton(string label, Texture2D icon, ICommand command, bool is_checked = false)
        {
            _items.Add((ToolboxItemTypes.ToggleButton, label, icon, command, "", is_checked));
        }

        public void AddGroupButton(string label, Texture2D icon, string group, ICommand command, bool is_checked = false)
        {
            _items.Add((ToolboxItemTypes.GroupButton, label, icon, command, group, is_checked));
        }

        public void AddSeparator()
        {
            _items.Add((ToolboxItemTypes.Separator, null, null, null, "", false));
        }
    }

    private class EditorViewMenu : IEditorMenu
    {
        private ViewBase _owner;
        internal HorizontalMenu _menu;
        private ObservableCollection<(MenuItemTypes Type, string? Label, ICommand? Command, string Group, bool IsChecked, EditorViewMenu? Child)> _items;
        private Dictionary<string, List<MenuItem>> _groupItems;
        private EditorViewIconSizes _iconSize = EditorViewIconSizes.Large;
        private EventHandler? _collection_changed;

        private enum MenuItemTypes
        {
            Separator,
            Item,
            Toggle,
            Group
        }

        internal EditorViewMenu(ViewBase owner)
        {
            _owner = owner;
            Clear();
        }

        public void Clear()
        {
            if (_items != null)
                _items.CollectionChanged -= ItemsCollectionChanged;

            _menu = new HorizontalMenu();
            _items = new();
            _items.CollectionChanged += ItemsCollectionChanged;
            _groupItems = new();
        }

        private void ItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Rebuild();
            _collection_changed?.Invoke(this, EventArgs.Empty);
        }

        private void Rebuild()
        {
            _menu.Items.Clear();
            _groupItems.Clear();

            foreach (var item in _items)
            {
                _menu.Items.Add(CreateMenuItem(item));
            }
        }

        private IMenuItem CreateMenuItem((MenuItemTypes Type, string? Label, ICommand? Command, string Group, bool IsChecked, EditorViewMenu? Child) item)
        {
            switch (item.Type)
            {
                case MenuItemTypes.Separator:
                    return new MenuSeparator();

                case MenuItemTypes.Item:
                case MenuItemTypes.Toggle:
                case MenuItemTypes.Group:
                    var menuItem = new MenuItem
                    {
                        Text = item.Label,
                        Command = item.Command,
                        IsCheckable = item.Type != MenuItemTypes.Item,
                        IsChecked = item.IsChecked,
                        CommandParameter = item.IsChecked,
                        ViewOwner = _owner
                    };

                    if (item.Type == MenuItemTypes.Group)
                    {
                        if (!_groupItems.TryGetValue(item.Group, out var groupList))
                        {
                            groupList = new List<MenuItem>();
                            _groupItems[item.Group] = groupList;
                        }
                        groupList.Add(menuItem);

                        menuItem.IsCheckedChanged += (s, e) =>
                        {
                            var toggled = (MenuItem)s!;
                            if (toggled.IsChecked)
                            {
                                foreach (var other in groupList)
                                {
                                    if (other != toggled)
                                        other.IsChecked = false;
                                }
                            }
                            toggled.CommandParameter = toggled.IsChecked;
                        };
                    }
                    else if (item.Type == MenuItemTypes.Toggle)
                    {
                        menuItem.IsCheckedChanged += (s, e) =>
                        {
                            var mi = (MenuItem)s!;
                            mi.CommandParameter = mi.IsChecked;
                        };
                    }

                    // Child-Menü hinzufügen, falls vorhanden
                    if (item.Child != null)
                    {
                        menuItem.Items.Clear();

                        // Rekursiv alle Kind-Items hinzufügen
                        foreach (var childRaw in item.Child._items)
                        {
                            var childMenuItem = item.Child.CreateMenuItem(childRaw);
                            menuItem.Items.Add(childMenuItem);
                        }

                        // WICHTIG: erzwinge Menüerkennung auch bei leerem Child-Menü
                        if (menuItem.Items.Count == 0)
                        {
                            menuItem.Items.Add(new MenuItem { Text = "(empty)" });
                        }
                    }

                    return menuItem;

                default:
                    return new MenuItem { Text = "?" };
            }
        }

        public void Add(string label, ICommand command)
        {
            _items.Add((MenuItemTypes.Item, label, command, "", false, null));
        }

        public void AddToggle(string label, ICommand command)
        {
            _items.Add((MenuItemTypes.Toggle, label, command, "", false, null));
        }

        public void AddGroup(string label, string group, ICommand command)
        {
            _items.Add((MenuItemTypes.Group, label, command, group, false, null));
        }

        public void AddSeparator()
        {
            _items.Add((MenuItemTypes.Separator, null, null, "", false, null));
        }

        public IEditorMenu AddChild(string label)
        {
            var childMenu = new EditorViewMenu(_owner);

            // Rebuild sowohl des Child-Menüs als auch des Elternmenüs (damit Anzeige aktualisiert wird)
            childMenu._items.CollectionChanged += (s, e) =>
            {
                childMenu.Rebuild();
                Rebuild(); // wichtig!
            };

            _items.Add((MenuItemTypes.Item, label, null, "", false, childMenu));
            return childMenu;
        }

    }

    private class EditorTabContainer<TStackPanel> : IEditorTabContainer where TStackPanel : StackPanel, new()
    {
        internal TStackPanel _panel;
        internal Grid _container;
        private bool _is_visible = true;
        private bool _is_tab_visible = true;
        private ObservableCollection<(string? Title, Texture2D Icon, Container Container)> _items;
        private ViewBase _owner;
        private EditorViewIconSizes _icon_size = EditorViewIconSizes.Large;

        public EditorTabContainer(ViewBase owner)
        {
            _owner = owner;
            Clear();
        }

        public bool IsVisible
        {
            get => _is_visible;
            set
            {
                _is_visible = value;
                _container.IsVisible = _is_visible;
                _panel.IsVisible = _is_tab_visible;
            }
        }

        public bool IsTabVisible
        {
            get => _is_tab_visible;
            set
            {
                _is_tab_visible = value;
                _panel.IsVisible = _is_tab_visible;
                _container.Background = new SolidBrush(_is_tab_visible ? COLOR_SIDE : COLOR_CONTENT);
            }
        }

        public EditorViewIconSizes IconSize
        {
            get => _icon_size;
            set
            {
                if (_icon_size != value)
                {
                    _icon_size = value;
                    Rebuild();
                }
            }
        }

        public void Clear()
        {
            if (_items != null)
                _items.CollectionChanged -= ItemsCollectionChanged;

            _panel = new TStackPanel();
            _container = new Grid();
            _items = new();
            _items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            => Rebuild();

        private void Rebuild()
        {
            _panel.Widgets.Clear();
            _container.Widgets.Clear();

            foreach (var item in _items)
            {
                var tab_item = CreateTabItem(item);
                tab_item.Tag = item.Container;
                _panel.Widgets.Add(tab_item);
            }

            if (_items.Count > 0)
            {
                _container.Widgets.Add(_items[0].Container);
            }
        }

        private ToggleButton CreateTabItem((string? Title, Texture2D Icon, Container Container) item)
        {
            var button = EditorViewUtils.AddToolboxButton(_panel, item.Title, item.Icon, _icon_size, (t) =>
            {
                var container = (Container)t.Tag!;

                if (_container.Widgets[0] != container)
                {
                    _container.Widgets.Clear();
                    _container.Widgets.Add(container);
                }

                foreach (ToggleButton tab_item in _panel.Widgets)
                {
                    tab_item.IsToggled = (tab_item == t);
                }
            });

            return button;
        }

        public void Add(string title, Texture2D icon, Container container)
        {
            _items.Add(new(title, icon, container));
        }
    }
}