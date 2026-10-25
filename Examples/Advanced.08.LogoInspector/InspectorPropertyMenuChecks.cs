using Microsoft.Xna.Framework;
using Sachssoft.Engine.Assets.Graphics;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Checks complete-value reset, typed clipboard operations and stale-menu lifetime.</summary>
internal static class InspectorPropertyMenuChecks
{
    internal static int Run(ControlLayoutContext layout)
    {
        int checks = 0;
        void Check(bool value, string message) { checks++; if (!value) throw new InvalidOperationException(message); }
        Vector2 a = new(3, 4), b = new(7, 8);
        int number = 25;
        var inspector = new InspectorView();
        Control first = inspector.Add("First vector", () => a, value => a = value);
        Control second = inspector.Add("Second vector", () => b, value => b = value);
        Control integer = inspector.Add("Integer", () => number, value => number = value);
        var asset = new Texture2DAsset("texture.default");
        var assets = new[] { new AssetItem(asset, "Default texture") };
        Reference<Texture2DAsset>? reference = new(asset.Id), otherReference = null;
        Control refEditor = inspector.AddReference("Reference", () => reference, value => reference = value, assets);
        Control otherRefEditor = inspector.AddReference("Other reference", () => otherReference, value => otherReference = value, assets);
        var styled = new Button { StyleId = "Menu.ResetStyle", Padding = new Insets2(8) };
        inspector.AddField("Style override", styled);
        using var workspace = new Workspace(inspector);
        workspace.Styles.Register("Menu.ResetStyle", new Style { BasedOn = "Button", Padding = new Insets2(2, 3) });
        var frame = new ControlInputFrame(new Size2(720, 600), TimeSpan.FromSeconds(.016));
        void Frame() => workspace.Update(frame, layout);
        Frame(); Frame();
        var firstMenu = inspector.GetPropertyMenu(first);
        var secondMenu = inspector.GetPropertyMenu(second);
        var intMenu = inspector.GetPropertyMenu(integer);
        Check(firstMenu.Bounds.X >= first.Bounds.Right && firstMenu.Bounds.Width == 26, "Every property has a trailing menu column");
        Check(!firstMenu.Menu.UseSkin, "Inspector popups use solid backgrounds without atlas skin parts");
        float originalWidth = first.Bounds.Width;
        int visibilityChanges = 0;
        inspector.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(InspectorView.ShowPropertyMenus)) visibilityChanges++; };
        firstMenu.PerformClick(); Frame();
        inspector.ShowPropertyMenus = false; Frame();
        Check(!firstMenu.IsVisible && first.Bounds.Width > originalWidth + 30 && workspace.OverlayCount == 0,
            "Hiding property menus removes their column and closes existing popups");
        firstMenu.PerformClick(); Frame();
        Check(workspace.OverlayCount == 0, "Hidden property buttons cannot open popups");
        float extra = 1;
        Control extraEditor = inspector.Add("Added while hidden", () => extra, value => extra = value);
        var group = inspector.AddGroup("Nested visibility");
        Control nested = group.Fields.Add("Nested value", () => extra, value => extra = value);
        Frame();
        Check(!inspector.GetPropertyMenu(extraEditor).IsVisible && !group.Fields.ShowPropertyMenus && !group.Fields.GetPropertyMenu(nested).IsVisible,
            "New fields and nested inspector groups inherit hidden menu visibility");
        inspector.ShowPropertyMenus = true; Frame();
        Check(firstMenu.IsVisible && MathF.Abs(first.Bounds.Width - originalWidth) < .01f && group.Fields.GetPropertyMenu(nested).IsVisible && visibilityChanges == 2,
            "Showing menus restores the original layout and raises change notifications");
        group.Fields.ShowPropertyMenus = false;
        inspector.ShowPropertyMenus = false; inspector.ShowPropertyMenus = true; Frame();
        Check(!group.Fields.GetPropertyMenu(nested).IsVisible, "A nested inspector can explicitly override inherited menu visibility");
        group.Fields.ShowPropertyMenus = true; Frame();
        a = new Vector2(9, 10); Frame(); firstMenu.ResetValue();
        Check(a == new Vector2(3, 4), "Default reset restores the captured complete value");
        inspector.SetDefault(first, Vector2.Zero); a = new Vector2(12, 24); firstMenu.ResetValue();
        Check(a == Vector2.Zero, "An explicit property default replaces the initial snapshot");
        a = new Vector2(16, 32); Frame(); firstMenu.CopyValue();
        Check(secondMenu.CanPaste && !intMenu.CanPaste, "Paste availability uses the exact property type");
        secondMenu.PasteValue(); intMenu.PasteValue();
        Check(b == a && number == 25, "Paste copies the complete vector and rejects incompatible numbers");
        bool rejectedDefault = false;
        try { inspector.SetDefault(first, 1); } catch (ArgumentException) { rejectedDefault = true; }
        Check(rejectedDefault, "Incorrect default types are rejected without reflection");
        var refMenu = inspector.GetPropertyMenu(refEditor);
        var otherRefMenu = inspector.GetPropertyMenu(otherRefEditor);
        reference!.Id = "mutated"; Frame(); refMenu.ResetValue();
        Check(reference?.Id == asset.Id, "Reference reset snapshots the identifier instead of a mutable reference object");
        refMenu.CopyValue(); reference!.Id = "changed after copy";
        otherRefMenu.PasteValue();
        Check(otherReference?.Id == asset.Id, "Reference clipboard values do not share the source reference instance");
        otherReference!.Id = "changed after paste"; otherRefMenu.PasteValue();
        Check(otherReference.Id == asset.Id, "Repeated reference pastes produce independent reference objects");
        var styleMenu = inspector.GetPropertyMenu(styled);
        styleMenu.ResetAction = () => styled.ResetStyleProperty(nameof(Control.Padding));
        styleMenu.ResetValue(); Frame();
        Check(styled.Padding.Equals(new Insets2(2, 3)), "Style reset removes the local override and restores the central style");
        firstMenu.PerformClick(); Frame();
        Check(workspace.OverlayCount == 1 && ((MenuItem)firstMenu.Menu.Items[0]).IsEnabled && ((MenuItem)firstMenu.Menu.Items[2]).IsEnabled == firstMenu.CanPaste,
            "Property buttons open owned menus with current command availability");
        Bounds2 menuBounds = firstMenu.Menu.Bounds;
        var click = new Vector2(menuBounds.X + firstMenu.Menu.Padding.Left + 15, menuBounds.Y + firstMenu.Menu.Padding.Top + 12);
        a = new Vector2(42, 43);
        workspace.Update(new ControlInputFrame(new Size2(720, 600), TimeSpan.FromSeconds(.016), new[] {
            new ControlInputEvent(ControlInputEventType.PointerDown, click), new ControlInputEvent(ControlInputEventType.PointerUp, click) }), layout);
        Check(a == Vector2.Zero && workspace.OverlayCount == 0, "Clicking Reset restores the value and closes the menu");
        for (int i = 0; i < 32; i++) Frame();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) Frame();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(allocated == 0, "Unchanged property menus must not allocate: " + allocated);
        Console.WriteLine($"BENCHMARK: 1000 steady property-menu updates: {allocated} bytes.");
        firstMenu.PerformClick(); Frame();
        inspector.Clear(); Frame();
        a = new Vector2(80, 90); firstMenu.ResetValue(); firstMenu.PasteValue();
        Check(a == new Vector2(80, 90) && !firstMenu.CanReset && workspace.OverlayCount == 0 && workspace.Bindings.Count == 0,
            "Removed property menus cannot modify stale models or retain popups and bindings");
        return checks;
    }
}
