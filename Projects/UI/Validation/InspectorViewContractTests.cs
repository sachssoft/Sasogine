using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Components.Definitions.Inspectors;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Inspection;

/// <summary>Validates the Basic inspector contract against the concrete Controls implementation.</summary>
internal static class InspectorViewContractTests
{
    internal static int Run()
    {
        int checks = 0;
        void Check(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
            checks++;
        }
        void Reject<T>(Action action) where T : Exception
        {
            try { action(); }
            catch (T) { checks++; return; }
            throw new InvalidOperationException("Expected " + typeof(T).Name);
        }

        var registry = new InspectorRegistry();
        registry.Register<InspectorViewContractDefinition>(static () => new InspectorViewContractDescription());
        var definition = new InspectorViewContractDefinition();
        var view = new InspectorView();
        IInspectorView contract = view;
        registry.Build(contract, definition);
        var group = view.Group("Common", "Common");
        Check(ReferenceEquals(group, contract.Group("Common", "Allgemein")), "Group identity must use the stable name.");
        var title = (TextBox)group.GetFieldEditor(nameof(definition.Title));
        var count = (TypedValueTextBox<int>)group.GetFieldEditor(nameof(definition.Count));
        var readOnly = (TypedValueTextBox<int>)group.GetFieldEditor(nameof(definition.ReadOnlyCount));
        var color = group.GetFieldEditor(nameof(definition.Color));
        var vector = group.GetFieldEditor(nameof(definition.Position));
        Check(!readOnly.IsEnabled && readOnly.IsReadOnly, "Read-only numeric fields must reject user editing.");
        Check(!color.IsEnabled && !vector.IsEnabled, "Color and composite editors must be disabled without a setter.");
        Reject<ArgumentException>(() => contract.Group(" ", "Group"));
        Reject<ArgumentException>(() => contract.Add(" ", "Field", () => 0));
        Reject<ArgumentNullException>(() => contract.Add<int>("NullRead", "Field", null!));
        Reject<ArgumentException>(() => ((IInspectorView)group).Add(nameof(definition.Count), "Duplicate", () => 0));

        var layout = new ControlLayoutContext(new TestFont());
        var frame = new ControlInputFrame(new Size2(800, 700), TimeSpan.FromSeconds(.016));
        using (var workspace = new Workspace(view))
        {
            workspace.Update(frame, layout);
            Check(workspace.Bindings.Count == 5, "A description must attach each binding once.");
            title.Text = "Updated";
            count.Text = "12";
            Check(count.CommitEdit() && definition.Count == 12 && definition.Title == "Updated", "Typed fields must write to the definition.");
            definition.Title = "External";
            definition.Count = 21;
            definition.ReadOnlyCount = 31;
            definition.Color = Color.Red;
            definition.Position = new Vector2(8, 9);
            workspace.Update(frame, layout);
            Check(title.Text == "External" && count.Value == 21 && readOnly.Value == 31, "Mutable and read-only values must refresh from the source.");

            var menu = group.GetPropertyMenu(readOnly);
            Check(menu.CanCopy && !menu.CanReset, "A read-only field must allow Copy but deny Reset.");
            menu.CopyValue();
            Check(workspace.Registry.InspectorClipboard.TryGet<int>(out int copied) && copied == 31, "Copy must retain the exact source value type.");
            workspace.Registry.InspectorClipboard.Copy(99);
            bool resetCalled = false;
            menu.ResetAction = () => resetCalled = true;
            menu.ResetValue();
            menu.PasteValue();
            Check(!menu.CanReset && !menu.CanPaste && !resetCalled && definition.ReadOnlyCount == 31, "Read-only menu actions must never mutate the model.");
            readOnly.Text = "44";
            Check(!readOnly.CommitEdit() && definition.ReadOnlyCount == 31, "Read-only text must not commit edits.");
            readOnly.CancelEdit();

            var editableMenu = group.GetPropertyMenu(count);
            editableMenu.CopyValue();
            Check(editableMenu.CanReset && editableMenu.CanPaste, "Writable fields must retain their normal property actions.");
            var other = (InspectorView)contract.Group("Other", "Other");
            ((IInspectorView)other).Add(nameof(definition.Count), "Other count", () => definition.Count);
            workspace.Update(frame, layout);
            Check(other.GetFieldEditor(nameof(definition.Count)) is TypedValueTextBox<int>, "Field names must be scoped to their group.");
            for (int i = 0; i < 50; i++) workspace.Update(frame, layout);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) workspace.Update(frame, layout);
            Check(GC.GetAllocatedBytesForCurrentThread() == before, "Unchanged interface-based inspector updates must not allocate.");

            view.Clear();
            Check(workspace.Bindings.Count == 0 && !menu.CanCopy, "Clear must detach fields, bindings and stale property menus.");
            registry.Build(contract, definition);
            Check(!ReferenceEquals(group, view.Group("Common", "Common")), "Clear must release named group identities.");
            workspace.Update(frame, layout);
            Check(workspace.Bindings.Count == 5, "Rebuilding a cleared description must not leak bindings.");
        }

        var editors = new InspectorEditorRegistry();
        int customCalls = 0;
        editors.Register<string>("Custom text", (fields, label, read, write, source, name) =>
        {
            customCalls++;
            return fields.AddText(label, read, write, source, name);
        });
        var customView = new InspectorView(editors);
        ((IInspectorView)customView).Group("Custom", "Custom").Add("Text", "Text", () => "Value");
        Check(customCalls == 1, "Named groups must inherit the parent's custom editor registry.");

        var legacy = new InspectorView();
        legacy.AddText("Legacy", () => definition.Title, value => definition.Title = value);
        legacy.AddGroup("Legacy group").Fields.AddNumber("Legacy number", () => definition.Count, value => definition.Count = value);
        using (var workspace = new Workspace(legacy))
        {
            workspace.Update(frame, layout);
            Check(workspace.Bindings.Count == 2, "Existing direct editor APIs must remain compatible.");
        }
        return checks;
    }
}
