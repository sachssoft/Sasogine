using System;
using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Input;
/// <summary>Checks viewport layout and deferred pipette acceptance without a graphics device.</summary>
internal static class SceneViewportTests
{
    internal static int Run()
    {
        int checks = 0;
        void Check(bool value) { checks++; if (!value) throw new InvalidOperationException("SceneViewport/deferred pipette regression"); }
        var view = new SceneViewport { Width = 180, Height = 100, IsFocusable = true };
        using var root = new Workspace(view);
        var layout = new ControlLayoutContext(new TestFont());
        var frame = new ControlInputFrame(new Size2(400,300), TimeSpan.Zero);
        root.Update(frame, layout);
        Check(view.Bounds.Width == 180 && view.Bounds.Height == 100);
        Check(new UIRegistry().Create("SceneViewport") is SceneViewport);
        int notifications = 0;
        view.PropertyChanged += (_, _) => notifications++;
        view.UseDirectRendering = true; view.UseDirectRendering = true;
        Check(notifications == 1);
        var source = new DeferredColorPickSource();
        int accepted = 0, cancelled = 0;
        Color picked = default;
        root.ColorPicking.Begin(view, source, Color.White, _ => { }, c => { accepted++; picked=c; }, () => cancelled++);
        root.Update(new ControlInputFrame(new Size2(400,300),TimeSpan.Zero,new[]{new ControlInputEvent(ControlInputEventType.PointerDown,new Vector2(20,20),Button:MouseButton.Left)}),layout);
        Check(root.ColorPicking.IsActive && accepted == 0);
        source.Source = new SceneViewportTestColorSource();
        root.Update(frame, layout);
        Check(!root.ColorPicking.IsActive && accepted == 1 && picked == Color.Red && cancelled == 0);
        source.Source = null;
        root.ColorPicking.Begin(view, source, Color.White, _ => { }, _ => accepted++, () => cancelled++);
        root.Update(frame, layout);
        root.ColorPicking.Cancel();
        source.Source = new SceneViewportTestColorSource();
        root.Update(frame,layout);
        Check(accepted == 1 && cancelled == 1);
        return checks;
    }
}
