using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;

/// <summary>Checks menu separators as independent entries during layout, activation and navigation.</summary>
internal static class MenuSeparatorChecks
{
    /// <summary>Runs menu model, command, keyboard and retained-frame checks.</summary>
    /// <param name = "check">The assertion callback.</param>
    /// <param name = "layout">The measurement context with its fixed test font.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        int firstCalls = 0;
        int secondCalls = 0;
        int commandCalls = 0;
        var first = new MenuItem
        {
            Text = "First",
            Action = () => firstCalls++
        };
        var separator = new MenuSeparator();
        var second = new MenuItem
        {
            Text = "Second",
            Action = () => secondCalls++,
            IsCheckable = true,
            ShortcutKey = Keys.R,
            ShortcutControl = true,
            CommandParameter = "payload",
            Command = new Sachssoft.Engine.Input.Command(parameter =>
            {
                check(Equals(parameter, "payload"), "Commands after a separator retain their parameter");
                commandCalls++;
            })
        };
        var menu = new ContextMenu
        {
            Padding = Insets2.Zero,
            ItemHeight = 30
        };
        menu.Items.Add(first);
        menu.Items.Add(separator);
        menu.Items.Add(second);
        using var workspace = new Workspace(menu);
        var frame = new ControlInputFrame(new Size2(320, 160), TimeSpan.Zero);
        void Frame(params ControlInputEvent[] events) => workspace.Update(new ControlInputFrame(frame.Size, TimeSpan.Zero, events), layout);
        workspace.Update(frame, layout);
        check(menu.DesiredSize.Height == 68, "Popup separators reserve eight pixels without a command row");
        Frame(new ControlInputEvent(ControlInputEventType.PointerMove, new Vector2(10, 34)));
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(10, 34)));
        Frame(new ControlInputEvent(ControlInputEventType.PointerUp, new Vector2(10, 34)));
        check(firstCalls == 0 && secondCalls == 0 && commandCalls == 0 && !second.IsChecked, "Hovering and clicking a separator cannot invoke commands or change checked states");
        workspace.Focus(menu);
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Down));
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter));
        check(secondCalls == 1 && commandCalls == 1 && second.IsChecked, "Down skips a separator and Enter invokes the next command");
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Down));
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter));
        check(firstCalls == 1, "Forward navigation wraps over separator entries");
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Up));
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter));
        check(secondCalls == 2 && commandCalls == 2 && !second.IsChecked, "Backward navigation skips separators and preserves checkbox activation");
        check(menu.HandleShortcut(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.R, Control: true)) && commandCalls == 3, "Shortcut matching skips non-command entries");
        var branch = new MenuItem
        {
            Text = "Branch"
        };
        branch.Items.Add(new MenuSeparator());
        branch.Items.Add(new MenuItem { Text = "Nested", ShortcutKey = Keys.N, Action = () => firstCalls++ });
        menu.Items.Add(branch);
        check(menu.HandleShortcut(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.N)) && firstCalls == 2, "Nested shortcut traversal supports mixed command and separator entries");
        workspace.Update(frame, layout);
        Frame(new ControlInputEvent(ControlInputEventType.PointerDown, new Vector2(10, 83)));
        Frame(new ControlInputEvent(ControlInputEventType.PointerUp, new Vector2(10, 83)));
        check(workspace.OverlayCount == 1, "Submenus copy mixed entry collections without converting separators into commands");
        Frame(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Escape));
        var bar = new MenuBar
        {
            Padding = Insets2.Zero
        };
        bar.Items.Add(new MenuItem { Text = "A" });
        bar.Items.Add(new MenuSeparator());
        bar.Items.Add(new MenuItem { Text = "B" });
        using var barWorkspace = new Workspace(bar);
        barWorkspace.Update(frame, layout);
        check(bar.DesiredSize.Width == layout.DefaultFont.Measure("A").Width + layout.DefaultFont.Measure("B").Width + 64, "Horizontal menu separators reserve an eight-pixel divider slot");
        check(bar.BorderColor.A > 0, "The standard menu bar provides a visible solid separator fallback");
        var onlySeparators = new ContextMenu
        {
            Padding = Insets2.Zero
        };
        onlySeparators.Items.Add(new MenuSeparator());
        using var emptyWorkspace = new Workspace(onlySeparators);
        emptyWorkspace.Update(frame, layout);
        emptyWorkspace.Focus(onlySeparators);
        emptyWorkspace.Update(new ControlInputFrame(frame.Size, TimeSpan.Zero, new[] { new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Down), new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter) }), layout);
        check(!onlySeparators.HandleShortcut(new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.R)), "A separator-only menu has no activatable entries or shortcuts");
        for (int i = 0; i < 100; i++)
            barWorkspace.Update(frame, layout);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            barWorkspace.Update(frame, layout);
        check(GC.GetAllocatedBytesForCurrentThread() == before, "Unchanged menus with separators allocate zero bytes per update");
    }
}
