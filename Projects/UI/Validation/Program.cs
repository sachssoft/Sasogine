using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Layouts;
using Sachssoft.Engine.Controls.Styling;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Binding;
using Sachssoft.Engine.Input;

internal static class Program
{
    private static int _checks, _groups;
    private static readonly ControlLayoutContext Layout = new(new TestFont());
    private static readonly Size2 Size = new(400, 300);
    private static void Check(bool ok, string message = "Assertion failed")
    {
        _checks++;
        if (!ok)
            throw new InvalidOperationException(message);
    }

    private static void Run(string name, Action test)
    {
        test();
        _groups++;
        Console.WriteLine("PASS " + name);
    }

    private static void Frame(Workspace root, params ControlInputEvent[] input) => root.Update(new ControlInputFrame(Size, TimeSpan.FromSeconds(.016), input), Layout);
    private static ControlInputEvent P(ControlInputEventType t, float x = 20, float y = 20, int id = 0) => new(t, new(x, y), PointerId: id, PointerKind: id == 0 ? ControlPointerKind.Mouse : ControlPointerKind.Touch);
    private static void Click(Workspace root, float x = 20, float y = 20) => Frame(root, P(ControlInputEventType.PointerDown, x, y), P(ControlInputEventType.PointerUp, x, y));
    private static int Main(string[] args)
    {
        try
        {
            Run("Scroll visibility remains stable across unbounded measurement and finite arrangement", () =>
            {
                var list = new ListBox();
                for (int i = 0; i < 40; i++)
                    list.Items.Add(new SelectorItem("Item " + i));
                var root = new Workspace(list);
                Frame(root);
                int changes = 0;
                list.ScrollBar.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == "IsVisible")
                        changes++;
                };
                for (int i = 0; i < 20; i++)
                {
                    list.Measure(Layout, new Size2(400, float.PositiveInfinity));
                    list.Arrange(new Bounds2(0, 0, 400, 100), true);
                    Check(list.ScrollBar.IsVisible && list.ScrollBar.Maximum > 0);
                }

                Check(changes == 0, "Measurement must not toggle the arranged scrollbar");
                list.Measure(Layout, new Size2(400, 2000));
                list.Arrange(new Bounds2(0, 0, 400, 2000), true);
                Check(!list.ScrollBar.IsVisible && list.ScrollBar.Maximum == 0, "Growing the viewport hides the bar");
                list.Measure(Layout, new Size2(400, 100));
                list.Arrange(new Bounds2(0, 0, 400, 100), true);
                Check(list.ScrollBar.IsVisible, "Shrinking the viewport restores the bar");
                list.Items.Clear();
                Frame(root);
                Check(!list.ScrollBar.IsVisible && list.ScrollBar.Maximum == 0, "Removing items resets scrolling");
                var viewer = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Width = 800,
                        Height = 800
                    }
                };
                root = new Workspace(viewer);
                Frame(root);
                changes = 0;
                viewer.HorizontalScrollBar.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == "IsVisible")
                        changes++;
                };
                viewer.VerticalScrollBar.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == "IsVisible")
                        changes++;
                };
                for (int i = 0; i < 20; i++)
                {
                    viewer.Measure(Layout, new Size2(float.PositiveInfinity, float.PositiveInfinity));
                    viewer.Arrange(new Bounds2(0, 0, 400, 300), true);
                    Check(viewer.HorizontalScrollBar.IsVisible && viewer.VerticalScrollBar.IsVisible);
                }

                Check(changes == 0, "Unbounded measurement must preserve arranged viewer visibility");
                viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
                Frame(root);
                Check(!viewer.HorizontalScrollBar.IsVisible && viewer.VerticalScrollBar.IsVisible);
            });
            Run("StyleProperty explicit default, cached fallback and reset", () =>
            {
                var p = new StyleProperty<int>(0);
                Check(p.Apply(5) && p.Value == 5);
                Check(p.SetLocal(0) && p.Value == 0 && p.HasLocalValue);
                Check(!p.Apply(8) && p.Value == 0);
                Check(p.ClearLocal() && p.Value == 8 && !p.HasLocalValue);
                Check(!p.SetLocal(8) && p.HasLocalValue);
                Check(!p.ClearLocal());
            });
            Run("Element notifications and cached event arguments", () =>
            {
                var button = new Button();
                int count = 0;
                PropertyChangedEventArgs? first = null;
                bool reused = true;
                button.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(button.IsEnabled))
                    {
                        count++;
                        if (first is null)
                            first = e;
                        else
                            reused &= ReferenceEquals(first, e);
                    }
                };
                button.IsEnabled = false;
                button.IsEnabled = false;
                button.IsEnabled = true;
                Check(count == 2 && reused);
                Check(button is Element && button is INotifyPropertyChanged && button is ButtonBase);
            });
            Run("Automatic factories, all controls and typed custom registration", () =>
            {
                var registry = new UIRegistry();
                Check(registry.Count >= 43);
                foreach (string kind in registry.Kinds)
                {
                    var c = registry.Create(kind);
                    Check(c is INotifyPropertyChanged, kind);
                    var root = new Workspace(c, registry);
                    Frame(root);
                    Check(c.Workspace == root, kind);
                }

                registry.Register("Custom", static () => new MeasureProbe());
                Check(registry.Create("Custom") is MeasureProbe);
            });
            Run("Central styles, equal-default override and live style changes", () =>
            {
                var button = new Button
                {
                    StyleId = "Accent",
                    Width = 100,
                    Height = 40
                };
                var root = new Workspace(button);
                root.Styles.Register(new Style("Accent", "Button") { ["IsEnabled"] = "false", ["Padding"] = "10" });
                Frame(root);
                Check(!button.IsEnabled && button.Padding.Left == 10);
                button.IsEnabled = true;
                root.Styles["Accent"]["Padding"] = "12";
                Frame(root);
                Check(button.IsEnabled && button.Padding.Left == 12);
                button.ResetStyleProperty(nameof(button.IsEnabled));
                Frame(root);
                Check(!button.IsEnabled);
                button.IsEnabled = false;
                root.Styles["Accent"]["IsEnabled"] = "true";
                Frame(root);
                Check(!button.IsEnabled, "Explicit default remains local");
                button.ResetStyleProperty(nameof(button.IsEnabled));
                Check(button.IsEnabled);
                int paddingChanges = 0;
                button.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(button.Padding))
                        paddingChanges++;
                };
                root.Styles["Accent"]["TextColor"] = "#FF0000";
                Frame(root);
                Check(paddingChanges == 0, "Style inheritance must not expose fallback transitions");
            });
            Run("Button frame polling, held state, release and cancellation", () =>
            {
                var button = new Button
                {
                    Width = 100,
                    Height = 40,
                    HorizontalAlignment = ControlAlignment.Start,
                    VerticalAlignment = ControlAlignment.Start
                };
                var root = new Workspace(button);
                Frame(root);
                Frame(root, P(ControlInputEventType.PointerDown));
                Check(button.IsHeld && !button.IsPressed);
                Frame(root, P(ControlInputEventType.PointerUp));
                Check(button.IsPressed && button.IsReleased && !button.IsHeld);
                Frame(root);
                Check(!button.IsPressed && !button.IsReleased);
                Frame(root, P(ControlInputEventType.PointerDown), new ControlInputEvent(ControlInputEventType.Deactivate));
                Check(!button.IsHeld && root.CapturedControl is null);
                int calls = 0;
                bool allowed = true;
                button.Command = new Command(_ => calls++, _ => allowed);
                Click(root);
                Check(calls == 1);
                allowed = false;
                Frame(root);
                Click(root);
                Check(calls == 1 && !button.IsEnabled);
            });
            Run("Immediate control ownership, atomic transfers and deferred traversal", () => ControlTreeChecks.Run(Check, Layout));
            Run("Tool groups, uniform button palettes, status content and cached wrapping", () => BarControlChecks.Run(Check, Layout));
            Run("Grid-owned placement, observable rows and columns, and invalidation", () =>
            {
                var grid = new Grid();
                grid.Rows.Add(new GridRow(100));
                grid.Rows.Add(new GridRow(GridLength.Proportion()));
                grid.Columns.Add(new GridColumn(120, GridUnitType.Pixel));
                grid.Columns.Add(new GridColumn());
                var child = new Button();
                grid.Children.Add(child);
                grid.Cells.Put(child, row: 1, column: 1);
                var root = new Workspace(grid);
                Frame(root);
                Check(child.Bounds.X >= 120 && child.Bounds.Y >= 100);
                grid.Rows[0].Height = 80;
                Frame(root);
                Check(child.Bounds.Y == 80);
                grid.Cells.Remove(child);
                grid.Cells.Put(child, row: 0, column: 0);
                Frame(root);
                Check(child.Bounds.X == 0 && child.Bounds.Y == 0);
                var probe = new MeasureProbe();
                var r = new Workspace(probe);
                Frame(r);
                Frame(r);
                int n = probe.Measurements;
                for (int i = 0; i < 10; i++)
                    Frame(r);
                Check(probe.Measurements == n, "Unchanged layout must not remeasure");
                probe.Width = 70;
                Frame(r);
                Check(probe.Measurements > n && probe.Bounds.Width == 70);
                var siblings = new StackPanel();
                var first = new MeasureProbe();
                var second = new MeasureProbe();
                siblings.Add(first);
                siblings.Add(second);
                var siblingsRoot = new Workspace(siblings);
                Frame(siblingsRoot);
                Frame(siblingsRoot);
                int secondBefore = second.Measurements;
                first.Width = 90;
                Frame(siblingsRoot);
                Check(second.Measurements == secondBefore, "Unchanged sibling must retain measurement");
            });
            Run("Grid cell identity, ownership, spans and cached layout", () =>
            {
                var grid = new Grid();
                grid.Rows.Add(new GridRow { Height = 100 });
                grid.Rows.Add(new GridRow { Height = GridLength.Proportion() });
                grid.Columns.Add(new GridColumn { Width = 120 });
                grid.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
                var child = new MeasureProbe();
                grid.Children.Add(child);
                grid.Cells.Put(child, row: 1, column: 1);
                int notifications = 0;
                grid.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(Grid.Cells))
                        notifications++;
                };
                grid.Cells.Put(child, row: 0, column: 0);
                Check(grid.Cells.Count == 1 && grid.Children.Count == 1);
                Check(grid.Cells[child] == new GridCell(0, 0) && notifications == 1);
                grid.Cells.Put(child, row: 0, column: 0);
                Check(notifications == 1, "Identical Put must not invalidate layout");
                Check(grid.Cells.Remove(child) && !grid.Cells.Remove(child));
                Check(child.Parent == grid && grid.Children.Contains(child) && notifications == 2);
                var root = new Workspace(grid);
                Frame(root);
                Check(child.Bounds.X == 0 && child.Bounds.Y == 0, "Released children use the first cell");
                grid.Cells.Put(child, row: 1, columnSpan: 2);
                Frame(root);
                Check(child.Bounds.Y == 100 && child.Bounds.Width == 400);
                grid.Columns[0].Width = 140;
                grid.Cells.Remove(child);
                grid.Cells.Put(child, column: 1);
                Frame(root);
                Check(child.Bounds.X == 140, "Column changes must invalidate layout");
                var other = new Button();
                grid.Cells.Put(other, column: 1);
                Check(other.Parent is null, "Put must not add children");
                grid.Children.Add(other);
                Frame(root);
                Check(other.Bounds.X == child.Bounds.X, "Different controls may share a cell");
                grid.Children.Remove(child);
                Check(!grid.Cells.TryGetValue(child, out _) && child.Parent is null);
                var foreignPanel = new StackPanel();
                foreignPanel.Children.Add(child);
                bool foreign = false;
                try
                {
                    grid.Cells.Put(child);
                }
                catch (InvalidOperationException)
                {
                    foreign = true;
                }

                Check(foreign && grid.Cells.Count == 1);
                bool invalid = false;
                try
                {
                    grid.Cells.Put(new Button(), rowSpan: 0);
                }
                catch (ArgumentOutOfRangeException)
                {
                    invalid = true;
                }

                Check(invalid && grid.Cells.Count == 1);
                invalid = false;
                try
                {
                    grid.Cells.Put(null!);
                }
                catch (ArgumentNullException)
                {
                    invalid = true;
                }

                Check(invalid && grid.Cells.Count == 1);
                grid.Cells.Remove(other);
                grid.Cells.Put(other, column: 1, columnSpan: 2);
                invalid = false;
                try
                {
                    Frame(root);
                }
                catch (InvalidOperationException)
                {
                    invalid = true;
                }

                Check(invalid, "Spans outside the grid must not silently shrink");
                grid.Cells.Remove(other);
                grid.Cells.Put(other);
                Frame(root);
                var frame = new ControlInputFrame(Size, TimeSpan.Zero);
                for (int i = 0; i < 100; i++)
                    root.Update(frame, Layout);
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 1000; i++)
                    root.Update(frame, Layout);
                Check(GC.GetAllocatedBytesForCurrentThread() == before, "Stable grids must not allocate per frame");
                grid.Children.Clear();
                Check(grid.Cells.Count == 0 && grid.Children.Count == 0 && other.Parent is null);
            });
            Run("Grid proportional sizing, limits, automatic content and spans", () =>
            {
                var grid = new Grid();
                grid.Columns.Add(new GridColumn { Width = 100 });
                grid.Columns.Add(new GridColumn { Width = GridLength.Proportion(3) });
                grid.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
                var root = new Workspace(grid);
                Frame(root);
                Check(grid.GetColumnWidth(0) == 100 && grid.GetColumnWidth(1) == 225 && grid.GetColumnWidth(2) == 75);
                grid.Columns[1].MaxWidth = 150;
                Frame(root);
                Check(grid.GetColumnWidth(1) == 150 && grid.GetColumnWidth(2) == 150);
                grid.Columns[1].Width = GridLength.Auto;
                var child = new MeasureProbe();
                grid.Children.Add(child);
                grid.Cells.Put(child, column: 1);
                Frame(root);
                Check(grid.GetColumnWidth(1) == 20 && grid.GetColumnWidth(2) == 280);
                grid.Columns[1].MinWidth = 50;
                Frame(root);
                Check(grid.GetColumnWidth(1) == 50 && grid.GetColumnWidth(2) == 250);
                int measurements = child.Measurements;
                for (int i = 0; i < 10; i++)
                    Frame(root);
                Check(child.Measurements == measurements, "Stable grid geometry must stay cached");
                Check(GridLength.Parse("2*") == GridLength.Proportion(2));
                var action = new Button();
                var deferred = new Grid();
                deferred.Children.Add(action);
                deferred.Cells.Put(action);
                var deferredRoot = new Workspace(deferred);
                var pending = new Button();
                action.Clicked += (_, _) =>
                {
                    deferred.Children.Add(pending);
                    deferred.Cells.Put(pending);
                    deferred.Cells.Put(pending);
                };
                Frame(deferredRoot);
                Click(deferredRoot);
                Check(pending.Parent == deferred && deferred.Cells.Count == 2, "Repeated Put must be safe during traversal");
            });
            Run("Equal-size live text preserves parent layout and processes changes during Update", () =>
            {
                var font = new CountingFont();
                var layout = new ControlLayoutContext(font);
                var frame = new ControlInputFrame(Size, TimeSpan.Zero);
                var panel = new TextLayoutProbePanel();
                var text = new TextBlock
                {
                    Text = "FPS: 40.0",
                    Padding = Insets2.Zero
                };
                panel.Add(text);
                using var root = new Workspace(panel);
                root.Update(frame, layout);
                int measured = panel.Measurements, arranged = panel.Arrangements, metrics = font.Measurements;
                Bounds2 bounds = text.Bounds;
                int notifications = 0;
                text.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(TextBlock.Text))
                        notifications++;
                };
                text.Text = "FPS: 60.0";
                Check(font.Measurements == metrics, "Property assignment must defer text preparation until Update");
                root.Update(frame, layout);
                Check(font.Measurements == metrics + 1 && notifications == 1);
                Check(panel.Measurements == measured && panel.Arrangements == arranged && text.Bounds.Equals(bounds), "Equal metrics must preserve parent layout");
                text.Text = "FPS: 120.0";
                root.Update(frame, layout);
                Check(panel.Measurements > measured && text.DesiredSize.Width == 80, "Changed width must propagate to the parent");
                measured = panel.Measurements;
                text.Text = "line one\nline two";
                root.Update(frame, layout);
                Check(panel.Measurements > measured && text.DesiredSize.Height == 32, "Changed height must propagate");
                text.Text = "different";
                font.Scale = 2;
                text.InvalidateLayout();
                root.Update(frame, layout);
                Check(text.DesiredSize.Width == 144 && text.DesiredSize.Height == 32, "External font invalidation must win over a pending text refresh");
                measured = panel.Measurements;
                text.Text = "123456789";
                text.WrapText = true;
                text.Width = 40;
                root.Update(frame, layout);
                Check(panel.Measurements > measured && text.DesiredSize.Height > 32, "Wrapping must retain full layout invalidation");
            });
            Run("Persistent layout and text caches with targeted invalidation", () =>
            {
                var font = new CountingFont();
                var layout = new ControlLayoutContext(font);
                var frame = new ControlInputFrame(Size, TimeSpan.Zero);
                var text = new TextBlock
                {
                    Text = "Cached",
                    Width = 160,
                    Padding = Insets2.Zero
                };
                using var root = new Workspace(text);
                root.Update(frame, layout);
                int initial = font.Measurements;
                Check(initial == 1);
                Bounds2 bounds = text.Bounds;
                for (int i = 0; i < 100; i++)
                    root.Update(frame, layout);
                Check(font.Measurements == initial && text.Bounds.Equals(bounds));
                text.Text = "Cached";
                text.Background = Color.Red;
                text.TextColor = Color.Green;
                text.Opacity = .5f;
                root.Update(frame, layout);
                Check(font.Measurements == initial, "Visual changes must preserve text metrics");
                text.Width = 200;
                text.TextHorizontalAlignment = ControlAlignment.End;
                root.Update(frame, layout);
                Check(font.Measurements == initial && text.Bounds.Width == 200, "Unwrapped text metrics must survive size and alignment changes");
                text.Text = "Changed";
                root.Update(frame, layout);
                Check(font.Measurements == ++initial);
                font.Scale = 2;
                text.InvalidateLayout();
                root.Update(frame, layout);
                Check(font.Measurements == ++initial && text.DesiredSize.Height == 32);
                var replacement = new CountingFont();
                root.Update(frame, new ControlLayoutContext(replacement));
                Check(replacement.Measurements == 1 && text.DesiredSize.Height == 16);
                text.WrapText = true;
                text.Text = "One two three four five six";
                text.Width = 64;
                root.Update(frame, layout);
                float tall = text.DesiredSize.Height;
                int wrapped = font.Measurements;
                for (int i = 0; i < 20; i++)
                    root.Update(frame, layout);
                Check(font.Measurements == wrapped);
                text.Width = 320;
                root.Update(frame, layout);
                Check(font.Measurements > wrapped && text.DesiredSize.Height < tall, "Wrapping must react to available width");
                var probe = new MeasureProbe();
                using var probeRoot = new Workspace(probe);
                probeRoot.Update(frame, layout);
                int measured = probe.Measurements, arranged = probe.Arrangements;
                for (int i = 0; i < 100; i++)
                    probeRoot.Update(frame, layout);
                Check(probe.Measurements == measured && probe.Arrangements == arranged);
                probe.Opacity = .25f;
                probeRoot.Update(frame, layout);
                Check(probe.Measurements == measured && probe.Arrangements == arranged);
                probeRoot.Update(new ControlInputFrame(new Size2(600, 350), TimeSpan.Zero), layout);
                Check(probe.Measurements > measured && probe.Arrangements > arranged);
            });
            Run("Frame-batch triangle clipping and conservative overlap bounds", () =>
            {
                var random = new Random(731);
                var clip = new Bounds2(0, 0, 10, 10);
                Span<Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture> result = stackalloc Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture[12];
                for (int sample = 0; sample < 200; sample++)
                {
                    Microsoft.Xna.Framework.Graphics.VertexPositionColorTexture Vertex()
                    {
                        float x = random.Next(-20, 30), y = random.Next(-20, 30);
                        return new(new Vector3(x, y, 0), Color.CornflowerBlue, new Vector2(x / 20, y / 20));
                    }

                    int count = ControlBatchGeometry.ClipTriangle(Vertex(), Vertex(), Vertex(), clip, result);
                    Check(count <= 7);
                    for (int i = 0; i < count; i++)
                    {
                        var v = result[i];
                        Check(v.Position.X >= -.0001f && v.Position.X <= 10.0001f && v.Position.Y >= -.0001f && v.Position.Y <= 10.0001f);
                        Check(MathF.Abs(v.TextureCoordinate.X - v.Position.X / 20) < .0001f && MathF.Abs(v.TextureCoordinate.Y - v.Position.Y / 20) < .0001f);
                        Check(v.Color == Color.CornflowerBlue);
                    }
                }

                Check(!ControlBatchGeometry.Overlaps(new Bounds2(0, 0, 10, 10), new Bounds2(10, 0, 10, 10)));
                Check(ControlBatchGeometry.Overlaps(new Bounds2(0, 0, 10, 10), new Bounds2(9, 9, 10, 10)));
                var union = ControlBatchGeometry.Union(new Bounds2(0, 0, 2, 2), new Bounds2(8, 7, 2, 3));
                Check(union.Equals(new Bounds2(0, 0, 10, 10)));
            });
            Run("Failed layout operations never become reusable cache entries", () =>
            {
                var probe = new MeasureProbe
                {
                    FailMeasure = true
                };
                using var root = new Workspace(probe);
                try
                {
                    Frame(root);
                    Check(false);
                }
                catch (InvalidOperationException e)when (e.Message == "Measure failure")
                {
                }

                probe.FailMeasure = false;
                Frame(root);
                Check(probe.Measurements == 2 && probe.Bounds.Width > 0);
                probe.FailArrange = true;
                probe.Width = 90;
                try
                {
                    Frame(root);
                    Check(false);
                }
                catch (InvalidOperationException e)when (e.Message == "Arrange failure")
                {
                }

                int before = probe.Arrangements;
                probe.FailArrange = false;
                Frame(root);
                Check(probe.Arrangements == before + 1 && probe.Bounds.Width == 90);
            });
            Run("Typed two-way MVVM applies at update boundaries", () =>
            {
                var model = new ValidationModel();
                var text = new TextBox();
                var root = new Workspace(text);
                using var binding = new PropertyBinding<ValidationModel, string>(model, nameof(model.Name), static m => m.Name, text, nameof(text.Text), s => text.Text = s, () => text.Text, static (m, s) => m.Name = s);
                root.Bindings.Add(binding);
                Frame(root);
                Check(text.Text == "Initial");
                model.Name = "Model";
                Check(text.Text == "Initial");
                Frame(root);
                Check(text.Text == "Model");
                text.Text = "View";
                Frame(root);
                Check(model.Name == "View");
            });
            Run("Content, tree, list, tabs and table collection ownership", () =>
            {
                var button = new Button
                {
                    Content = new TextBlock
                    {
                        Text = "Image or text"
                    }
                };
                var root = new Workspace(button);
                Frame(root);
                Check(button.Children.Count == 1);
                button.Content = new Image();
                Frame(root);
                Check(button.Children.Count == 1 && button.Children[0] is Image);
                var tree = new TreeView();
                var node = new TreeViewItem
                {
                    Text = "Workspace",
                    IsExpanded = true
                };
                node.Items.Add(new TreeViewItem { Text = "Child" });
                tree.Items.Add(node);
                var tr = new Workspace(tree);
                Frame(tr);
                Check(tree.VisibleCount == 2);
                node.IsExpanded = false;
                Frame(tr);
                Check(tree.VisibleCount == 1);
                var list = new ListBox();
                list.Items.Add(new SelectorItem("One"));
                list.Items.Add(new SelectorItem("Two"));
                var lr = new Workspace(list);
                Frame(lr);
                list.SelectedIndex = 1;
                Frame(lr);
                Check(list.SelectedIndex == 1);
                var tabs = new TabControl();
                var a = new TextBlock
                {
                    Text = "A"
                };
                var b = new TextBlock
                {
                    Text = "B"
                };
                tabs.Items.Add(new TabItem { Header = "A", Content = a });
                tabs.Items.Add(new TabItem { Header = "B", Content = b });
                var tabRoot = new Workspace(tabs);
                Frame(tabRoot);
                tabs.SelectedIndex = 1;
                Frame(tabRoot);
                Check(b.Workspace == tabRoot);
                var table = new DataTable();
                table.Items.Add("Row");
                Check(table.Rows.Count == 1);
            });
            Run("Tab switching refreshes draw order, hit testing and retained content", () =>
            {
                var actionA = new Button
                {
                    Content = new TextBlock
                    {
                        Text = "Page A"
                    }
                };
                var actionB = new Button
                {
                    Content = new TextBlock
                    {
                        Text = "Page B"
                    }
                };
                var first = new TabItem
                {
                    Header = "First",
                    Content = actionA
                };
                var second = new TabItem
                {
                    Header = "Second",
                    Content = actionB
                };
                var tabs = new TabControl();
                tabs.Items.Add(first);
                tabs.Items.Add(second);
                tabs.SelectedIndex = 0;
                using var root = new Workspace(tabs);
                Frame(root);
                Check(tabs.OrderedChildren.Contains(first));
                int switched = 0;
                tabs.SelectionChanged += (_, _) => switched++;
                tabs.SelectedIndex = 1;
                Frame(root);
                Check(tabs.OrderedChildren.Contains(second) && !tabs.OrderedChildren.Contains(first), "Draw order still contains the previous tab page");
                Check(second.Workspace == root && first.Workspace is null);
                Check(actionB.Bounds.Width > 0 && actionB.Bounds.Height > 0);
                Click(root, actionB.Bounds.X + actionB.Bounds.Width / 2, actionB.Bounds.Y + actionB.Bounds.Height / 2);
                Check(actionB.IsPressed && !actionA.IsPressed, "Selected page does not receive pointer input");
                for (int i = 0; i < 6; i++)
                {
                    tabs.SelectedIndex = i & 1;
                    Frame(root);
                    Check(tabs.OrderedChildren.Contains(tabs.Items[tabs.SelectedIndex]));
                    Check(tabs.Children.Count == 2);
                }

                Check(switched == 7);
                var headerPanel = (StackPanel)tabs.HeaderScrollViewer.Content!;
                var firstHeader = headerPanel.Children[0];
                Click(root, firstHeader.Bounds.X + firstHeader.Bounds.Width / 2, firstHeader.Bounds.Y + firstHeader.Bounds.Height / 2);
                Check(tabs.SelectedIndex == 0 && tabs.OrderedChildren.Contains(first));
                var secondHeader = headerPanel.Children[1];
                Click(root, secondHeader.Bounds.X + secondHeader.Bounds.Width / 2, secondHeader.Bounds.Y + secondHeader.Bounds.Height / 2);
                Check(tabs.SelectedIndex == 1 && tabs.OrderedChildren.Contains(second));
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Tab, Control: true));
                Check(tabs.SelectedIndex == 0 && tabs.OrderedChildren.Contains(first));
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Tab, Control: true, Shift: true));
                Check(tabs.SelectedIndex == 1 && tabs.OrderedChildren.Contains(second));
                var replacement = new TextBlock
                {
                    Text = "Replacement"
                };
                var host = new ContentControl
                {
                    Content = new TextBlock
                    {
                        Text = "Old"
                    }
                };
                using var contentRoot = new Workspace(host);
                Frame(contentRoot);
                Control old = host.OrderedChildren[0];
                host.Content = replacement;
                Frame(contentRoot);
                Check(host.OrderedChildren.Length == 1 && ReferenceEquals(host.OrderedChildren[0], replacement));
                Check(old.Workspace is null && replacement.Workspace == contentRoot);
                host.Content = null;
                Frame(contentRoot);
                Check(host.OrderedChildren.Length == 0 && replacement.Workspace is null);
            });
            Run("Content-only buttons, glyph spacing and selector alignment", () =>
            {
                var caption = new TextBlock
                {
                    Text = "Text"
                };
                var box = new CheckBox
                {
                    Content = caption,
                    Width = 150,
                    Height = 32
                };
                using var root = new Workspace(box);
                Frame(root);
                Check(caption.Parent == box && caption.Workspace == root);
                Check(caption.Bounds.X == box.Bounds.X + box.Padding.Left + box.GlyphSize.Width + box.TextSpacing);
                Check(box.Children.Count == 1);
                caption.Text = "Longer caption";
                Frame(root);
                Check(caption.DesiredSize.Width == Layout.DefaultFont.Measure(caption.Text).Width);
                var picture = new Image
                {
                    Width = 12,
                    Height = 12
                };
                box.Content = picture;
                Frame(root);
                Check(caption.Workspace is null && picture.Parent == box);
                Check(picture.Bounds.X == box.Bounds.X + box.Padding.Left + box.GlyphSize.Width + box.TextSpacing);
                var selectedText = new TextBlock
                {
                    Text = "Text"
                };
                var selector = new SelectorButton
                {
                    Content = selectedText,
                    Width = 100,
                    Height = 40
                };
                using var selectorRoot = new Workspace(selector);
                Frame(selectorRoot);
                Check(selectedText.Bounds.X == selector.Bounds.X + (selector.Bounds.Width - selectedText.DesiredSize.Width) / 2);
                Check(selectedText.Bounds.Y == selector.Bounds.Y + (selector.Bounds.Height - selectedText.DesiredSize.Height) / 2);
                var radioCaption = new TextBlock
                {
                    Text = "Radio"
                };
                var radio = new RadioButton
                {
                    Content = radioCaption,
                    Width = 150,
                    Height = 32
                };
                using var radioRoot = new Workspace(radio);
                radioRoot.Styles["RadioButton"].BasedOn = "Control";
                Frame(radioRoot);
                Check(radioCaption.Bounds.X == radio.Bounds.X + radio.GlyphSize.Width + radio.TextSpacing);
                var action = new Button
                {
                    Content = new TextBlock
                    {
                        Text = "Action"
                    },
                    Width = 100,
                    Height = 40
                };
                int activated = 0;
                action.Clicked += (_, _) => activated++;
                using var actionRoot = new Workspace(action);
                Frame(actionRoot);
                Click(actionRoot, 50, 20);
                Check(activated == 1 && action.IsPressed);
            });
            Run("Rotation, curves and independent touch joystick", () =>
            {
                var dial = new RotationControl
                {
                    Width = 160,
                    Height = 160,
                    HorizontalAlignment = ControlAlignment.Start,
                    VerticalAlignment = ControlAlignment.Start
                };
                var root = new Workspace(dial);
                Frame(root);
                dial.AngleDegrees = -90;
                Check(dial.AngleDegrees == 270);
                Vector2 c = dial.DialCenter;
                Frame(root, new ControlInputEvent(ControlInputEventType.PointerDown, c + new Vector2(50, 0)), new ControlInputEvent(ControlInputEventType.PointerMove, c + new Vector2(0, 50)));
                Check(dial.IsEditing && dial.AngleDegrees == 90);
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Escape));
                Check(!dial.IsEditing && dial.AngleDegrees == 270);
                var curve = new CurveControl();
                var cr = new Workspace(curve);
                Frame(cr);
                curve.AddPoint(new CurvePoint(.5f, .2f));
                Check(curve.EditableCurve!.Points.Count == 3);
                curve.Undo();
                Check(curve.EditableCurve.Points.Count == 2);
                Frame(cr);
                int external = 0;
                curve.CurveChanged += (_, _) => external++;
                curve.EditableCurve.AddPoint(new CurvePoint(.3f, .4f));
                Frame(cr);
                Check(external == 1, "Observable curve mutations invalidate layout");
                cr.Dispose();
                Check(curve.Workspace is null, "Disposed root detaches observable controls");
                var stick = new VirtualJoystick
                {
                    Width = 160,
                    Height = 160,
                    Radius = 50,
                    HorizontalAlignment = ControlAlignment.Start,
                    VerticalAlignment = ControlAlignment.Start
                };
                var sr = new Workspace(stick);
                Frame(sr);
                Frame(sr, P(ControlInputEventType.PointerDown, 80, 80, 7), P(ControlInputEventType.PointerMove, 110, 80, 7));
                Check(stick.IsActive && stick.Value.X > 0);
                Frame(sr, P(ControlInputEventType.PointerUp, 110, 80, 8));
                Check(stick.IsActive);
                Frame(sr, P(ControlInputEventType.PointerUp, 110, 80, 7));
                Check(!stick.IsActive && stick.Value == Vector2.Zero);
            });
            Run("Calendar, decimal spinner, password and dialogs", () =>
            {
                var calendar = new Calendar
                {
                    DisplayMonth = new DateOnly(2024, 2, 1)
                };
                Frame(new Workspace(calendar));
                Check(calendar.VisibleDates.Contains(new DateOnly(2024, 2, 29)));
                var number = new NumericSpinner
                {
                    NumericMinimum = -10,
                    NumericMaximum = 10,
                    NumericIncrement = .25m,
                    DecimalPlaces = 2
                };
                var nr = new Workspace(number);
                Frame(nr);
                number.Value = 2.25m;
                Frame(nr);
                Check(number.Value == 2.25m);
                var password = new PasswordBox
                {
                    Text = "Secret"
                };
                Frame(new Workspace(password));
                Check(password.IsPassword);
                var page = new Panel();
                var r = new Workspace(page);
                Frame(r);
                var dialog = new Dialog
                {
                    Content = new TextBlock
                    {
                        Text = "Modal"
                    }
                };
                var task = dialog.ShowAsync(r);
                Frame(r);
                Check(r.OverlayCount == 1);
                dialog.Complete(DialogResult.OK);
                Frame(r);
                Check(task.Result == DialogResult.OK && r.OverlayCount == 0);
            });
            Run("Immutable snapshots and reusable input sampling", () =>
            {
                var events = new System.Collections.Generic.List<ControlInputEvent>
                {
                    P(ControlInputEventType.PointerMove)
                };
                var frame = new ControlInputFrame(Size, TimeSpan.Zero, events);
                events.Clear();
                Check(frame.Events.Count == 1);
                var sampler = new ControlInputSampler();
                var viewport = new Rectangle(0, 0, 400, 300);
                var a = sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero);
                var b = sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero);
                Check(ReferenceEquals(a, b));
                var saved = sampler.Sample(default, default, viewport, 1, TimeSpan.Zero);
                Check(!ReferenceEquals(saved, a));
            });
            Run("Direct property writes avoid external Insets2 boxing", () =>
            {
                var button = new Button();
                for (int i = 0; i < 200; i++)
                {
                    button.Padding = new Insets2(i % 2);
                    button.Margin = new Insets2(i % 2);
                }

                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 10000; i++)
                {
                    button.Padding = new Insets2(i % 2);
                    button.Margin = new Insets2(i % 2);
                }

                long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
                Check(bytes == 0, "Insets property writes must not box");
                Console.WriteLine($"BENCHMARK: 20000 padding/margin writes: {bytes} bytes.");
            });
            Run("Steady input/update allocation benchmark", () =>
            {
                var panel = new StackPanel();
                for (int i = 0; i < 100; i++)
                    panel.Add(new Button { Content = new TextBlock { Text = "Button" }, Height = 20 });
                var root = new Workspace(panel);
                var sampler = new ControlInputSampler();
                var viewport = new Rectangle(0, 0, 400, 300);
                for (int i = 0; i < 200; i++)
                    root.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                long before = GC.GetAllocatedBytesForCurrentThread();
                var timer = System.Diagnostics.Stopwatch.StartNew();
                long timing = GC.GetAllocatedBytesForCurrentThread() - before;
                before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 10000; i++)
                    root.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
                timer.Stop();
                Console.WriteLine($"BENCHMARK: 100 buttons, 10000 reusable input/update frames: {bytes} bytes; {timer.Elapsed.TotalMilliseconds:0.0} ms.");
                Check(bytes == 0, "Steady input/update must allocate zero managed bytes");
            });
            Run("New controls: commands, expansion, numeric editing, charts and script languages", () =>
            {
                var split = new SplitButton
                {
                    Content = new TextBlock
                    {
                        Text = "Run"
                    },
                    CommandParameter = "payload",
                    Height = 40
                };
                object? parameter = null;
                bool enabled = true;
                split.Command = new Sachssoft.Engine.Input.Command(p => parameter = p, _ => enabled);
                var root = new Workspace(split);
                Frame(root);
                split.PrimaryButton.PerformClick();
                Check((string? )parameter == "payload");
                enabled = false;
                Frame(root);
                Check(!split.PrimaryButton.IsEnabled && split.DropDown.IsEnabled, "Dropdown must remain available when primary command is disabled");
                var menu = new ContextMenu();
                menu.Items.Add(new MenuItem { Text = "Option" });
                split.Menu = menu;
                split.DropDown.OpenMenu();
                Frame(root);
                Check(root.OverlayCount == 1);
                split.DropDown.OpenMenu();
                Frame(root);
                Check(root.OverlayCount == 1);
                var dropdown = new DropDownButton
                {
                    Menu = new ContextMenu()
                };
                dropdown.Menu.Items.Add(new MenuItem { Text = "Popup" });
                var dr = new Workspace(dropdown);
                Frame(dr);
                Click(dr);
                Check(dropdown.IsPressed && dropdown.WasOpened && dr.OverlayCount == 1);
                Frame(dr);
                Check(!dropdown.WasOpened);
                var child = new TextBlock
                {
                    Text = "Expanded content"
                };
                var expander = new Expander
                {
                    HeaderContent = new TextBlock
                    {
                        Text = "Details"
                    },
                    Content = child
                };
                var er = new Workspace(expander);
                Frame(er);
                float expanded = expander.DesiredSize.Height;
                Check(child.Workspace == er);
                expander.IsExpanded = false;
                Frame(er);
                Check(child.Workspace is null && expander.DesiredSize.Height < expanded);
                expander.IsExpanded = true;
                Frame(er);
                Check(child.Workspace == er);
                ((ToggleButton)expander.Header!).PerformClick();
                Frame(er);
                Check(!expander.IsExpanded && child.Workspace is null);
                var number = new ValueTextBox
                {
                    Minimum = -2,
                    Maximum = 2,
                    Step = .5f
                };
                var nr = new Workspace(number);
                Frame(nr);
                number.Value = 99;
                Check(number.Value == 2);
                number.Editor.Text = "-1.3";
                Check(number.CommitEdit() && number.Value == -1.5f);
                number.Editor.Text = "invalid";
                Check(!number.CommitEdit() && number.Value == -1.5f && number.Editor.HasError);
                bool rejected = false;
                try
                {
                    number.Value = float.NaN;
                }
                catch (ArgumentOutOfRangeException)
                {
                    rejected = true;
                }

                Check(rejected);
                number.BeginEdit();
                Frame(nr);
                number.Editor.Text = "1.2";
                Frame(nr);
                nr.Focus(null);
                Frame(nr);
                Check(number.Value == 1 && !number.Editor.HasError, "Focus loss must commit even when layout is cached");
                var series = new ChartSeries(4);
                for (int i = 0; i < 6; i++)
                    series.Append(i);
                Check(series.Count == 4 && series[0] == 2 && series[3] == 5);
                series.Clear();
                Check(series.Count == 0);
                var chart = new Chart
                {
                    AutoScale = false,
                    Maximum = 50
                };
                chart.Series.Add(series);
                var cr = new Workspace(chart);
                Frame(cr);
                series.Append(16);
                Frame(cr);
                chart.Kind = ChartKind.Bar;
                Frame(cr);
                chart.Kind = ChartKind.Step;
                Frame(cr);
                var sampler = new ControlInputSampler();
                var viewport = new Rectangle(0, 0, 400, 300);
                for (int i = 0; i < 100; i++)
                {
                    series.Append(i);
                    cr.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                }

                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 1000; i++)
                {
                    series.Append(i);
                    cr.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                }

                Check(GC.GetAllocatedBytesForCurrentThread() == before, "Appending and updating warmed chart must not allocate");
                chart.AutoScale = true;
                for (int i = 0; i < 100; i++)
                {
                    series.Append(i);
                    cr.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                }

                before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 1000; i++)
                {
                    series.Append(i);
                    cr.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                }

                Check(GC.GetAllocatedBytesForCurrentThread() == before, "Autoscaling chart must not allocate after warmup");
                var script = new ScriptTextBox
                {
                    Language = ScriptLanguage.Lua,
                    Text = "-- comment\nlocal value = 42\nlocal s = [[long\nstring]]"
                };
                var sr = new Workspace(script);
                Frame(sr);
                Check(script.Tokens.Any(t => t.Text == "local" && t.Kind == ScriptTokenKind.Keyword));
                Check(script.Tokens.Any(t => t.Kind == ScriptTokenKind.Number));
                Check(script.Tokens.Any(t => t.Line == 3 && t.Kind == ScriptTokenKind.String));
                var cached = script.Tokens;
                Frame(sr);
                Check(ReferenceEquals(cached, script.Tokens));
                script.Language = ScriptLanguage.Python;
                script.Text = "# comment\ndef run():\n    return \"\"\"multi\nline\"\"\"";
                Frame(sr);
                Check(script.Tokens.Any(t => t.Text == "def" && t.Kind == ScriptTokenKind.Keyword));
                Check(script.Tokens.Any(t => t.Line == 3 && t.Kind == ScriptTokenKind.String));
                script.Language = ScriptLanguage.CSharp;
                script.Text = "/* comment\ncontinued */ public int Value = 12;";
                Frame(sr);
                Check(script.Tokens.Any(t => t.Text == "public" && t.Kind == ScriptTokenKind.Keyword));
                Check(script.Tokens.Count(t => t.Kind == ScriptTokenKind.Comment) == 2);
                script.Language = ScriptLanguage.PlainText;
                Frame(sr);
                Check(script.Tokens.All(t => t.Kind == ScriptTokenKind.Text));
                var style = new Style();
                style["Language"] = "Lua";
                script.ApplyStyle(style);
                Check(script.Language == ScriptLanguage.PlainText);
                script.ResetStyleProperty("Language");
                Check(script.Language == ScriptLanguage.Lua);
                var dialog = new FolderSelectorDialog
                {
                    UseSystemDrives = false
                };
                dialog.Favorites.Clear();
                dialog.Favorites.Add(new FileDialogLocation("Test", "/virtual"));
                dialog.Drives.Add(new FileDialogLocation("Drive", "/drive"));
                dialog.RefreshLocations();
                Check(dialog.FavoritesList.Items.Count == 1 && dialog.DrivesList.Items.Count == 1);
                using var skinStream = System.IO.File.OpenRead(System.IO.Path.Combine(AppContext.BaseDirectory, "Skins", "SachssoftDark", "sachssoft_dark.skin.xml"));
                using var atlasStream = System.IO.File.OpenRead(System.IO.Path.Combine(AppContext.BaseDirectory, "Skins", "SachssoftDark", "sachssoft_dark.atlas.xml"));
                using var skin = ControlSkin.Read(skinStream, atlasStream);
                var skinned = new Workspace(new GroupBox());
                skinned.SetSkin(skin);
                Frame(skinned);
                Check(skinned.Registry.Styles["GroupBox"].Values.ContainsKey("HeaderHeight"));
            });
            Run("Custom syntax ranges, palette overrides and independent classification cache", () =>
            {
                var rules = new TestSyntaxHighlighter();
                rules.Spans.Add(new SyntaxSpan(4, 4, ScriptTokenKind.Keyword, Color.Magenta));
                var editor = new ScriptTextBox
                {
                    Text = "say move now\n\nend",
                    Highlighter = rules
                };
                var root = new Workspace(editor);
                Frame(root);
                Check(rules.Calls == 1);
                Check(string.Concat(editor.Tokens.Select(t => t.Text)) == "say move nowend", "Unclassified gaps must remain visible");
                Check(editor.Tokens.Any(t => t.Text == "move" && t.Color == Color.Magenta && t.Kind == ScriptTokenKind.Keyword));
                Check(editor.Tokens.Last().Line == 2 && editor.Tokens.Last().Start == 14);
                var cached = editor.Tokens;
                Frame(root);
                editor.Highlighter = rules;
                Frame(root);
                Check(rules.Calls == 1 && ReferenceEquals(cached, editor.Tokens));
                editor.Language = ScriptLanguage.Python;
                editor.KeywordColor = Color.Gold;
                Frame(root);
                Check(rules.Calls == 1, "Unused built-in language or palette changes must not reclassify custom syntax");
                var font = new CountingFont
                {
                    Scale = 2
                };
                editor.Font = font;
                Frame(root);
                Check(rules.Calls == 1 && editor.Tokens.First(t => t.Text == "move").Offset == 64, "Font changes must only remeasure cached ranges");
                rules.Spans[0] = new SyntaxSpan(4, 4, ScriptTokenKind.Number);
                editor.InvalidateHighlighting();
                Frame(root);
                Check(rules.Calls == 2 && editor.Tokens.Any(t => t.Text == "move" && t.Kind == ScriptTokenKind.Number && t.Color is null));
                editor.Text = "say move later\n\nend";
                Frame(root);
                Check(rules.Calls == 3);
                var replacement = new TestSyntaxHighlighter();
                replacement.Spans.Add(new SyntaxSpan(0, editor.Text.Length, ScriptTokenKind.Comment, Color.Green));
                editor.Highlighter = replacement;
                Frame(root);
                Check(replacement.Calls == 1 && editor.Tokens.All(t => t.Kind == ScriptTokenKind.Comment && t.Color == Color.Green));
                Check(editor.Tokens.Last().Line == 2, "Multiline ranges must be split at actual source line boundaries");
                replacement.Spans.Clear();
                replacement.Spans.Add(new SyntaxSpan(1000, 1));
                editor.InvalidateHighlighting();
                bool rejected = false;
                try
                {
                    Frame(root);
                }
                catch (InvalidOperationException)
                {
                    rejected = true;
                }

                Check(rejected, "Out-of-source ranges must be rejected");
                replacement.Spans.Clear();
                replacement.Spans.Add(new SyntaxSpan(1, 4));
                replacement.Spans.Add(new SyntaxSpan(2, 2));
                rejected = false;
                try
                {
                    Frame(root);
                }
                catch (InvalidOperationException)
                {
                    rejected = true;
                }

                Check(rejected, "Overlapping ranges must be rejected, and failed classification must retry");
                replacement.Spans.Clear();
                Frame(root);
                Check(editor.Tokens.All(t => t.Kind == ScriptTokenKind.Text), "An empty classifier must preserve all source text");
                editor.Highlighter = null;
                editor.Text = "def update():\n    return 42";
                Frame(root);
                Check(editor.Tokens.Any(t => t.Text == "def" && t.Kind == ScriptTokenKind.Keyword), "Null restores the chosen built-in language");
                Check(ReferenceEquals(ScriptSyntaxHighlighter.ForLanguage(ScriptLanguage.Lua), ScriptSyntaxHighlighter.ForLanguage(ScriptLanguage.Lua)));
                bool badLanguage = false;
                try
                {
                    ScriptSyntaxHighlighter.ForLanguage((ScriptLanguage)999);
                }
                catch (ArgumentOutOfRangeException)
                {
                    badLanguage = true;
                }

                Check(badLanguage);
                var unicode = new TestSyntaxHighlighter();
                unicode.Spans.Add(new SyntaxSpan(3, 4, ScriptTokenKind.Keyword));
                editor.Highlighter = unicode;
                editor.Text = "🎮 move\n";
                Frame(root);
                Check(editor.Tokens.Any(t => t.Text == "move" && t.Start == 3), "Offsets use UTF-16 source indices");
                for (int i = 0; i < 100; i++)
                    Frame(root);
                int calls = unicode.Calls;
                var sampler = new ControlInputSampler();
                var viewport = new Rectangle(0, 0, 400, 300);
                for (int i = 0; i < 100; i++)
                    root.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 1000; i++)
                    root.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                Check(GC.GetAllocatedBytesForCurrentThread() == before && unicode.Calls == calls, "Unchanged script updates must allocate zero bytes and skip classification");
            });
            Run("ValueTextBox click-versus-drag, repeat arrows and exclusive editing modes", () =>
            {
                var initialized = new ValueTextBox();
                using (var initialRoot = new Workspace(initialized))
                {
                    initialized.BeginEdit();
                    Frame(initialRoot);
                    Check(initialized.IsEditing && initialized.Editor.IsFocused);
                    initialized.CancelEdit();
                    Frame(initialRoot);
                    Check(!initialized.IsEditing);
                }

                var value = new ValueTextBox
                {
                    Minimum = 0,
                    Maximum = 100,
                    Step = 1,
                    Value = 50,
                    Width = 300,
                    Height = 36
                };
                var outside = new Button
                {
                    Content = new TextBlock
                    {
                        Text = "Outside"
                    },
                    Height = 36
                };
                var stack = new StackPanel();
                stack.Add(value);
                stack.Add(outside);
                var root = new Workspace(stack);
                Frame(root);
                float x = value.RangeBar.Bounds.X + value.RangeBar.Bounds.Width / 2, y = value.RangeBar.Bounds.Y + 18;
                Check(!value.IsEditing && !value.Editor.IsVisible && value.RangeBar.IsVisible);
                Check(value.DecreaseButton.Bounds.Right <= value.RangeBar.Bounds.X && value.RangeBar.Bounds.Right <= value.IncreaseButton.Bounds.X, "Arrows and center must share one horizontal row");
                Frame(root, P(ControlInputEventType.PointerDown, x, y));
                Check(value.Value == 50 && !value.IsEditing);
                Frame(root, P(ControlInputEventType.PointerMove, x + 2, y));
                Check(!value.RangeBar.IsDragging && value.Value == 50);
                Frame(root, P(ControlInputEventType.PointerUp, x + 2, y));
                Check(value.IsEditing && value.Editor.IsVisible && !value.RangeBar.IsVisible && root.FocusedControl == value.Editor);
                Check(value.Editor.SelectionLength == value.Editor.Text.Length, "Clicking must select the entire value for typing");
                value.Editor.Text = "72";
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter));
                Check(value.Value == 72 && !value.IsEditing && root.FocusedControl == value.RangeBar);
                value.Value = 50;
                Frame(root);
                Frame(root, P(ControlInputEventType.PointerDown, x, y), P(ControlInputEventType.PointerMove, x + 40, y));
                Check(value.RangeBar.IsDragging && !value.IsEditing && value.Value > 50);
                Frame(root, P(ControlInputEventType.PointerUp, x + 40, y));
                Check(!value.IsEditing && value.RangeBar.IsVisible && !value.RangeBar.IsDragging);
                value.Value = 50;
                Frame(root);
                Frame(root, P(ControlInputEventType.PointerDown, x, y), P(ControlInputEventType.PointerMove, x + 50, y), P(ControlInputEventType.PointerMove, x, y), P(ControlInputEventType.PointerUp, x, y));
                Check(value.Value == 50 && !value.IsEditing, "Returning a dragged thumb to its start must never become a click");
                Frame(root, P(ControlInputEventType.PointerDown, x, y), P(ControlInputEventType.PointerUp, x + 20, y));
                Check(value.Value > 50 && !value.IsEditing, "A moved release must count as dragging even without a move event");
                value.Value = 50;
                Frame(root);
                Click(root, value.RangeBar.Bounds.X + 4, y);
                Check(value.IsEditing && value.Value == 50, "A track click must open text without jumping to the clicked value");
                value.Editor.Text = "invalid";
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter));
                Check(value.IsEditing && value.Editor.HasError);
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Escape));
                Check(!value.IsEditing && value.Value == 50 && !value.Editor.HasError);
                Click(root, x, y);
                value.Editor.Text = "81";
                float observed = 0;
                outside.Clicked += (_, _) => observed = value.Value;
                Click(root, outside.Bounds.X + 10, outside.Bounds.Y + 10);
                Check(!value.IsEditing && value.Value == 81 && observed == 81, "Valid text must commit synchronously before an outside button activates");
                Click(root, x, y);
                value.Editor.Text = "invalid";
                Click(root, outside.Bounds.X + 10, outside.Bounds.Y + 10);
                Check(!value.IsEditing && value.Value == 81 && !value.Editor.HasError, "Invalid text must revert when focus leaves");
                value.Value = 50;
                Frame(root);
                float right = value.IncreaseButton.Bounds.X + value.IncreaseButton.Bounds.Width / 2;
                root.Update(new ControlInputFrame(Size, TimeSpan.FromSeconds(.6), new[] { P(ControlInputEventType.PointerDown, right, y) }), Layout);
                Check(value.Value > 51 && !value.IsEditing, "Holding the right arrow must repeat");
                Frame(root, P(ControlInputEventType.PointerUp, right, y));
                float released = value.Value;
                root.Update(new ControlInputFrame(Size, TimeSpan.FromSeconds(1)), Layout);
                Check(value.Value == released, "Repeating must stop after pointer release");
                value.Value = 100;
                Frame(root);
                Check(!value.IncreaseButton.IsEnabled && value.DecreaseButton.IsEnabled);
                value.Value = 0;
                Frame(root);
                Check(!value.DecreaseButton.IsEnabled && value.IncreaseButton.IsEnabled);
                value.Value = 50;
                Frame(root);
                root.Focus(value.RangeBar);
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.F2));
                Check(value.IsEditing);
                value.Editor.Text = "999";
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter));
                Check(value.Value == 100 && !value.IsEditing);
                value.Value = 50;
                Frame(root, P(ControlInputEventType.PointerDown, x, y), P(ControlInputEventType.PointerMove, x + 30, y));
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Escape));
                Check(value.Value == 50 && !value.IsEditing && !value.RangeBar.IsDragging);
                Frame(root, P(ControlInputEventType.PointerUp, x + 30, y));
                Check(!value.IsEditing);
                Frame(root, P(ControlInputEventType.PointerDown, x, y, id: 2), P(ControlInputEventType.PointerMove, x + 25, y, id: 2), P(ControlInputEventType.PointerUp, x + 25, y, id: 2));
                Check(value.Value > 50 && !value.IsEditing, "Touch dragging must remain in bar mode");
                Frame(root, P(ControlInputEventType.PointerDown, x, y), P(ControlInputEventType.PointerMove, x + 10, y));
                value.IsEnabled = false;
                Frame(root);
                Check(!value.RangeBar.IsDragging);
                value.IsEnabled = true;
                Frame(root, P(ControlInputEventType.PointerUp, x + 10, y));
                Check(!value.IsEditing);
                value.Minimum = 50;
                value.Maximum = 50;
                Frame(root);
                Check(value.Value == 50 && !value.IncreaseButton.IsEnabled && !value.DecreaseButton.IsEnabled);
                Frame(root, P(ControlInputEventType.PointerDown, x, y), P(ControlInputEventType.PointerMove, x + 50, y), P(ControlInputEventType.PointerUp, x + 50, y));
                Check(value.Value == 50 && !value.IsEditing);
                var style = new Style();
                style["ButtonWidth"] = "22";
                value.ApplyStyle(style);
                Frame(root);
                Check(value.DecreaseButton.Bounds.Width == 22);
                value.ButtonWidth = 26;
                value.ApplyStyle(style);
                Check(value.ButtonWidth == 26);
                value.ResetStyleProperty("ButtonWidth");
                Check(value.ButtonWidth == 22);
                var sampler = new ControlInputSampler();
                var viewport = new Rectangle(0, 0, 400, 300);
                for (int i = 0; i < 100; i++)
                    root.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 1000; i++)
                    root.Update(sampler.SampleReusable(default, default, default, viewport, 1, TimeSpan.Zero), Layout);
                Check(GC.GetAllocatedBytesForCurrentThread() == before, "Unchanged numeric fields must allocate zero managed bytes");
            });
            Run("Menu hover navigates without invoking commands, callbacks or check states", () =>
            {
                int commandCalls = 0, actionCalls = 0;
                var menu = new MenuBar();
                var file = new MenuItem
                {
                    Text = "File"
                };
                file.Items.Add(new MenuItem { Text = "Child" });
                var command = new MenuItem
                {
                    Text = "Run",
                    IsCheckable = true,
                    CommandParameter = "payload",
                    Command = new Sachssoft.Engine.Input.Command(p =>
                    {
                        Check((string? )p == "payload");
                        commandCalls++;
                    }),
                    Action = () => actionCalls++
                };
                var settings = new MenuItem
                {
                    Text = "Edit"
                };
                settings.Items.Add(new MenuItem { Text = "Other" });
                menu.Items.Add(file);
                menu.Items.Add(command);
                menu.Items.Add(settings);
                var root = new Workspace(menu);
                Frame(root);
                Frame(root, P(ControlInputEventType.PointerMove, 20, 12));
                Check(root.OverlayCount == 0 && commandCalls == 0);
                Click(root, 20, 12);
                Check(root.OverlayCount == 1);
                Frame(root, P(ControlInputEventType.PointerMove, 80, 12));
                Check(commandCalls == 0 && actionCalls == 0 && !command.IsChecked, "Hover over a sibling command must never activate it");
                Frame(root, P(ControlInputEventType.PointerMove, 20, 12));
                Click(root, 20, 12);
                Check(root.OverlayCount == 1);
                Frame(root, P(ControlInputEventType.PointerMove, 135, 12));
                Check(root.OverlayCount == 1 && commandCalls == 0, "Hover may switch an already-open top-level submenu");
                Frame(root, new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(80, 12), MouseButton.Right));
                Check(commandCalls == 0 && actionCalls == 0);
                Click(root, 80, 12);
                Check(commandCalls == 1 && actionCalls == 1 && command.IsChecked);
                var context = new ContextMenu();
                var branch = new MenuItem
                {
                    Text = "Branch"
                };
                branch.Items.Add(new MenuItem { Text = "Nested" });
                var leaf = new MenuItem
                {
                    Text = "Leaf",
                    IsCheckable = true,
                    Action = () => actionCalls++
                };
                context.Items.Add(branch);
                context.Items.Add(leaf);
                context.Show(root, new Vector2(10, 60));
                Frame(root);
                float cx = context.Bounds.X + context.Padding.Left + 10, cy = context.Bounds.Y + context.Padding.Top + 12;
                Frame(root, P(ControlInputEventType.PointerMove, cx, cy));
                Check(root.OverlayCount == 2, "Hovering a submenu entry may open its child popup");
                Frame(root, P(ControlInputEventType.PointerMove, cx, cy + 30));
                Check(actionCalls == 1 && !leaf.IsChecked && root.OverlayCount == 1, "Hovering a leaf closes the child popup without invoking the leaf");
                root.Focus(context);
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter));
                Check(actionCalls == 2 && leaf.IsChecked && root.OverlayCount == 0, "Explicit keyboard activation must still invoke the selected item");
            });
            Run("Skin collection metadata, safe resources and reversible palette switching", () =>
            {
                ControlSkin ReadSkin(string name)
                {
                    string folder = System.IO.Path.Combine(AppContext.BaseDirectory, "Skins", "Sachssoft" + name);
                    string stem = "sachssoft_" + name.ToLowerInvariant();
                    using var manifest = System.IO.File.OpenRead(System.IO.Path.Combine(folder, stem + ".skin.xml"));
                    using var atlas = System.IO.File.OpenRead(System.IO.Path.Combine(folder, stem + ".atlas.xml"));
                    return ControlSkin.Read(manifest, atlas);
                }

                var button = new Button
                {
                    Content = new TextBlock
                    {
                        Text = "Palette"
                    }
                };
                var root = new Workspace(button);
                var skins = new List<ControlSkin>();
                foreach (string name in new[]
                {
                    "Dark",
                    "Light",
                    "Modern",
                    "Futuristic",
                    "Medieval"
                }

                )
                {
                    ControlSkin skin = ReadSkin(name);
                    skins.Add(skin);
                    Check(skin.Name == "Sachssoft" + name && skin.RegionCount == 147);
                    Check(skin.AtlasFileName == "sachssoft_" + name.ToLowerInvariant() + ".atlas.xml");
                    Check(skin.TextureFileName == "sachssoft_" + name.ToLowerInvariant() + ".png");
                    Check(skin.TryGetColor("text", out Color text) && text == skin.GetColor("text"));
                    root.SetSkin(skin);
                    Frame(root);
                    Check(button.TextColor == text);
                    root.Registry.Styles["Button"]["Opacity"] = "0.85";
                }

                root.SetSkin(skins[0]);
                Frame(root);
                Check(button.TextColor == skins[0].GetColor("text"));
                Check(button.Opacity == .85f, "Application properties not declared by a new skin must survive theme switching");
                button.TextColor = Color.Magenta;
                foreach (var skin in skins)
                {
                    root.SetSkin(skin);
                    Frame(root);
                    Check(button.TextColor == Color.Magenta, "Local control assignments must take priority over every skin");
                }

                int styles = root.Registry.Styles.Count;
                for (int i = 0; i < 20; i++)
                {
                    root.SetSkin(skins[i % skins.Count]);
                    Frame(root);
                }

                Check(root.Registry.Styles.Count <= styles, "Switching must not accumulate styles");
                string manifestText = "<UiSkin name='Unsafe' atlas='../outside.xml'><Palette/><Regions/><Styles/></UiSkin>";
                string atlasText = "<TextureAtlas imagePath='safe.png' width='1' height='1'/>";
                bool rejected = false;
                using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(manifestText)))
                using (var ma = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(atlasText)))
                {
                    try
                    {
                        using var bad = ControlSkin.Read(ms, ma);
                    }
                    catch (System.IO.InvalidDataException)
                    {
                        rejected = true;
                    }
                }

                Check(rejected, "Atlas traversal must be rejected before resource loading");
                manifestText = "<UiSkin name='Unsafe' atlas='safe.atlas.xml'><Palette/><Regions/><Styles/></UiSkin>";
                atlasText = "<TextureAtlas imagePath='../outside.png' width='1' height='1'/>";
                rejected = false;
                using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(manifestText)))
                using (var ma = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(atlasText)))
                {
                    try
                    {
                        using var bad = ControlSkin.Read(ms, ma);
                    }
                    catch (System.IO.InvalidDataException)
                    {
                        rejected = true;
                    }
                }

                Check(rejected, "Texture traversal must be rejected");
                foreach (var skin in skins)
                    skin.Dispose();
                root.Dispose();
            });
            Run("Scene viewport layout and deferred color picking", () => _checks += SceneViewportTests.Run());
            Run("Central font families, inheritance and adapter cache", () => _checks += FontControlChecks.Run());
            Run("UI-independent inspector view contract", () => _checks += InspectorViewContractTests.Run());
            Run("Responsive inspector property rows", () => _checks += InspectorFieldLayoutTests.Run());
            if (args.Length == 2 && args[0] == "--font")
                Run("Real engine font metrics", () =>
                {
                    using var font = new FontFaceControlFont(new Sachssoft.Engine.Graphics.Text.FontFace(System.IO.File.ReadAllBytes(args[1]), "Test"), 20);
                    Check(font.Measure("Sasogine").Width > 0);
                });
            Console.WriteLine($"PASS: {_groups} groups, {_checks} assertions; {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
