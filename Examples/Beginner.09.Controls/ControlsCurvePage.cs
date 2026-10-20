using Microsoft.Xna.Framework;
using Sachssoft.Engine.Gameplay.Curves;
using Sachssoft.Engine.Input;
using System;
using System.Globalization;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Editable response curve, engine easing previews and a joystick using the same live model.</summary>
public sealed class ControlsCurvePage : Grid
{
    private bool _syncing;
    /// <summary>Gets the interactive editor.</summary>
    public CurveControl Editor { get; }
    /// <summary>Gets the read-only engine curve preview.</summary>
    public CurveControl Preview { get; }
    /// <summary>Gets the joystick evaluating the edited curve.</summary>
    public VirtualJoystick Joystick { get; }

    /// <summary>Creates a complete curve editing example.</summary>
    public ControlsCurvePage() : base()
    {
        Padding = new Insets2(20);
        Spacing = 12;
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Rows.Add(new RowTrack { Height = GridLength.Star() });
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Add(new Label() { Text = "CURVE CONTROL / EDITOR + CURVEBASE PREVIEW", Height = 28, StyleId = "Gallery.Heading" });
        var model = new EditableCurve(new[] { new CurvePoint(0, 0), new CurvePoint(.25f, .08f), new CurvePoint(.6f, .8f), new CurvePoint(1, 1) });
        Editor = new CurveControl()
        {
            Curve = model,
            SelectedIndex = 1,
            ToolTip = "Drag points. Ctrl+click/double-click adds. Delete removes. Shift snaps. Wheel zooms; middle/Alt+drag pans. Escape cancels."
        };
        var toolbar = new WrapPanel()
        {
            Spacing = 8
        };
        Add(toolbar, row: 1);
        Button Action(string text, Action execute, Func<bool>? allowed = null)
        {
            var button = new Button()
            {
                Content = new TextBlock
                {
                    Text = text
                },
                Height = 36,
                Command = new Command(_ => execute(), _ => allowed?.Invoke() ?? true)
            };
            toolbar.Add(button);
            return button;
        }

        Action("Add point", () =>
        {
            var points = Editor.EditableCurve!.Points;
            int gap = 0;
            for (int i = 1; i < points.Count - 1; i++)
                if (points[i + 1].X - points[i].X > points[gap + 1].X - points[gap].X)
                    gap = i;
            float x = (points[gap].X + points[gap + 1].X) / 2;
            Editor.AddPoint(new(x, Editor.Curve.GetValue(x)));
        });
        Action("Delete", () => Editor.RemoveSelectedPoint(), () => Editor.SelectedIndex > 0 && Editor.SelectedIndex < Editor.EditableCurve!.Points.Count - 1);
        Action("Undo", Editor.Undo, () => Editor.CanUndo);
        Action("Redo", Editor.Redo, () => Editor.CanRedo);
        Action("Fit", Editor.FitToCurve);
        Action("Reset view", Editor.ResetView);
        var interpolation = new ComboBox()
        {
            Width = 160,
            Height = 36
        };
        foreach (string name in new[]
        {
            "Step",
            "Linear",
            "Monotone cubic"
        }

        )
            interpolation.Items.Add(new SelectorItem(name));
        interpolation.SelectedIndex = 2;
        toolbar.Add(interpolation);
        interpolation.SelectionChanged += (_, _) =>
        {
            if (!_syncing)
                Editor.Interpolation = interpolation.SelectedIndex switch
                {
                    0 => CurveInterpolation.Step,
                    1 => CurveInterpolation.Linear,
                    _ => CurveInterpolation.Monotone
                };
        };
        var content = new Grid()
        {
            Spacing = 14
        };
        content.Tracks.Add(new ColumnTrack { Width = GridLength.Star(2.2f) });
        content.Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        content.Add(Editor);
        Add(content, row: 2);
        var side = new Grid()
        {
            Padding = new Insets2(12),
            Spacing = 10,
            BorderThickness = 1
        };
        side.Rows.Add(new RowTrack { Height = GridLength.Auto });
        side.Rows.Add(new RowTrack { Height = GridLength.Star() });
        side.Rows.Add(new RowTrack { Height = GridLength.Auto });
        side.Rows.Add(new RowTrack { Height = GridLength.Auto });
        content.Add(side, column: 1);
        var presets = new ComboBox()
        {
            Height = 36
        };
        foreach (string name in new[]
        {
            "Linear",
            "Quadratic In",
            "Sine In/Out",
            "Bounce Out",
            "Back Out",
            "Elastic Out"
        }

        )
            presets.Items.Add(new SelectorItem(name));
        presets.SelectedIndex = 2;
        side.Add(presets);
        Preview = new CurveControl()
        {
            Curve = new SineInOutCurve(),
            IsReadOnly = true,
            MinHeight = 180,
            Padding = new Insets2(4),
            PointRadius = 3
        };
        side.Add(Preview, row: 1);
        presets.SelectionChanged += (_, _) =>
        {
            Preview.Curve = presets.SelectedIndex switch
            {
                0 => new LinearCurve(),
                1 => new QuadraticInCurve(),
                2 => new SineInOutCurve(),
                3 => new BounceOutCurve(),
                4 => new BackOutCurve(),
                _ => new ElasticOutCurve()};
            Preview.FitToCurve();
        };
        Joystick = new VirtualJoystick()
        {
            Width = 184,
            Height = 184,
            HorizontalAlignment = ControlAlignment.Center,
            ResponseCurve = model,
            DeadZone = 0,
            ToolTip = "The joystick evaluates the exact same EditableCurve through CurveBase."
        };
        side.Add(Joystick, row: 2);
        side.Add(new Label() { Text = "Drag the joystick: its response uses the edited curve on the left.", WrapText = true }, row: 3);
        var pointRow = new WrapPanel()
        {
            Spacing = 10
        };
        Add(pointRow, row: 3);
        pointRow.Add(new Label() { Text = "Selected point", Height = 36, VerticalAlignment = ControlAlignment.Center });
        var xInput = new NumericSpinner()
        {
            Width = 140,
            Height = 36,
            NumericMinimum = 0,
            NumericMaximum = 1,
            NumericIncrement = .01m,
            DecimalPlaces = 3
        };
        var yInput = new NumericSpinner()
        {
            Width = 140,
            Height = 36,
            NumericMinimum = -1000,
            NumericMaximum = 1000,
            NumericIncrement = .01m,
            DecimalPlaces = 3
        };
        pointRow.Add(new Label() { Text = "X", Height = 36, VerticalAlignment = ControlAlignment.Center });
        pointRow.Add(xInput);
        pointRow.Add(new Label() { Text = "Y", Height = 36, VerticalAlignment = ControlAlignment.Center });
        pointRow.Add(yInput);
        pointRow.Add(new Label() { Text = "Shift = snap  |  Ctrl+Z / Ctrl+Y", Height = 36, VerticalAlignment = ControlAlignment.Center });
        var status = new Label()
        {
            Text = "Drag handles, edit X/Y, or compare engine curves on the right. Home resets the view; F fits it.",
            WrapText = true
        };
        Add(status, row: 4);
        void Sync()
        {
            _syncing = true;
            try
            {
                int index = Editor.SelectedIndex;
                var curve = Editor.EditableCurve!;
                xInput.IsEnabled = yInput.IsEnabled = index >= 0;
                if (index >= 0)
                {
                    var point = curve.Points[index];
                    xInput.Value = (decimal)point.X;
                    yInput.Value = (decimal)point.Y;
                    xInput.IsReadOnly = index == 0 || index == curve.Points.Count - 1;
                }

                interpolation.SelectedIndex = curve.Interpolation switch
                {
                    CurveInterpolation.Step => 0,
                    CurveInterpolation.Linear => 1,
                    _ => 2
                };
                status.Text = $"{curve.Points.Count} points. CurveBase output at 0.5: {curve.GetValue(.5f).ToString("0.000", CultureInfo.InvariantCulture)}";
            }
            finally
            {
                _syncing = false;
            }
        }

        xInput.ValueChanged += (_, _) =>
        {
            if (!_syncing)
                Editor.MoveSelectedPoint(new((float)xInput.Value, (float)yInput.Value));
        };
        yInput.ValueChanged += (_, _) =>
        {
            if (!_syncing)
                Editor.MoveSelectedPoint(new((float)xInput.Value, (float)yInput.Value));
        };
        Editor.CurveChanged += (_, _) => Sync();
        Editor.SelectionChanged += (_, _) => Sync();
        Editor.EditCancelled += (_, _) => status.Text = "Edit cancelled; original points restored.";
        Sync();
    }
}
