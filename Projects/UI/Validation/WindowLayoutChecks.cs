using Microsoft.Xna.Framework;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Styling;

/// <summary>Checks window content sizing, close-button placement and independent visibility and availability.</summary>
internal static class WindowLayoutChecks
{
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var body = new MeasureProbe
        {
            Width = 120,
            Height = 50
        };
        var window = new Window
        {
            Width = 300,
            Height = 240,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Insets2(10),
            ShowCloseButton = false,
            Content = body
        };
        using var workspace = new Workspace(new Panel());
        workspace.ShowWindow(window);
        var idle = new ControlInputFrame(new Size2(600, 400), TimeSpan.Zero);
        void Frame() => workspace.Update(idle, layout);
        Frame();
        check(window.SizeToContent == SizeToContent.Manual && window.Bounds.Width == 300 && window.Bounds.Height == 240, "Manual sizing preserves configured dimensions");
        foreach (var mode in new[]
        {
            SizeToContent.Width,
            SizeToContent.Height,
            SizeToContent.WidthAndHeight
        }

        )
        {
            window.SizeToContent = mode;
            Frame();
            check(window.Bounds.Width == (mode == SizeToContent.Height ? 300 : 140), mode + " fits the correct width axis");
            check(window.Bounds.Height == (mode == SizeToContent.Width ? 240 : 106), mode + " includes padding and title bar in the correct height axis");
            check(window.Width == 300 && window.Height == 240, "Content sizing retains stored manual dimensions");
        }

        body.Width = 200;
        body.Height = 80;
        Frame();
        check(window.Bounds.Width == 220 && window.Bounds.Height == 136, "Content changes resize an already shown window");
        window.MinWidth = 250;
        window.MinHeight = 150;
        Frame();
        check(window.Bounds.Width == 250 && window.Bounds.Height == 150, "Automatic sizing respects minimum dimensions");
        window.MinWidth = 0;
        window.MinHeight = 0;
        window.SizeToContent = SizeToContent.Manual;
        Frame();
        check(window.Bounds.Width == 300 && window.Bounds.Height == 240, "Returning to Manual restores stored dimensions");
        var style = new Style
        {
            BasedOn = "Window",
            SizeToContent = SizeToContent.Height,
            CloseButtonPlacement = CloseButtonPlacement.Left,
            ShowCloseButton = true,
            IsCloseButtonEnabled = false
        };
        workspace.Registry.Styles.Register("Test.Window", style);
        window.StyleId = "Test.Window";
        window.ResetStyleProperty(nameof(Window.SizeToContent));
        window.ResetStyleProperty(nameof(Window.ShowCloseButton));
        Frame();
        check(window.SizeToContent == SizeToContent.Height && window.Bounds.Height == 136 && !window.IsCloseButtonEnabled, "New window settings support central styles");
        check(window.CloseButtonBounds.X == window.Bounds.X && window.CloseButtonBounds.Width == 32, "Left close-button hit bounds match the title bar");
        Vector2 at = new(window.CloseButtonBounds.X + 16, window.CloseButtonBounds.Y + 18);
        Vector2 old = new(window.Bounds.X, window.Bounds.Y);
        workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.PointerDown, at), new ControlInputEvent(ControlInputEventType.PointerMove, at + new Vector2(30, 20)), new ControlInputEvent(ControlInputEventType.PointerUp, at + new Vector2(30, 20)) }), layout);
        check(workspace.OverlayCount == 1 && window.Bounds.X == old.X && window.Bounds.Y == old.Y, "Disabled close buttons do not close or initiate window dragging");
        window.ShowCloseButton = false;
        Frame();
        check(window.CloseButtonBounds.Width == 0 && workspace.OverlayCount == 1, "Hiding close removes its hit target without closing the window");
        window.ShowCloseButton = true;
        window.IsCloseButtonEnabled = true;
        window.CloseButtonPlacement = CloseButtonPlacement.Right;
        Frame();
        check(window.CloseButtonBounds.Right == window.Bounds.Right, "Right close-button hit bounds match the title bar");
        for (int i = 0; i < 100; i++)
            Frame();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Frame();
        long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
        check(bytes == 0, "Unchanged automatic window layout allocates zero bytes");
        window.SizeToContent = SizeToContent.WidthAndHeight;
        Frame();
        Size2 prior = window.Bounds.Size;
        Vector2 grip = new(window.Bounds.Right - 4, window.Bounds.Bottom - 4);
        workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.PointerDown, grip), new ControlInputEvent(ControlInputEventType.PointerMove, grip + new Vector2(20, 15)), new ControlInputEvent(ControlInputEventType.PointerUp, grip + new Vector2(20, 15)) }), layout);
        check(window.SizeToContent == SizeToContent.Manual && window.Width == prior.Width + 20 && window.Height == prior.Height + 15, "Grip resizing switches to Manual from the current automatic bounds");
        int closed = 0;
        window.Closed += (_, _) => closed++;
        Frame();
        at = new(window.CloseButtonBounds.X + 16, window.CloseButtonBounds.Y + 18);
        workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.PointerDown, at) }), layout);
        check(workspace.OverlayCount == 0 && closed == 1, "Enabled right close button closes exactly once");
        var left = new Window
        {
            CloseButtonPlacement = CloseButtonPlacement.Left,
            Width = 200,
            Height = 140,
            MinWidth = 0,
            MinHeight = 0
        };
        workspace.ShowWindow(left);
        Frame();
        at = new(left.CloseButtonBounds.X + 16, left.CloseButtonBounds.Y + 18);
        workspace.Update(new ControlInputFrame(idle.Size, TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.PointerDown, at) }), layout);
        check(workspace.OverlayCount == 0, "Enabled left close button uses its relocated hit target");
        var wrapped = new Window
        {
            SizeToContent = SizeToContent.WidthAndHeight,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Insets2(10),
            ShowCloseButton = false,
            Content = new TextBlock
            {
                WrapText = true,
                Text = "A long paragraph with enough words to wrap inside a narrow workspace without requiring horizontal space outside the viewport."
            }
        };
        workspace.ShowWindow(wrapped);
        workspace.Update(new ControlInputFrame(new Size2(200, 180), TimeSpan.Zero), layout);
        check(wrapped.Bounds.Width == 200 && wrapped.Bounds.Height <= 180 && wrapped.ContentPresenter!.DesiredSize.Height > 16, "Large natural content reflows within the workspace width");
        wrapped.Close();
        Frame();
        var dialog = new Dialog
        {
            SizeToContent = SizeToContent.WidthAndHeight,
            Content = "Dialog",
            MinWidth = 0,
            MinHeight = 0
        };
        var completion = workspace.ShowDialog(dialog);
        Frame();
        check(dialog.Bounds.Width < 560 && dialog.Bounds.Height < 360, "Dialogs inherit automatic sizing instead of their styled fixed dimensions");
        dialog.Close();
        Frame();
        check(completion.IsCompleted, "Automatic sizing preserves the dialog completion lifecycle");
    }
}
