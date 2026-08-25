using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        private int _previousScrollValue = 0;

        private Point _currentPosition;

        private Point _lastPosition;

        private Point _delta;


        /// <summary>
        /// Initializes a new instance of the <see cref="MouseInteractionManager"/> class
        /// with the specified initial mouse state.
        /// </summary>
        /// <param name="initialState">The initial mouse state.</param>
        public MouseInteractionManager(MouseState initialState)
            : base(new MouseStateWrapper(initialState))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MouseInteractionManager"/> class
        /// using the current mouse state.
        /// </summary>
        public MouseInteractionManager()
            : base(new MouseStateWrapper(Mouse.GetState()))
        {
        }


        /// <summary>
        /// Gets the current absolute mouse position.
        /// </summary>
        public Point Position => _currentPosition;


        /// <summary>
        /// Gets the mouse movement delta since the last update.
        /// </summary>
        public Point Delta => _delta;


        /// <summary>
        /// Gets the mouse movement delta on the X axis.
        /// </summary>
        public int DeltaX => _delta.X;


        /// <summary>
        /// Gets the mouse movement delta on the Y axis.
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
            _wheelActions[wheelState] = (pressAction, releaseAction);
        }


        /// <summary>
        /// Updates the mouse state using the current MonoGame mouse state.
        /// </summary>
        /// <param name="gameTime">The current game time information.</param>
        public override void Update(GameTime gameTime)
        {
            Update(Mouse.GetState(), gameTime.ElapsedGameTime);
        }


        /// <summary>
        /// Updates the mouse state using the specified mouse state.
        /// </summary>
        /// <param name="state">The current mouse state.</param>
        /// <param name="elapsed">The elapsed time since the previous update.</param>
        public void Update(MouseState state, TimeSpan elapsed)
        {
            var currentStateWrapper = new MouseStateWrapper(state);

            Update(currentStateWrapper, elapsed);


            _currentPosition = state.Position;

            _delta = _currentPosition - _lastPosition;

            _lastPosition = _currentPosition;


            int currentScrollValue = state.ScrollWheelValue;

            int scrollDelta = currentScrollValue - _previousScrollValue;


            MouseWheelState newWheelState = MouseWheelState.None;

            if (scrollDelta > 0)
            {
                newWheelState = MouseWheelState.Up;
            }
            else if (scrollDelta < 0)
            {
                newWheelState = MouseWheelState.Down;
            }


            if (newWheelState != _activeWheelState)
            {
                if (_activeWheelState != MouseWheelState.None &&
                    _wheelActions.TryGetValue(_activeWheelState, out var oldActions))
                {
                    oldActions.releaseAction?.Invoke();
                }


                if (newWheelState != MouseWheelState.None &&
                    _wheelActions.TryGetValue(newWheelState, out var newActions))
                {
                    newActions.pressAction.Invoke();
                }


                _activeWheelState = newWheelState;
            }
            else if (newWheelState == MouseWheelState.None &&
                     _activeWheelState != MouseWheelState.None)
            {
                if (_wheelActions.TryGetValue(_activeWheelState, out var releaseActions))
                {
                    releaseActions.releaseAction?.Invoke();
                }

                _activeWheelState = MouseWheelState.None;
            }


            _previousScrollValue = currentScrollValue;
        }
    }
}