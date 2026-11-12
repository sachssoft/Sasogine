using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

/// <summary>Verifies data-driven selection, recycling and template integration across the specialized controls.</summary>
internal static class DataDrivenControlChecks
{
    /// <summary>Exercises the source and template contracts using a headless viewport.</summary>
    /// <param name="check">The assertion callback.</param>
    /// <param name="layout">The shared layout context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var frame = new ControlInputFrame(new Size2(400, 180), TimeSpan.Zero);
        void Update(Workspace root)
        {
            root.Update(frame, layout);
            root.Update(frame, layout);
        }

        var source = new ObservableCollection<string>(Enumerable.Range(0, 150).Select(i => $"Rank {i}"));
        var rows = new RecyclingItemTemplate<TextBlock>(static () => new TextBlock { IsFocusable = true },
            static (row, value) => row.Text = (string)value!);
        var list = new ListBox { ItemsSource = source, ItemTemplate = rows, SelectedValue = source[0] };
        using (var root = new Workspace(list))
        {
            Update(root);
            var host = list.Children.OfType<ItemsControl>().Single();
            check(list.SelectedIndex == 0 && host.ContainerFromIndex(0).IsItemSelected, "ListBox initializes a bound selection before its source is realized");
            root.Focus(host.ContainerFromIndex(0));
            host.ItemsScrollViewer!.VerticalScrollBar.Value = 1400;
            Update(root);
            check(host.TryContainerFromIndex(0) is null && (string?)list.SelectedValue == "Rank 0",
                "ListBox retains the data selection while the container is recycled");
            check(ReferenceEquals(root.FocusedControl, list), "Recycling a focused row transfers focus to its list");
            host.ItemsScrollViewer.VerticalScrollBar.Value = 0;
            Update(root);
            check(host.ContainerFromIndex(0).IsItemSelected, "The returning ListBox container restores selection state");
            source.Move(0, 3);
            Update(root);
            check(list.SelectedIndex == 3 && host.ContainerFromIndex(3).IsItemSelected, "ListBox selection follows a moved source item");
        }

        var assetData = new ObservableCollection<AssetItem>();
        int previews = 0;
        for (int i = 0; i < 100; i++)
            assetData.Add(new AssetItem(new Sachssoft.Engine.Assets.Graphics.Texture2DAsset($"asset-{i}"), $"Asset {i}",
                preview: () => { previews++; return null; }));
        var assets = new AssetView { ItemsSource = assetData, ViewMode = AssetViewMode.Tiles };
        using (var root = new Workspace(assets))
        {
            Update(root);
            check(assets.ItemsPresenter?.ItemsPanelRoot is VirtualAdaptiveTilePanel &&
                ReferenceEquals(assets.ItemsPresenter.TryContainerFromIndex(0)?.DataContext, assetData[0]) && previews is > 0 and < 100,
                "AssetView creates adaptive tiles and requests previews only for its realized asset data");
            assets.ViewMode = AssetViewMode.List;
            Update(root);
            check(assets.ItemsPresenter?.ItemsPanelRoot is VirtualStackPanel, "AssetView switches to the shared virtual stack in list mode");
        }

        var combo = new ComboBox
        {
            ItemsSource = source,
            ItemTemplate = rows,
            SelectedValue = source[2],
            SelectedContentTemplate = new DelegateTemplate<object?, Control>(value => new TextBlock { Text = $"Selected {value}" })
        };
        using (var root = new Workspace(combo))
        {
            Update(root);
            check(combo.SelectedIndex == 2 && Equals(combo.Children.Single().DataContext, source[2]), "ComboBox applies its closed selection template");
            combo.OpenDropDown();
            Update(root);
            ListBox popup = root.EnumerateControls().OfType<ListBox>().Single();
            check(popup.SelectedIndex == 2 && ReferenceEquals(popup.ItemsSource, source) && popup.Children.OfType<ItemsControl>().Any(),
                "ComboBox popup uses the source and the shared templated ListBox");
        }

        var table = new DataTable { ItemsSource = source };
        table.Columns.Add(new DataTableTextColumn<string>("Rank", static value => value) { CellTemplate = rows });
        using (var root = new Workspace(table))
        {
            Update(root);
            TextBlock[] cells = table.Children.OfType<TextBlock>().ToArray();
            check(cells.Length is > 0 and < 20 && cells.Any(cell => cell.Text == source[0]), "DataTable realizes templates only for visible cells");
            table.VerticalScrollBar.Value = 900;
            Update(root);
            check(table.Children.OfType<TextBlock>().SequenceEqual(cells) && cells.All(cell => cell.Text != source[0]),
                "DataTable reuses the same cell controls when scrolling");
        }

        var pages = new ObservableCollection<string> { "One", "Two" };
        var content = new DelegateTemplate<object?, Control>(value => new TextBlock { Text = (string)value! });
        var tabs = new TabControl { ItemsSource = pages, HeaderTemplate = content, ContentTemplate = content };
        using (var root = new Workspace(tabs))
        {
            Update(root);
            tabs.SelectedIndex = 1;
            var selected = tabs.SelectedItem;
            pages.Move(1, 0);
            Update(root);
            check(ReferenceEquals(tabs.SelectedItem, selected) && tabs.SelectedIndex == 0 && Equals(selected?.DataContext, "Two"),
                "Tabs keep their data-backed page and selection when the source moves");
        }

        var carousel = new Carousel { ItemsSource = pages, PageTemplate = content, Transition = CarouselTransition.None };
        using (var root = new Workspace(carousel))
        {
            Update(root);
            Control first = carousel.Items[0];
            pages.Move(0, 1);
            Update(root);
            check(ReferenceEquals(carousel.Items[1], first) && ReferenceEquals(carousel.SelectedItem, first),
                "Carousel preserves a templated page and its selection through a source move");
        }

        var tree = new TreeView
        {
            ItemsSource = pages,
            NodeTemplate = new DelegateTemplate<object?, TreeViewItem>(value => new TreeViewItem { Text = (string)value!, Value = value }),
            NodeContentTemplate = rows
        };
        using (var root = new Workspace(tree))
        {
            Update(root);
            TreeViewItem node = tree.GetVisibleItem(0);
            node.Items.Add(new TreeViewItem { Text = "Child", Value = "Child" });
            node.IsExpanded = true;
            Update(root);
            check(tree.VisibleCount == 3 && tree.GetVisibleItem(1).Text == "Child" &&
                tree.Children.OfType<TextBlock>().Any(control => control.Text == "Child"),
                "TreeView template nodes and their visuals refresh after nested collection and expansion changes");
        }

        var menu = new ContextMenu
        {
            ItemsSource = pages,
            ItemTemplate = new DelegateTemplate<object?, MenuEntry>(value => new MenuItem { Text = (string)value! })
        };
        using (var root = new Workspace(new Panel()))
        {
            menu.Show(root, new Vector2(5, 5));
            Update(root);
            pages.Add("Three");
            Update(root);
            check(menu.Items.Count == 3 && menu.Items[2] is MenuItem { Text: "Three" }, "Context menus inherit live item templates from MenuBar");
        }
    }
}
