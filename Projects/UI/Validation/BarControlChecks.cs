using System;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

internal static class BarControlChecks
{
    private static Button Button(float width = 30) => new()
    {
        Width = width,
        Height = 10,
        MinWidth = 0,
        MinHeight = 0,
        Padding = Insets2.Zero
    };
    private static void Reject<T>(Action action, Action<bool, string> check, string message)
        where T : Exception
    {
        bool rejected = false;
        try
        {
            action();
        }
        catch (T)
        {
            rejected = true;
        }

        check(rejected, message);
    }

    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var frame = new ControlInputFrame(new Size2(140, 200), TimeSpan.Zero);
        var toolbar = new ToolBar
        {
            Padding = Insets2.Zero,
            Spacing = 5,
            LineSpacing = 7
        };
        var first = new ToolStrip
        {
            Padding = Insets2.Zero,
            Spacing = 0,
            LineSpacing = 0
        };
        var second = new ToolStrip
        {
            Padding = Insets2.Zero,
            Spacing = 0,
            LineSpacing = 0
        };
        first.Children.Add(Button(40));
        first.Children.Add(Button(40));
        second.Children.Add(Button(70));
        toolbar.Children.Add(first);
        toolbar.Children.Add(second);
        check(first.Parent == toolbar, "Tool groups reserve their parent immediately");
        Reject<ArgumentException>(() => toolbar.Children.Add(new Button()), check, "ToolBar accepts only ToolStrip groups");
        Reject<ArgumentNullException>(() => toolbar.Children.Add(null!), check, "Null tool group must throw");
        Reject<InvalidOperationException>(() => toolbar.Children.Add(first), check, "Duplicate tool group must throw");
        using var toolbarRoot = new Workspace(toolbar);
        toolbarRoot.Update(frame, layout);
        check(first.Bounds.Y == 0 && second.Bounds.Y == first.Bounds.Bottom + 7, "Toolbar wraps complete groups to the next line");
        toolbarRoot.Update(new ControlInputFrame(new Size2(200, 200), TimeSpan.Zero), layout);
        check(first.Bounds.Y == second.Bounds.Y && second.Bounds.X == first.Bounds.Right + 5, "Toolbar recomputes wrapping after resize");
        toolbarRoot.Update(new ControlInputFrame(new Size2(50, 200), TimeSpan.Zero), layout);
        check(first.Children[1].Bounds.Y >= first.Children[0].Bounds.Bottom, "An oversized strip also wraps internally");
        first.IsVisible = false;
        toolbarRoot.Update(frame, layout);
        check(first.Bounds.Width == 0 && second.Bounds.Y == 0, "Hidden strips occupy no space");
        first.IsVisible = true;
        var mixed = new ToolStrip
        {
            Padding = Insets2.Zero
        };
        var editor = new TextBox();
        mixed.Children.Add(editor);
        mixed.Children.Add(new Image());
        check(editor.Parent == mixed && mixed.Children.Count == 2, "ToolStrip accepts arbitrary controls");
        var box = new ToolBox
        {
            Columns = 3,
            Spacing = 2,
            Padding = Insets2.Zero
        };
        for (int i = 0; i < 5; i++)
            box.Children.Add(Button(i == 0 ? 20 : 30));
        var bad = new TextBox();
        Reject<ArgumentException>(() => box.Children.Add(bad), check, "ToolBox accepts only ButtonBase controls");
        check(bad.Parent is null && box.Children.Count == 5, "Rejected toolbox child remains unowned");
        Reject<ArgumentNullException>(() => box.Children.Add(null!), check, "Null tool must throw");
        Reject<InvalidOperationException>(() => box.Children.Add(box.Children[0]), check, "Duplicate toolbox child must throw");
        Reject<ArgumentOutOfRangeException>(() => box.Columns = -1, check, "Negative columns must throw");
        Reject<ArgumentOutOfRangeException>(() => box.Rows = -1, check, "Negative rows must throw");
        Reject<ArgumentOutOfRangeException>(() => box.Spacing = float.NaN, check, "Invalid spacing must throw");
        using var boxRoot = new Workspace(box);
        boxRoot.Update(frame, layout);
        check(box.Children[1].Bounds.X == 32 && box.Children[2].Bounds.X == 64 && box.Children[3].Bounds.Y == 12, "Fixed columns use uniform cells and fill rows");
        box.Columns = 0;
        box.Rows = 2;
        boxRoot.Update(frame, layout);
        check(box.Children[1].Bounds.Y == 12 && box.Children[2].Bounds.X == 32 && box.Children[2].Bounds.Y == 0, "Fixed rows fill columns from top to bottom");
        box.Columns = 3;
        boxRoot.Update(frame, layout);
        check(box.Children[2].Bounds.X == 64 && box.Children[2].Bounds.Y == 0, "Explicit columns take precedence over rows");
        box.Columns = box.Rows = 0;
        boxRoot.Update(new ControlInputFrame(new Size2(65, 200), TimeSpan.Zero), layout);
        check(box.Children[1].Bounds.X == 32 && box.Children[2].Bounds.Y == 12, "Automatic toolbox wrapping follows available width");
        box.Children[1].IsVisible = false;
        boxRoot.Update(new ControlInputFrame(new Size2(65, 200), TimeSpan.Zero), layout);
        check(box.Children[1].Bounds.Width == 0 && box.Children[2].Bounds.X == 32, "Hidden tools do not reserve cells");
        box.Children[1].IsVisible = true;
        boxRoot.Update(new ControlInputFrame(new Size2(20, 200), TimeSpan.Zero), layout);
        check(box.Children[1].Bounds.X == 0 && box.Children[1].Bounds.Y == 12, "Narrow toolbox still provides at least one column");
        box.Children[0].Width = 40;
        boxRoot.Update(frame, layout);
        check(box.Children[1].Bounds.X == 42, "Changed tool size invalidates the uniform layout cache");
        var accepted = new ToolBox();
        accepted.Children.Add(new ToggleButton());
        accepted.Children.Add(new SelectorButton());
        accepted.Children.Add(new RepeatButton());
        accepted.Children.Add(new CheckBox());
        accepted.Children.Add(new RadioButton());
        check(accepted.Children.Count == 5, "All ButtonBase specializations are supported");
        boxRoot.Styles["ToolBox"]["Columns"] = "2";
        boxRoot.Styles["ToolBox"]["Spacing"] = "7";
        boxRoot.Update(frame, layout);
        check(box.Columns == 0 && box.Spacing == 2, "Local toolbox properties override styles");
        box.ResetStyleProperty(nameof(ToolBox.Columns));
        box.ResetStyleProperty(nameof(ToolBox.Spacing));
        boxRoot.Update(frame, layout);
        check(box.Columns == 2 && box.Spacing == 7, "Resetting toolbox properties restores central style values");
        int notifications = 0;
        box.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolBox.Columns))
                notifications++;
        };
        box.Columns = 4;
        box.Columns = 4;
        check(notifications == 1, "Toolbox properties notify only for effective changes");
        object? received = null;
        var commandButton = (ButtonBase)box.Children[0];
        commandButton.Command = new Sachssoft.Engine.Input.Command(parameter => received = parameter);
        commandButton.CommandParameter = "tool-command";
        boxRoot.Update(new ControlInputFrame(new Size2(140, 200), TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.PointerDown, new Microsoft.Xna.Framework.Vector2(4, 4)), new ControlInputEvent(ControlInputEventType.PointerUp, new Microsoft.Xna.Framework.Vector2(4, 4)) }), layout);
        check(Equals(received, "tool-command") && commandButton.IsPressed, "Toolbox buttons retain routed input, polling and command parameters");
        boxRoot.Update(frame, layout);
        check(!commandButton.IsPressed, "Toolbox button polling pulses reset next frame");
        var status = new StatusBar
        {
            Padding = Insets2.Zero,
            Spacing = 3
        };
        var text = new TextBlock
        {
            Text = "Ready",
            Width = 50,
            Height = 10
        };
        var statusText = new StatusStrip
        {
            Content = text,
            Padding = Insets2.Zero
        };
        var statusButton = new StatusStrip
        {
            Content = Button(20),
            Padding = Insets2.Zero
        };
        status.Children.Add(statusText);
        status.Children.Add(statusButton);
        Reject<ArgumentException>(() => status.Children.Add(new TextBlock()), check, "StatusBar accepts only StatusStrip hosts");
        Reject<InvalidOperationException>(() => status.Children.Add(statusText), check, "Duplicate status strip must throw");
        Reject<InvalidOperationException>(() => statusButton.Content = text, check, "Status content cannot have two owners");
        using var statusRoot = new Workspace(status);
        statusRoot.Update(frame, layout);
        check(status.Orientation == StackOrientation.Horizontal && statusButton.Bounds.X == 53, "Status strips form a horizontal row");
        check(text.Parent == statusText, "StatusStrip uses immediate Content ownership");
        statusText.Content = null;
        statusButton.Content = text;
        check(text.Parent == statusButton, "Status content can move after explicit release");
        statusRoot.Styles["StatusBar"]["Orientation"] = "Vertical";
        statusRoot.Update(frame, layout);
        check(status.Orientation == StackOrientation.Vertical, "Status bar defaults do not block central styles");
        var empty = new ToolBox
        {
            Padding = Insets2.Zero
        };
        empty.Measure(layout, new Size2(float.PositiveInfinity, float.PositiveInfinity));
        check(empty.DesiredSize == Size2.Zero, "Empty toolbox is valid under unbounded measurement");
        for (int i = 0; i < 100; i++)
            boxRoot.Update(frame, layout);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            boxRoot.Update(frame, layout);
        check(GC.GetAllocatedBytesForCurrentThread() == before, "Unchanged toolbox updates must allocate zero bytes");
        for (int i = 0; i < 100; i++)
            toolbarRoot.Update(frame, layout);
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            toolbarRoot.Update(frame, layout);
        check(GC.GetAllocatedBytesForCurrentThread() == before, "Unchanged toolbar updates must allocate zero bytes");
    }
}
