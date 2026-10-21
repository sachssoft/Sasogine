using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Checks physical viewport resizing, shared DPI input transforms and steady layout caching.</summary>
internal static class InspectorDpiChecks
{
    internal static int Run(ControlLayoutContext layout)
    {
        int checks = 0;
        void Check(bool value, string message)
        {
            checks++;
            if (!value)
                throw new InvalidOperationException(message);
        }

        var root = new Grid();
        root.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
        root.Columns.Add(new GridColumn { Width = new GridLength(40) });
        var button = new Button
        {
            Width = 100,
            Height = 32,
            HorizontalAlignment = ControlAlignment.Start,
            VerticalAlignment = ControlAlignment.Start
        };
        var probe = new InspectorDpiProbe();
        root.Children.Add(button);
        root.Cells.Put(button);
        root.Children.Add(probe);
        root.Cells.Put(probe, column: 1);
        using var workspace = new Workspace(root);
        var sampler = new ControlInputSampler();
        Rectangle viewport = new(20, 10, 600, 400);
        void Frame(MouseState mouse = default, TouchCollection touches = default) => workspace.Update(sampler, mouse, default, touches, viewport, TimeSpan.FromSeconds(.016), layout);
        static MouseState Mouse(int x, int y, ButtonState left = ButtonState.Released) => new(x, y, 0, left, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
        Check(workspace.Dpi == 96 && workspace.DpiScale == 1, "Workspace DPI defaults to 96 dots per inch");
        Frame();
        Check(workspace.Size.Equals(new Size2(600, 400)) && root.Bounds.Width == 600 && root.GetColumnWidth(0) == 560, "Physical viewport creates the initial logical layout");
        int measured = probe.Measures;
        Frame();
        Check(probe.Measures == measured, "Unchanged physical viewports retain the cached layout");
        viewport = new Rectangle(20, 10, 900, 600);
        Frame();
        Check(workspace.Size.Equals(new Size2(900, 600)) && root.GetColumnWidth(0) == 860 && probe.Measures > measured, "Resizing reallocates layout space instead of scaling the previous layout");
        Check(button.Bounds.Width == 100 && button.Bounds.Height == 32 && workspace.Dpi == 96, "Fixed controls retain their logical size and DPI when the window grows");
        int dpiNotifications = 0, scaleNotifications = 0;
        workspace.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Workspace.Dpi))
                dpiNotifications++;
            if (args.PropertyName == nameof(Workspace.DpiScale))
                scaleNotifications++;
        };
        workspace.Dpi = 192;
        workspace.Dpi = 192;
        Frame();
        Check(workspace.Size.Equals(new Size2(450, 300)) && root.Bounds.Width == 450 && root.GetColumnWidth(0) == 410, "200 percent DPI halves logical viewport dimensions and reflows layout");
        Check(dpiNotifications == 1 && scaleNotifications == 1 && workspace.DpiScale == 2, "DPI changes expose cached MVVM notifications once per effective change");
        int clicked = 0;
        button.Clicked += (_, _) => clicked++;
        Frame(Mouse(100, 42, ButtonState.Pressed));
        Frame(Mouse(100, 42));
        Check(clicked == 1 && workspace.PointerPosition == new Vector2(40, 16), "Mouse hit testing uses the same DPI and viewport offset as layout");
        Frame(Mouse(300, 42, ButtonState.Pressed));
        Frame(Mouse(300, 42));
        Check(clicked == 1, "DPI hit testing does not accept pixels outside a fixed-size control");
        var touches = new TouchCollection(new[] { new TouchLocation(7, TouchLocationState.Pressed, new Vector2(100, 42)) });
        Frame(Mouse(300, 42), touches);
        touches = new TouchCollection(new[] { new TouchLocation(7, TouchLocationState.Released, new Vector2(100, 42)) });
        Frame(Mouse(300, 42), touches);
        Check(clicked == 2, "Native touch input shares the mouse DPI transform");
        workspace.Dpi = 144;
        Frame(Mouse(320, 160));
        Check(workspace.Size.Equals(new Size2(600, 400)) && workspace.PointerPosition == new Vector2(200, 100), "150 percent DPI maps physical size and coordinates consistently");
        workspace.Dpi = 96;
        Frame(Mouse(320, 160));
        Check(workspace.PointerPosition == new Vector2(300, 150), "A stationary physical pointer is remapped after DPI changes");
        foreach (float invalid in new[]
        {
            0f,
            -1f,
            float.NaN,
            float.PositiveInfinity,
            float.Epsilon
        }

        )
        {
            bool rejected = false;
            try
            {
                workspace.Dpi = invalid;
            }
            catch (ArgumentOutOfRangeException)
            {
                rejected = true;
            }

            Check(rejected && workspace.Dpi == 96, "Invalid DPI values leave workspace state unchanged");
        }

        workspace.Dpi = 192;
        ControlInputFrame sampled = sampler.Sample(Mouse(100, 42), default, viewport, .5f, TimeSpan.Zero);
        Check(sampled.PhysicalViewport == viewport && sampled.SourceScale == .5f, "Sampled frames preserve their physical viewport and coordinate transform");
        workspace.Update(sampled, layout);
        Check(workspace.Size.Equals(new Size2(450, 300)) && workspace.PointerPosition == new Vector2(40, 16), "Existing sampled-frame Update calls automatically follow Workspace.Dpi without double scaling");
        workspace.Dpi = 96;
        Frame();
        measured = probe.Measures;
        for (int i = 0; i < 100; i++)
            Frame();
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            Frame();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Check(probe.Measures == measured, "Stable DPI and viewport do not rebuild layout on subsequent frames");
        Check(allocated == 0, "Reusable physical input and DPI processing must not allocate: " + allocated);
        Console.WriteLine($"BENCHMARK: 1000 steady DPI/viewport updates: {allocated} bytes.");
        return checks;
    }
}
