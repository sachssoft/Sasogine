using System;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;

/// <summary>Checks numeric text drafts against repeated application updates and decimal cultures.</summary>
internal static class ValueTextBoxEditingTests
{
    /// <summary>Runs decimal entry, binding refresh, focus, cancellation and steady-state allocation checks.</summary>
    /// <param name="layout">The shared headless layout and font context.</param>
    /// <returns>The number of assertions completed.</returns>
    internal static int Run(ControlLayoutContext layout)
    {
        int checks = 0;
        void Check(bool condition, string message)
        {
            checks++;
            if (!condition) throw new InvalidOperationException(message);
        }

        void Frame(Workspace workspace, params ControlInputEvent[] events) =>
            workspace.Update(new ControlInputFrame(new Size2(400, 300), TimeSpan.FromSeconds(.016), events), layout);

        foreach (string culture in new[] { "", "de-DE", "en-US" })
        {
            var value = new ValueTextBox
            {
                Minimum = .01f, Maximum = 10, Step = .01f, Value = 1,
                CultureName = culture, NumberFormat = "0.00'x'", Width = 260
            };
            using var root = new Workspace(value);
            Frame(root);
            foreach (string draft in new[] { "", "0", "0,", "0.", "0,0", "0.0", "0,01", "0.01", "-" })
            {
                value.BeginEdit();
                value.Editor.Text = draft;
                for (int frame = 0; frame < 8; frame++)
                {
                    value.Value = value.Value;
                    Frame(root);
                    Check(value.Editor.Text == draft && value.IsEditing,
                        "An unchanged binding refresh overwrote the numeric draft: " + draft);
                }
                value.CancelEdit();
            }

            value.BeginEdit();
            value.Editor.Text = "0,01";
            value.Value = 2;
            Frame(root);
            Check(value.Editor.Text == "0,01" && value.Value == 2, "External changes must preserve an active draft.");
            Check(value.CommitEdit() && value.Value == .01f, "Comma decimal input must commit as 0.01, never as 1.");
            value.CancelEdit();
            Check(value.Editor.Text is "0.01" or "0,01", "Editing must not show G9 floating-point artifacts.");

            foreach (string input in new[] { "0.01", "0,01", "1.25", "1,25", "10", "0", "100" })
            {
                value.BeginEdit();
                value.Editor.Text = input;
                float expected = input.StartsWith('1') ? input == "10" || input == "100" ? 10f : 1.25f : .01f;
                Check(value.CommitEdit() && Math.Abs(value.Value - expected) < .00001f,
                    "Decimal commit or boundary clamp failed: " + culture + " / " + input);
                value.CancelEdit();
            }

            foreach (string invalid in new[] { "", "-", "1,234.5", "NaN", "Infinity", "invalid" })
            {
                value.BeginEdit();
                float committed = value.Value;
                value.Editor.Text = invalid;
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Enter));
                Check(value.IsEditing && value.Editor.HasError && value.Value == committed,
                    "Invalid text must stay editable without changing the committed value.");
                value.Value = committed;
                Frame(root);
                Check(value.Editor.Text == invalid, "Validation errors must survive unrelated model updates.");
                Frame(root, new ControlInputEvent(ControlInputEventType.KeyDown, Key: Keys.Escape));
                Check(!value.IsEditing && !value.Editor.HasError && value.Value == committed,
                    "Escape must cancel the draft and retain the committed value.");
            }

            value.BeginEdit();
            value.Editor.Text = "0,01";
            root.Focus(null);
            Check(value.Value == .01f && !value.IsEditing, "Focus loss must commit a comma decimal synchronously.");

            value.Value = 1;
            value.BeginEdit();
            value.Editor.Text = "0.";
            value.Value = 3;
            Frame(root);
            value.CancelEdit();
            Check(value.Value == 3 && value.Editor.Text == "3", "Cancellation must restore the latest external value.");
        }

        var idle = new ValueTextBox { Minimum = .01f, Maximum = 10, Step = .01f, Value = 1 };
        using (var workspace = new Workspace(idle))
        {
            Frame(workspace);
            for (int i = 0; i < 100; i++) idle.Value = 1;
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) idle.Value = 1;
            Check(GC.GetAllocatedBytesForCurrentThread() == before, "Unchanged numeric refresh must not allocate formatting strings.");
        }
        return checks;
    }
}
