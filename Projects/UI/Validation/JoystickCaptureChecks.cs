using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Controls.Drawing;
using Sachssoft.Engine.Controls.Input;
using Sachssoft.Engine.Controls.Layouts;

/// <summary>Exercises joystick capture and the output seen by a consumer of ValueChanged.</summary>
internal static class JoystickCaptureChecks
{
    /// <summary>Runs touch, mouse, interruption, and workspace teardown scenarios.</summary>
    /// <param name="check">The validation runner's assertion callback.</param>
    /// <param name="layout">The font and layout context used by the test workspaces.</param>
    internal static void Run(Action<bool, string> check, ControlLayoutContext layout)
    {
        var first = new VirtualJoystick { Width = 160, Height = 160, Radius = 50 };
        var second = new VirtualJoystick { Width = 160, Height = 160, Radius = 50 };
        var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 20 };
        panel.Add(first);
        panel.Add(second);
        using var workspace = new Workspace(panel);
        var firstPointer = new ControlPointer(ControlPointerKind.Touch, 7);
        var secondPointer = new ControlPointer(ControlPointerKind.Touch, 8);
        var sampler = new ControlInputSampler();
        var viewport = new Rectangle(0, 0, 400, 300);
        Vector2 gameplayValue = Vector2.Zero;
        int completed = 0;
        int cancelled = 0;
        first.ValueChanged += (_, _) => gameplayValue = first.Value;
        first.Completed += (_, _) =>
        {
            CheckNeutral("Completed");
            completed++;
        };
        first.Cancelled += (_, _) =>
        {
            CheckNeutral("Cancelled");
            cancelled++;
        };

        void Frame(params ControlInputEvent[] input) =>
            workspace.Update(new ControlInputFrame(new Size2(400, 300), TimeSpan.Zero, input), layout);

        void Sample(MouseState mouse, bool active, params TouchLocation[] touches) =>
            workspace.Update(sampler, mouse, default, new TouchCollection(touches), viewport, TimeSpan.Zero, layout, active);

        void CheckNeutral(string scenario)
        {
            check(first.Value == Vector2.Zero && gameplayValue == Vector2.Zero && first.RawValue == Vector2.Zero,
                scenario + " must clear output and thumb displacement.");
            check(first.Center == new Vector2(first.Bounds.X + first.Bounds.Width / 2, first.Bounds.Y + first.Bounds.Height / 2),
                scenario + " must restore the arranged midpoint immediately.");
            check(!first.IsActive && first.ActivePointer is null &&
                workspace.GetCapturedControl(firstPointer) is null && workspace.CapturedControl is null,
                scenario + " must clear the active pointer and its capture.");
        }

        Frame();
        Vector2 center = first.Center;
        Vector2 pulled = center + new Vector2(35, 0);
        Vector2 outside = new(550, -40);
        ControlInputEvent Touch(ControlInputEventType type, Vector2 position, int id = 7) =>
            new(type, position, PointerId: id, PointerKind: ControlPointerKind.Touch);
        void Begin() => Frame(Touch(ControlInputEventType.PointerDown, first.Center),
            Touch(ControlInputEventType.PointerMove, first.Center + new Vector2(35, 0)));

        Begin();
        check(first.ActivePointer == firstPointer && workspace.GetCapturedControl(firstPointer) == first &&
            gameplayValue.X > 0 && gameplayValue.Y == 0, "The initiating finger owns the joystick and supplies a direction.");
        Frame(Touch(ControlInputEventType.PointerUp, pulled));
        CheckNeutral("Inside release");
        Begin();
        Frame(Touch(ControlInputEventType.PointerMove, outside));
        check(first.IsActive && gameplayValue.X > 0, "Capture must continue outside the control and viewport.");
        Frame(Touch(ControlInputEventType.PointerUp, outside));
        CheckNeutral("Outside release");

        Begin();
        Frame(Touch(ControlInputEventType.PointerDown, pulled, 9), Touch(ControlInputEventType.PointerUp, pulled, 9),
            Touch(ControlInputEventType.PointerDown, second.Center, 8),
            Touch(ControlInputEventType.PointerMove, second.Center + new Vector2(30, 0), 8));
        check(first.ActivePointer == firstPointer && second.ActivePointer == secondPointer &&
            gameplayValue.X > 0 && second.Value.X > 0, "Another finger cannot steal capture, and the second control can capture independently.");
        Frame(Touch(ControlInputEventType.PointerUp, outside));
        CheckNeutral("Release while the other finger is down");
        check(second.IsActive && second.Value.X > 0, "Releasing the first pointer must preserve the second pointer.");
        Frame(Touch(ControlInputEventType.PointerUp, second.Center, 8));

        Begin();
        Frame(Touch(ControlInputEventType.PointerCancel, outside));
        CheckNeutral("Explicit cancel");
        Begin();
        workspace.ReleasePointer(first, firstPointer);
        CheckNeutral("Capture lost");

        Begin();
        workspace.ColorPicking.Begin(second, new SceneViewportTestColorSource(), Color.White, _ => { }, _ => { }, () => { });
        int releases = completed;
        Frame(Touch(ControlInputEventType.PointerUp, outside));
        CheckNeutral("Release consumed by color picking");
        check(completed == releases, "An intercepted release must cancel instead of completing the gameplay gesture.");
        workspace.ColorPicking.Cancel();

        first.PlacementMode = JoystickPlacementMode.Floating;
        Frame();
        Frame(Touch(ControlInputEventType.PointerDown, center - new Vector2(20, 15)),
            Touch(ControlInputEventType.PointerMove, pulled));
        check(first.Center != center, "A floating stick uses its touchdown anchor while held.");
        workspace.CancelInput();
        CheckNeutral("Focus loss without a new frame");
        workspace.CancelInput();
        CheckNeutral("Repeated focus loss");
        first.PlacementMode = JoystickPlacementMode.Fixed;
        Frame();

        Sample(default, true, new TouchLocation(7, TouchLocationState.Pressed, center));
        Sample(default, true, new TouchLocation(7, TouchLocationState.Moved, outside),
            new TouchLocation(8, TouchLocationState.Pressed, second.Center));
        Sample(default, true, new TouchLocation(8, TouchLocationState.Moved, second.Center));
        CheckNeutral("The active contact disappears at the screen edge");
        check(second.IsActive, "A missing contact must cancel only its own pointer.");
        Sample(default, false);
        check(!second.IsActive && second.Value == Vector2.Zero, "Deactivation cancels the remaining touch.");

        Sample(Mouse(pulled, ButtonState.Pressed), true);
        check(first.ActivePointer == ControlPointer.Mouse && gameplayValue.X > 0, "The desktop mouse must capture and update output.");
        Sample(Mouse(outside, ButtonState.Pressed), true);
        Sample(Mouse(outside, ButtonState.Released), true);
        CheckNeutral("Desktop release outside");

        int beforeNative = completed;
        var native = new JoystickPrimaryMouseEvents(
            (new Point((int)center.X, (int)center.Y), true),
            (new Point((int)outside.X, (int)outside.Y), false));
        var nativeFrame = sampler.SampleReusable(Mouse(outside, ButtonState.Released), default, default,
            viewport, 1f, TimeSpan.Zero, true, true, native);
        workspace.Update(nativeFrame, layout);
        CheckNeutral("Native drag between samples");
        check(completed == beforeNative + 1, "The original press inside must capture even if the next sample is outside.");

        int beforeRapid = completed;
        for (int i = 0; i < 40; i++)
        {
            Frame(Touch(ControlInputEventType.PointerDown, pulled), Touch(ControlInputEventType.PointerUp, pulled));
            CheckNeutral("Rapid press and release");
        }
        check(completed == beforeRapid + 40, "Each ordered press/release pair completes once.");

        Begin();
        first.IsEnabled = false;
        Frame();
        CheckNeutral("Control disabled");
        first.IsEnabled = true;
        Frame();
        Begin();
        panel.Remove(first);
        Frame();
        CheckNeutral("Control detached");
        panel.Add(first);
        Frame();
        Begin();
        workspace.Dispose();
        CheckNeutral("Scene workspace disposed");
        check(cancelled >= 8, "Interruptions must notify the consumer about cancellation.");
    }

    /// <summary>Creates a mouse sample at the specified position and left-button state.</summary>
    /// <param name="position">The position in viewport pixels.</param>
    /// <param name="left">The state of the primary mouse button.</param>
    /// <returns>The device state supplied to the workspace sampler.</returns>
    private static MouseState Mouse(Vector2 position, ButtonState left) =>
        new((int)position.X, (int)position.Y, 0, left, ButtonState.Released, ButtonState.Released,
            ButtonState.Released, ButtonState.Released);
}
