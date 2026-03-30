using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using System;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Input
{
    public struct PointerStateTracker
    {
        private bool _wasInsideLastFrame;
        private bool _wasPressedLastFrame;
        private bool _pressedInsideThisControl; // Press begann innerhalb des Controls
        private Vector2 _lastPointerPos;

        private double _lastClickTime;
        private const double DoubleClickThreshold = 500; // ms

        public VisualState VisualState;
        public PointerPressState PressState;
        public PointerMoveState MoveState;

        /// <summary>
        /// Aktualisiert den Tracker für ein Control.
        /// </summary>
        public void Update(in Rectangle bounds, in Vector2 pointerPos, bool isPressed, TimeSpan time, bool isFocused = false, bool isDisabled = false, bool isSelected = false)
        {
            double totalMilliseconds = time.TotalMilliseconds;

            PressState = PointerPressState.None;
            MoveState = PointerMoveState.None;
            VisualState = VisualState.None;

            if (isDisabled)
            {
                VisualState = VisualState.Disabled;
                _wasInsideLastFrame = false;
                _wasPressedLastFrame = false;
                _pressedInsideThisControl = false;
                _lastPointerPos = pointerPos;
                return;
            }

            bool isInside = bounds.Contains(pointerPos);

            // --- Movement Flags ---
            if (!_wasInsideLastFrame && isInside) MoveState |= PointerMoveState.Enter;
            if (_wasInsideLastFrame && !isInside) MoveState |= PointerMoveState.Leave;
            if (isInside) MoveState |= PointerMoveState.Hovered;
            if (pointerPos != _lastPointerPos) MoveState |= PointerMoveState.Moved;

            // --- Pressed ---
            if (!_wasPressedLastFrame && isPressed)
            {
                if (isInside)
                {
                    // Press beginnt innerhalb des Controls → gültig
                    _pressedInsideThisControl = true;
                    PressState |= PointerPressState.Pressed;
                }
                else
                {
                    _pressedInsideThisControl = false;
                }
            }

            // --- Released / Clicked / DoubleClicked ---
            if (_wasPressedLastFrame && !isPressed && _pressedInsideThisControl)
            {
                PressState |= PointerPressState.Released;
                PressState |= PointerPressState.Clicked;

                if ((totalMilliseconds - _lastClickTime) <= DoubleClickThreshold)
                    PressState |= PointerPressState.DoubleClicked;

                _lastClickTime = totalMilliseconds;
                _pressedInsideThisControl = false;
            }

            // --- VisualState ---
            if (isInside) VisualState |= VisualState.Hovered;
            if (_pressedInsideThisControl && isPressed) VisualState |= VisualState.Pressed;
            if (isFocused) VisualState |= VisualState.Focused;
            if (isSelected) VisualState |= VisualState.Selected;

            // --- Internal State Update ---
            _wasInsideLastFrame = isInside;
            _wasPressedLastFrame = isPressed;
            _lastPointerPos = pointerPos;
        }
    }
}