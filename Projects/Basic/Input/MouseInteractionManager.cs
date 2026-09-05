using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Manages mouse interactions including buttons, position tracking,
    /// movement delta and mouse wheel actions.
    /// </summary>
    public class MouseInteractionManager : InputInteractionManager<MouseButton>
    {
        private readonly Dictionary<MouseWheelState, (Action pressAction, Action? releaseAction)> _wheelActions = new();

        private MouseWheelState _activeWheelState = MouseWheelState.None;
        private int _previousScrollValue;
        private PixelPoint2 _currentPosition;
        private PixelPoint2 _lastPosition;
        private PixelPoint2 _delta;

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseInteractionManager"/> class
        /// with the specified initial mouse state.
        /// </summary>
        /// <param name="initialState">The initial mouse state.</param>
        public MouseInteractionManager(MouseState initialState)
            : base(new MouseStateWrapper(initialState))
        {
            _currentPosition = new PixelPoint2(initialState.X, initialState.Y);
            _lastPosition = _currentPosition;
            _previousScrollValue = initialState.ScrollWheelValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseInteractionManager"/> class
        /// using the current mouse state.
        /// </summary>
        public MouseInteractionManager()
            : this(Mouse.GetState())
        {
        }

        /// <summary>
        /// Gets the current absolute mouse position in pixels.
        /// </summary>
        public PixelPoint2 Position => _currentPosition;

        /// <summary>
        /// Gets the mouse movement delta in pixels since the last update.
        /// </summary>
        public PixelPoint2 Delta => _delta;

        /// <summary>
        /// Gets the horizontal mouse movement delta in pixels.
        /// </summary>
        public int DeltaX => _delta.X;

        /// <summary>
        /// Gets the vertical mouse movement delta in pixels.
        /// </summary>
        public int DeltaY => _delta.Y;

        /// <summary>
        /// Adds an action for a mouse wheel state.
        /// </summary>
        /// <param name="wheelState">The mouse wheel state.</param>
        /// <param name="pressAction">The action invoked when the state becomes active.</param>
        /// <param name="releaseAction">The optional action invoked when the state is released.</param>
        public void AddWheel(
            MouseWheelState wheelState,
            Action pressAction,
            Action? releaseAction = null)
        {
            ArgumentNullException.ThrowIfNull(pressAction);
            _wheelActions[wheelState] = (pressAction, releaseAction);
        }

        /// <summary>
        /// Updates the mouse interactions using the current scene update context.
        /// </summary>
        /// <param name="context">Provides information about the current scene update.</param>
        public override void Update(SceneUpdateContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var state = Mouse.GetState();

            UpdateState(
                new MouseStateWrapper(state),
                context.GameTime.ElapsedGameTime);

            UpdatePosition(state);
            UpdateWheel(state);
        }

        private void UpdatePosition(MouseState state)
        {
            _currentPosition = new PixelPoint2(state.X, state.Y);
            _delta = _currentPosition - _lastPosition;
            _lastPosition = _currentPosition;
        }

        private void UpdateWheel(MouseState state)
        {
            var currentScrollValue = state.ScrollWheelValue;
            var scrollDelta = currentScrollValue - _previousScrollValue;
            var newWheelState = MouseWheelState.None;

            if (scrollDelta > 0)
                newWheelState = MouseWheelState.Up;
            else if (scrollDelta < 0)
                newWheelState = MouseWheelState.Down;

            if (newWheelState != _activeWheelState)
            {
                ReleaseWheel(_activeWheelState);
                PressWheel(newWheelState);
                _activeWheelState = newWheelState;
            }

            _previousScrollValue = currentScrollValue;
        }

        private void PressWheel(MouseWheelState wheelState)
        {
            if (wheelState == MouseWheelState.None)
                return;

            if (_wheelActions.TryGetValue(wheelState, out var actions))
                actions.pressAction.Invoke();
        }

        private void ReleaseWheel(MouseWheelState wheelState)
        {
            if (wheelState == MouseWheelState.None)
                return;

            if (_wheelActions.TryGetValue(wheelState, out var actions))
                actions.releaseAction?.Invoke();
        }
    }
}