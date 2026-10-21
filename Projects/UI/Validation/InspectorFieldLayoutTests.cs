using System;
using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Inspection;

internal static class InspectorFieldLayoutTests
{
    internal static int Run()
    {
        int checks = 0;
        void Check(bool ok, string message)
        {
            checks++;
            if (!ok) throw new InvalidOperationException(message);
        }
        var view = new InspectorView();
        string text = "A deliberately long editable value that must scroll inside its field";
        var editor = view.AddText("A long property label that needs wrapping", () => text, value => text = value);
        var vector = view.AddVector3("Position", () => Vector3.Zero, _ => { }, -100, 100);
        using var workspace = new Workspace(view);
        var layout = new ControlLayoutContext(new TestFont());
        foreach (int width in new[] { 260, 340, 600 })
        {
            var frame = new ControlInputFrame(new Size2(width, 300), TimeSpan.FromSeconds(.016));
            workspace.Update(frame, layout);
            var row = (ContainerControl)editor.Parent!;
            var label = row.Children[0];
            var menu = view.GetPropertyMenu(editor);
            Check(editor.Bounds.Left >= 0 && editor.Bounds.Right <= width, "Editor stays inside the available width.");
            Check(editor.Bounds.Right <= menu.Bounds.Left, "Property menu must not overlap its editor.");
            Check(menu.Bounds.Right <= width, "Property menu stays inside the available width.");
            Check(width < 420 ? label.Bounds.Bottom < editor.Bounds.Top : label.Bounds.Right < editor.Bounds.Left,
                "Labels stack only in narrow property rows.");
            foreach (var component in vector.Editors)
                Check(component.Bounds.Left >= vector.Bounds.Left && component.Bounds.Right <= vector.Bounds.Right,
                    "Composite editors stay inside their row.");
            float previous = editor.Bounds.Width;
            view.ShowPropertyMenus = false;
            workspace.Update(frame, layout);
            Check(!menu.IsVisible && editor.Bounds.Width > previous, "Hidden menus release their column width.");
            view.ShowPropertyMenus = true;
            workspace.Update(frame, layout);
            Check(menu.IsVisible, "Property menu can be restored.");
        }
        var steady = new ControlInputFrame(new Size2(340, 300), TimeSpan.FromSeconds(.016));
        for (int i = 0; i < 100; i++) workspace.Update(steady, layout);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) workspace.Update(steady, layout);
        Check(GC.GetAllocatedBytesForCurrentThread() == before, "Unchanged responsive property rows must not allocate.");
        return checks;
    }
}
