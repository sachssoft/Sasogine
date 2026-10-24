using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Inspection;

/// <summary>Checks full-width inspector groups and property rows across expansion, nesting and viewport changes.</summary>
internal static class InspectorStretchChecks
{
    /// <summary>Runs regression checks for collapsed inspectors shrinking to their header text.</summary>
    /// <param name = "check">The assertion callback.</param>
    /// <param name = "layout">The fixed test font and layout context.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        CheckLabelThreshold(check, layout);
        var inspector = new InspectorView();
        var group = inspector.AddGroup("A", false);
        string value = "Value";
        var editor = group.Fields.AddText("Name", () => value, next => value = next);
        var nested = group.Fields.AddGroup("B", false);
        var nestedEditor = nested.Fields.AddText("Nested", () => value, next => value = next);
        using var workspace = new Workspace(inspector);
        foreach (int width in new[]
        {
            260,
            400,
            600
        }

        )
        {
            var idle = new ControlInputFrame(new Size2(width, 500), TimeSpan.Zero);
            void Frame() => workspace.Update(idle, layout);
            for (int cycle = 0; cycle < 3; cycle++)
            {
                group.IsExpanded = false;
                Frame();
                Control fields = inspector.ContentPresenter!;
                check(fields.Bounds.Width == inspector.ContentBounds.Width && fields.Bounds.X == inspector.ContentBounds.X, "Collapsed inspector fields retain the full content width");
                check(fields.Bounds.Y == inspector.ContentBounds.Y, "Collapsed groups stay anchored at the top");
                check(group.Bounds.Width == fields.ContentBounds.Width && group.Bounds.X == fields.ContentBounds.X, "Collapsed group headers fill their complete row");
                group.IsExpanded = true;
                Frame();
                check(group.Bounds.Width == fields.ContentBounds.Width && group.Fields.Bounds.Width == group.ContentBounds.Width, "Expanding preserves group and nested inspector width");
                check(editor.Parent!.Bounds.Width == group.Fields.ContentBounds.Width, "Expanded property rows fill the complete inner width");
                nested.IsExpanded = true;
                Frame();
                check(nestedEditor.Parent!.Bounds.Width == nested.Fields.ContentBounds.Width, "Nested definition rows also stretch to their available width");
                nested.IsExpanded = false;
                Frame();
                check(nested.Bounds.Width == nested.Parent!.ContentBounds.Width, "Collapsing a nested group preserves the complete header row");
            }
        }

        var scrollInspector = new InspectorView();
        var scrollGroup = scrollInspector.AddGroup("Scroll", false);
        for (int i = 0; i < 12; i++)
            scrollGroup.Fields.AddText("Field " + i, () => value, next => value = next);
        var viewer = new ScrollViewer
        {
            Content = scrollInspector,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
        };
        using var scrollWorkspace = new Workspace(viewer);
        var frame = new ControlInputFrame(new Size2(340, 220), TimeSpan.Zero);
        for (int i = 0; i < 4; i++)
        {
            scrollGroup.IsExpanded = true;
            scrollWorkspace.Update(frame, layout);
            check(viewer.VerticalScrollBar.IsVisible && scrollGroup.Bounds.Width == viewer.ViewportBounds.Width, "Expanded groups use the full viewport width beside a vertical scrollbar");
            scrollGroup.IsExpanded = false;
            scrollWorkspace.Update(frame, layout);
            check(!viewer.VerticalScrollBar.IsVisible && scrollGroup.Bounds.Width == viewer.ViewportBounds.Width, "Collapsed groups reclaim the full row when the vertical scrollbar disappears");
        }
    }

    /// <summary>Checks configurable thresholds, nested inheritance, local overrides, style resets and input validation.</summary>
    /// <param name = "check">The assertion callback.</param>
    /// <param name = "layout">The test layout context.</param>
    private static void CheckLabelThreshold(Action<bool, string> check, ControlLayoutContext layout)
    {
        var view = new InspectorView();
        string value = "Value";
        Control editor = view.AddText("Name", () => value, next => value = next);
        var group = view.AddGroup("Nested", true);
        Control nestedEditor = group.Fields.AddText("Name", () => value, next => value = next);
        using var workspace = new Workspace(view);
        var frame = new ControlInputFrame(new Size2(360, 500), TimeSpan.Zero);
        Control label = ((ContainerControl)editor.Parent!).Children[0];
        Control nestedLabel = ((ContainerControl)nestedEditor.Parent!).Children[0];
        workspace.Update(frame, layout);
        check(label.Bounds.Bottom < editor.Bounds.Top, "The default threshold retains stacked labels below 420");
        view.LabelInlineMinWidth = 300;
        workspace.Update(frame, layout);
        check(label.Bounds.Right < editor.Bounds.Left, "A smaller threshold places labels beside editors");
        check(group.Fields.LabelInlineMinWidth == 300 && nestedLabel.Bounds.Right < nestedEditor.Bounds.Left, "Nested inspectors inherit the configured threshold and invalidate their cached rows");
        group.Fields.LabelInlineMinWidth = 500;
        view.LabelInlineMinWidth = 0;
        workspace.Update(frame, layout);
        check(label.Bounds.Right < editor.Bounds.Left && nestedLabel.Bounds.Bottom < nestedEditor.Bounds.Top, "Zero keeps labels inline while nested local overrides remain independent");
        group.Fields.ResetStyleProperty(nameof(InspectorView.LabelInlineMinWidth));
        workspace.Update(frame, layout);
        check(group.Fields.LabelInlineMinWidth == 0 && nestedLabel.Bounds.Right < nestedEditor.Bounds.Left, "Resetting a nested override restores the inherited threshold");
        var style = new Sachssoft.Engine.Controls.Styling.Style
        {
            LabelInlineMinWidth = 600
        };
        view.ApplyStyle(style);
        check(view.LabelInlineMinWidth == 0, "Local threshold values take precedence over styles");
        view.ResetStyleProperty(nameof(InspectorView.LabelInlineMinWidth));
        workspace.Update(frame, layout);
        check(view.LabelInlineMinWidth == 600 && label.Bounds.Bottom < editor.Bounds.Top, "Resetting the local value restores the styled threshold and refreshes layout");
        foreach (float invalid in new[]
        {
            -1f,
            float.NaN,
            float.PositiveInfinity,
            float.NegativeInfinity
        }

        )
        {
            bool rejected = false;
            try
            {
                view.LabelInlineMinWidth = invalid;
            }
            catch (ArgumentOutOfRangeException)
            {
                rejected = true;
            }

            check(rejected && view.LabelInlineMinWidth == 600, "Invalid thresholds cannot change the effective layout");
        }
    }
}
