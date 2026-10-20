using Microsoft.Xna.Framework;
using Sachssoft.Engine.Gameplay.Curves;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Mouse/touch playground with six independent analog controls.</summary>
public sealed class ControlsJoystickPage : Grid
{
    private readonly List<VirtualJoystick> _sticks = new();
    /// <summary>Gets the sample joysticks in visual order.</summary>
    public IReadOnlyList<VirtualJoystick> Sticks => _stickView;

    private readonly ReadOnlyCollection<VirtualJoystick> _stickView;
    /// <summary>Creates the mobile control playground.</summary>
    public ControlsJoystickPage() : base()
    {
        Padding = new Insets2(20);
        Spacing = 12;
        Background = new Color(17, 24, 35);
        _stickView = _sticks.AsReadOnly();
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Rows.Add(new RowTrack { Height = GridLength.Star() });
        Rows.Add(new RowTrack { Height = GridLength.Auto });
        Add(new Label() { Text = "VIRTUAL JOYSTICKS / MOUSE + MULTITOUCH", TextColor = new Color(65, 199, 223), Height = 28 });
        Add(new Label() { Text = "Drag a knob. Each contact has its own capture; floating controls start at the touch position.", WrapText = true }, row: 1);
        var cards = new Grid()
        {
            Spacing = 14
        };
        for (int i = 0; i < 3; i++)
            cards.Tracks.Add(new ColumnTrack { Width = GridLength.Star() });
        for (int i = 0; i < 2; i++)
            cards.Rows.Add(new RowTrack { Height = GridLength.Star() });
        Add(cards, row: 2);
        var status = new Label()
        {
            Text = "Ready. Positive Y points down; InvertY adapts it for game coordinates.",
            WrapText = true
        };
        Add(status, row: 3);
        AddStick("Free / fixed", JoystickMovementMode.Free, JoystickPlacementMode.Fixed, false, false);
        AddStick("Free / floating", JoystickMovementMode.Free, JoystickPlacementMode.Floating, false, false);
        AddStick("Vertical / up-down", JoystickMovementMode.Vertical, JoystickPlacementMode.Fixed, false, false);
        AddStick("Horizontal / retained value", JoystickMovementMode.Horizontal, JoystickPlacementMode.Fixed, true, false);
        AddStick("Custom axis / diagonal", JoystickMovementMode.CustomAxis, JoystickPlacementMode.Fixed, false, false);
        AddStick("Dead zone + quadratic curve", JoystickMovementMode.Free, JoystickPlacementMode.Fixed, false, true);
        void AddStick(string title, JoystickMovementMode mode, JoystickPlacementMode placement, bool retain, bool curve)
        {
            int index = _sticks.Count;
            var card = new Grid()
            {
                Padding = new Insets2(14),
                Spacing = 6,
                Background = new Color(25, 35, 49),
                BorderColor = new Color(43, 57, 76),
                BorderThickness = 1
            };
            card.Rows.Add(new RowTrack { Height = GridLength.Auto });
            card.Rows.Add(new RowTrack { Height = GridLength.Star() });
            card.Rows.Add(new RowTrack { Height = GridLength.Auto });
            card.Add(new Label() { Text = title, TextColor = new Color(65, 199, 223), Height = 24 });
            var stick = new VirtualJoystick()
            {
                MovementMode = mode,
                PlacementMode = placement,
                CustomAxis = new Vector2(1, -1),
                Radius = 64,
                DeadZone = curve ? .25f : .08f,
                ReturnToCenter = !retain,
                ResponseCurve = curve ? new QuadraticInCurve() : null
            };
            var value = new Label()
            {
                Text = "X 0.00   Y 0.00",
                Height = 24
            };
            stick.ValueChanged += (_, _) => value.Text = string.Create(CultureInfo.InvariantCulture, $"X {stick.Value.X:0.00}   Y {stick.Value.Y:0.00}");
            stick.Started += (_, _) => status.Text = $"{title}: started ({stick.ActivePointer?.Kind}, ID {stick.ActivePointer?.Id})";
            stick.Completed += (_, _) => status.Text = title + ": completed";
            stick.Cancelled += (_, _) => status.Text = title + ": cancelled / reset";
            card.Add(stick, row: 1);
            card.Add(value, row: 2);
            cards.Add(card, row: index / 3, column: index % 3);
            _sticks.Add(stick);
        }
    }
}
