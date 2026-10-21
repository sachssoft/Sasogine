using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Assets.Graphics;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Checks tiled selection, lazy previews, typed references, drag/drop and pipette input isolation.</summary>
internal static class AssetControlsChecks
{
    internal static int Run(ControlLayoutContext layout)
    {
        int checks = 0, loads = 0;
        void Check(bool value, string message)
        {
            checks++;
            if (!value)
                throw new InvalidOperationException(message);
        }

        var entries = new AssetItem[100];
        for (int i = 0; i < entries.Length; i++)
            entries[i] = new AssetItem(new Texture2DAsset("texture." + i), "Texture " + i.ToString("00"), "Texture", () =>
            {
                loads++;
                return null;
            });
        var other = new AssetItem(new AssetExampleObject("entity.1"), "Entity", "Entity");
        var all = entries.Append(other).ToArray();
        var assets = new AssetView
        {
            Source = entries,
            MultiSelect = true
        };
        var picker = new ReferencePicker<Texture2DAsset>
        {
            Source = all
        };
        var root = new Grid
        {
            Spacing = 8
        };
        root.Rows.Add(new GridRow { Height = new GridLength(260) });
        root.Rows.Add(new GridRow { Height = new GridLength(40) });
        root.Children.Add(assets);
        root.Cells.Put(assets);
        root.Children.Add(picker);
        root.Cells.Put(picker, 1);
        using var workspace = new Workspace(root);
        void Frame(params ControlInputEvent[] events) => workspace.Update(new ControlInputFrame(new Size2(500, 340), TimeSpan.FromSeconds(.016), events), layout);
        void Click(Vector2 at, bool shift = false, bool control = false) => Frame(new ControlInputEvent(ControlInputEventType.PointerDown, at, Shift: shift, Control: control), new ControlInputEvent(ControlInputEventType.PointerUp, at, Shift: shift, Control: control));
        static Vector2 Center(Bounds2 b) => new(b.X + b.Width / 2, b.Y + b.Height / 2);
        Frame();
        Frame();
        Check(loads > 0 && loads < 20, "Only visible asset previews are requested");
        int initialLoads = loads;
        Frame();
        Frame();
        Check(loads == initialLoads, "Absent previews are cached without repeated loading");
        Check(assets.GetItemBounds(1).X > assets.GetItemBounds(0).X, "Tiles share TiledPanel column geometry");
        assets.SelectedIndex = 0;
        workspace.Focus(assets);
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Right));
        Check(assets.SelectedIndex == 1, "Right arrow advances one tile");
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Down));
        Check(assets.SelectedIndex > 2 && assets.GetItemBounds(assets.SelectedIndex).Y > assets.GetItemBounds(1).Y, "Down arrow advances one tile row");
        Click(Center(assets.GetItemBounds(0)));
        Click(Center(assets.GetItemBounds(4)), shift: true);
        Check(assets.SelectedIndices.Count == 5, "Shift selects a contiguous asset range");
        assets.ScrollBar.Value = assets.ScrollBar.Maximum;
        Frame();
        Check(loads > initialLoads && loads < entries.Length, "Scrolling requests the newly visible previews only");
        assets.FilterText = "Texture 00";
        Frame();
        Check(assets.Items.Count == 1 && assets.Items[0].Value == entries[0], "Filtering searches metadata without loading every asset");
        assets.FilterText = "";
        assets.ViewMode = AssetViewMode.List;
        Frame();
        Check(assets.GetItemBounds(0).X == assets.GetItemBounds(1).X && assets.GetItemBounds(1).Y > assets.GetItemBounds(0).Y, "List view uses the same selectable list");
        assets.ViewMode = AssetViewMode.Tiles;
        assets.ScrollBar.Value = 0;
        Frame();
        int failedLoads = 0;
        var failed = new AssetItem(new AssetExampleObject("failed"), "Failed", preview: () =>
        {
            failedLoads++;
            throw new IOException("Preview unavailable");
        });
        failed.RequestPreview();
        failed.RequestPreview();
        Check(failedLoads == 1 && failed.PreviewError is IOException, "Preview failures are cached without breaking the view");
        failed.InvalidatePreview();
        failed.RequestPreview();
        Check(failedLoads == 2, "Explicit invalidation retries a failed preview");
        picker.Value = new Reference<Texture2DAsset>("missing");
        Check(!picker.IsResolved && picker.Identifier.Text.Contains("missing"), "Missing references remain visible and retain their identifiers");
        Check(!picker.CanSelectAsset(other) && picker.CanSelectAsset(entries[0]), "Reference filters use the actual target type");
        bool rejected = false;
        try
        {
            picker.SelectAsset(other);
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        Check(rejected && picker.ReferenceId == "missing", "Incompatible selections preserve the previous reference");
        var shared = new Reference<Texture2DAsset>(entries[0].Id);
        picker.Value = shared;
        picker.SelectAsset(entries[1]);
        Check(shared.Id == entries[0].Id && picker.Value?.Id == entries[1].Id, "User selection replaces a reference without mutating shared objects");
        picker.AllowClear = false;
        picker.ClearReference();
        Check(picker.Value?.Id == entries[1].Id, "Required reference fields reject clearing");
        picker.AllowClear = true;
        picker.ClearReference();
        Check(picker.Value is null, "Optional references can be cleared");
        picker.Value = new Reference<Texture2DAsset>(entries[2].Id);
        picker.Value.Id = entries[3].Id;
        Frame();
        Check(picker.ReferenceId == entries[3].Id, "External mutation of Reference.Id refreshes the field");
        picker.Browse();
        Frame();
        var dialog = picker.ActiveDialog!;
        Check(dialog.Assets.Items.Count == entries.Length, "Browse dialog contains compatible assets only");
        dialog.Assets.SelectedIndex = 0;
        dialog.Complete(DialogResult.Cancel);
        Frame();
        Check(picker.ReferenceId == entries[3].Id && picker.ActiveDialog is null, "Dialog cancellation preserves the reference");
        picker.Browse();
        Frame();
        dialog = picker.ActiveDialog!;
        dialog.Assets.SelectedIndex = 1;
        dialog.AcceptButton.PerformClick();
        Frame();
        Check(picker.ReferenceId == entries[1].Id && workspace.OverlayCount == 0, "Dialog acceptance commits the selected reference");
        assets.ScrollBar.Value = 0;
        Frame();
        Vector2 start = Center(assets.GetItemBounds(0));
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, start), new ControlInputEvent(ControlInputEventType.PointerMove, start + new Vector2(18, 0)));
        Check(workspace.AssetDragging.Payload == entries[0], "Dragging a tile starts an asset payload");
        Frame(new ControlInputEvent(ControlInputEventType.PointerUp, Center(picker.Bounds)));
        Check(picker.ReferenceId == entries[0].Id && workspace.AssetDragging.Payload is null && workspace.CapturedControl is null, "Dropping onto a compatible reference selects it and releases capture");
        Vector2 gap = new(assets.GetItemBounds(2).Right + 3, assets.GetItemBounds(2).Y + 10);
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, gap), new ControlInputEvent(ControlInputEventType.PointerMove, gap + new Vector2(10, 0)));
        Check(workspace.AssetDragging.Payload is null, "Dragging tile spacing does not drag the previous selection");
        Frame(new ControlInputEvent(ControlInputEventType.PointerUp, gap));
        var probe = new InspectorColorProbe();
        Color original = new(10, 20, 30, 80), current = original;
        int accepted = 0, cancelled = 0;
        void Begin() => workspace.ColorPicking.Begin(picker, probe, original, color => current = color, color =>
        {
            current = color;
            accepted++;
        }, () =>
        {
            current = original;
            cancelled++;
        });
        Frame();
        Check(probe.Samples == 0, "Inactive pipettes never sample the source");
        Begin();
        Frame(new ControlInputEvent(ControlInputEventType.PointerMove, new Vector2(10, 10)), new ControlInputEvent(ControlInputEventType.PointerMove, new Vector2(20, 20)));
        Check(probe.Samples == 1 && current.R == 255 && current.A == original.A, "Pointer previews are coalesced and preserve the original alpha");
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Escape));
        Check(!workspace.ColorPicking.IsActive && current == original && cancelled == 1, "Escape restores the original color exactly once");
        Begin();
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, Center(picker.BrowseButton.Bounds)));
        Check(accepted == 1 && picker.ActiveDialog is null && !workspace.ColorPicking.IsActive, "Accepting a color consumes the click instead of opening underlying controls");
        Begin();
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(-1, -1)));
        Check(workspace.ColorPicking.IsActive && accepted == 1, "An invalid sample cannot commit a stale preview");
        Frame(new ControlInputEvent(ControlInputEventType.Deactivate));
        Check(!workspace.ColorPicking.IsActive && cancelled == 2 && current == original, "Deactivation cancels and rolls back picking");
        workspace.ColorPickSource = probe;
        var colorDialog = new ColorPickerDialog
        {
            SelectedColor = original
        };
        _ = workspace.ShowDialog(colorDialog);
        Frame();
        Check(colorDialog.PickButton.IsEnabled, "Color dialogs discover the workspace render-target source");
        colorDialog.BeginColorPick();
        Frame(new ControlInputEvent(ControlInputEventType.PointerMove, new Vector2(10, 10)));
        Check(colorDialog.SelectedColor.R == 255 && colorDialog.SelectedColor.A == original.A, "Pipette preview reaches the color dialog");
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Escape));
        Check(colorDialog.IsOpen && colorDialog.SelectedColor == original, "Escape cancels the pipette while keeping its color dialog open");
        colorDialog.BeginColorPick();
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(10, 10)));
        Check(!workspace.ColorPicking.IsActive && colorDialog.IsOpen && colorDialog.SelectedColor.R == 255, "A picked color returns to the open color dialog for final confirmation");
        colorDialog.Complete(DialogResult.Cancel);
        Frame();
        Check(workspace.OverlayCount == 0, "Color picking leaves no stale modal overlays");
        // Check style/local precedence independently of the current skin.
        assets.TileSize = new Size2(150, 140);
        var style = new Style();
        style["TileSize"] = "90, 80";
        assets.ApplyStyle(style);
        Check(assets.TileSize == new Size2(150, 140), "Asset tile styles preserve local overrides");
        var steady = new ControlInputFrame(new Size2(500, 340), TimeSpan.FromSeconds(.016));
        for (int i = 0; i < 32; i++)
            workspace.Update(steady, layout);
        int cachedLoads = loads;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            workspace.Update(steady, layout);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(loads == cachedLoads, "Steady asset updates do not rebuild previews");
        Check(allocated == 0, "Steady AssetView and ReferencePicker updates must not allocate: " + allocated);
        Console.WriteLine($"BENCHMARK: 1000 steady asset-view/reference updates: {allocated} bytes.");
        return checks;
    }
}
