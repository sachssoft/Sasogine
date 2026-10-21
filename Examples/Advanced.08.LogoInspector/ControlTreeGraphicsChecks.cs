namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Verifies that ownership guards reject draw-time changes before reserving or releasing controls.</summary>
internal static class ControlTreeGraphicsChecks
{
    internal static void Verify(ControlDrawContext draw, ControlLayoutContext layout)
    {
        var panel = new Grid();
        panel.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
        panel.Columns.Add(new GridColumn { Width = GridLength.Proportion() });
        var retained = new Image();
        var host = new ContentControl
        {
            Content = retained
        };
        var probe = new OwnershipDrawProbe();
        var tabs = new TabControl();
        var inactive = new TabItem();
        tabs.Items.Add(inactive);
        panel.Children.Add(host);
        panel.Children.Add(tabs);
        panel.Children.Add(probe);
        panel.Cells.Put(host);
        using var root = new Workspace(panel);
        root.Update(new ControlInputFrame(draw.LogicalSize, TimeSpan.Zero), layout);
        var candidate = new Image();
        var popup = new Window();
        int rejected = 0;
        void Reject(Action action)
        {
            bool failed = false;
            try
            {
                action();
            }
            catch (InvalidOperationException)
            {
                failed = true;
            }

            if (!failed)
                throw new InvalidOperationException("Tree mutation was accepted while drawing.");
            rejected++;
        }

        probe.Checking = () =>
        {
            Reject(() => host.Content = candidate);
            Reject(() => panel.Children.Add(candidate));
            Reject(() => panel.Children.Remove(host));
            Reject(() => inactive.Content = candidate);
            Reject(() => inactive.ToolTipContent = candidate);
            Reject(() => panel.Cells.Put(host, column: 1));
            Reject(() => panel.Cells.Remove(host));
            Reject(() => root.ShowWindow(popup));
        };
        root.Draw(draw);
        if (rejected != 8 || candidate.Parent is not null || popup.Workspace is not null || host.Content != retained || retained.Parent != host || host.Parent != panel || panel.Cells[host].Column != 0)
            throw new InvalidOperationException("Rejected draw-time mutations changed ownership or placement.");
        var recovered = new ContentControl
        {
            Content = popup
        };
        recovered.Content = null;
        Console.WriteLine("PASS: 8 real draw-time ownership guards; rejected changes leave the tree intact.");
    }
}
