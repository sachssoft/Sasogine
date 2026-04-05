using Sachssoft.Sasogine.Presentation.Input;
using Sachssoft.Sasogine.Presentation.Rendering;

namespace Sachssoft.Sasogine.Presentation.Widgets;

public class Button : Widget
{
    private PointerStateTracker _pointerTracker;
    private ButtonState _buttonState;

    public VisualState VisualState => _pointerTracker.VisualState;
    public ButtonState State => _buttonState;

    internal protected override void Render(FrameContext context)
    {
        bool isPressed = (context.Input.Mouse.Interaction & MouseInteractionState.Pressed) != 0;

        var bounds = context.Bounds;

        // --- Tracker Update ---
        _pointerTracker.Update(
            bounds.Container,
            context.Input.Mouse.Position,
            isPressed,
            context.GameTime.TotalGameTime,
            isFocused: IsFocused,
            isDisabled: !IsEnabled,
            isSelected: false
        );

        //// --- Visualisierung ---
        //Color color = _pointerTracker.VisualState switch
        //{
        //    VisualState.None => Color.Black,
        //    VisualState.Hovered => Color.Red,
        //    VisualState.Pressed => Color.Green,
        //    VisualState.Focused => Color.Yellow,
        //    VisualState.Selected => Color.Violet,
        //    VisualState.Disabled => Color.DimGray,
        //    _ => Color.Black
        //};

        //context.Render.DrawRectangle(bounds.Container, Color);
        BackgroundBrush?.Render(context.Bounds.Container, context.Render);

        // --- ButtonState direkt aus Tracker berechnen ---
        _buttonState = ButtonState.None;

        if (_pointerTracker.VisualState.HasFlag(VisualState.Hovered))
            _buttonState |= ButtonState.Hovered;
        if (_pointerTracker.PressState.HasFlag(PointerPressState.Pressed))
            _buttonState |= ButtonState.Pressed;
        if (_pointerTracker.PressState.HasFlag(PointerPressState.Released))
            _buttonState |= ButtonState.Released;
        if (_pointerTracker.PressState.HasFlag(PointerPressState.Clicked))
            _buttonState |= ButtonState.Clicked;
        if (_pointerTracker.PressState.HasFlag(PointerPressState.DoubleClicked))
            _buttonState |= ButtonState.DoubleClicked;
        if (_pointerTracker.MoveState.HasFlag(PointerMoveState.Enter))
            _buttonState |= ButtonState.PointerEnter;
        if (_pointerTracker.MoveState.HasFlag(PointerMoveState.Leave))
            _buttonState |= ButtonState.PointerLeave;
    }
}