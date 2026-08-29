using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Manages button combinations and button sequences for an input state.
    /// </summary>
    /// <typeparam name="TButton">
    /// The button type used by the input state.
    /// </typeparam>
    public abstract class InputInteractionManager<TButton> where TButton : struct, Enum
    {
        private static readonly TButton[] _allButtons =
            Enum.GetValues<TButton>().Cast<TButton>().ToArray();

        /// <summary>
        /// Gets the input state from the previous update.
        /// </summary>
        protected IInputState<TButton> _previousState;

        private readonly List<CombinationEntry> _comboActions = new();
        private readonly List<CombinationEntry> _activeCombos = new();
        private readonly List<SequenceEntry> _sequences = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="InputInteractionManager{TButton}"/> class.
        /// </summary>
        /// <param name="initialState">
        /// The initial input state used as the previous state for the first update.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="initialState"/> is <see langword="null"/>.
        /// </exception>
        protected InputInteractionManager(
            IInputState<TButton> initialState)
        {
            _previousState =
                initialState ??
                throw new ArgumentNullException(nameof(initialState));
        }

        /// <summary>
        /// Adds a button combination with a press action and an optional release action.
        /// </summary>
        /// <param name="buttons">
        /// The buttons that must be pressed simultaneously.
        /// </param>
        /// <param name="pressAction">
        /// The action invoked when the combination becomes pressed.
        /// </param>
        /// <param name="releaseAction">
        /// The optional action invoked when the combination is released.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buttons"/> or <paramref name="pressAction"/> is
        /// <see langword="null"/>.
        /// </exception>
        public void AddCombination(
            IEnumerable<TButton> buttons,
            Action pressAction,
            Action? releaseAction = null)
        {
            ArgumentNullException.ThrowIfNull(buttons);
            ArgumentNullException.ThrowIfNull(pressAction);

            var buttonSet =
                new HashSet<TButton>(buttons);

            _comboActions.Add(
                new CombinationEntry(
                    buttonSet,
                    pressAction,
                    releaseAction));
        }

        /// <summary>
        /// Adds a single button with a press action and an optional release action.
        /// </summary>
        /// <param name="button">
        /// The button to register.
        /// </param>
        /// <param name="pressAction">
        /// The action invoked when the button becomes pressed.
        /// </param>
        /// <param name="releaseAction">
        /// The optional action invoked when the button is released.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="pressAction"/> is <see langword="null"/>.
        /// </exception>
        public void Add(
            TButton button,
            Action pressAction,
            Action? releaseAction = null)
        {
            AddCombination(
                new[] { button },
                pressAction,
                releaseAction);
        }

        /// <summary>
        /// Removes all registered combinations that contain exactly the specified buttons.
        /// </summary>
        /// <param name="buttons">
        /// The buttons that identify the combinations to remove.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buttons"/> is <see langword="null"/>.
        /// </exception>
        public void RemoveCombination(
            IEnumerable<TButton> buttons)
        {
            ArgumentNullException.ThrowIfNull(buttons);

            var buttonSet =
                new HashSet<TButton>(buttons);

            _comboActions.RemoveAll(
                entry => entry.Buttons.SetEquals(buttonSet));

            _activeCombos.RemoveAll(
                entry => entry.Buttons.SetEquals(buttonSet));
        }

        /// <summary>
        /// Adds a button sequence that invokes an action when completed.
        /// </summary>
        /// <param name="sequence">
        /// The buttons that must be pressed in the specified order.
        /// </param>
        /// <param name="action">
        /// The action invoked when the sequence is completed.
        /// </param>
        /// <param name="timeout">
        /// The maximum time allowed between sequence buttons.
        /// If <see langword="null"/>, one second is used.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sequence"/> or <paramref name="action"/> is
        /// <see langword="null"/>.
        /// </exception>
        public void AddSequence(
            IList<TButton> sequence,
            Action action,
            TimeSpan? timeout = null)
        {
            _sequences.Add(
                new SequenceEntry(
                    sequence,
                    action,
                    timeout ?? TimeSpan.FromSeconds(1)));
        }

        /// <summary>
        /// Removes a previously registered button sequence.
        /// </summary>
        /// <param name="sequence">
        /// The button sequence to remove.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sequence"/> is <see langword="null"/>.
        /// </exception>
        public void RemoveSequence(
            IList<TButton> sequence)
        {
            ArgumentNullException.ThrowIfNull(sequence);

            _sequences.RemoveAll(
                entry => entry.Sequence.SequenceEqual(sequence));
        }

        /// <summary>
        /// Removes all registered button combinations.
        /// </summary>
        public void ClearCombinations()
        {
            _comboActions.Clear();
            _activeCombos.Clear();
        }

        /// <summary>
        /// Clears all currently active combinations.
        /// </summary>
        /// <param name="triggerReleaseActions">
        /// <see langword="true"/> to invoke the release action of each active combination.
        /// </param>
        public void ClearActiveCombinations(
            bool triggerReleaseActions = true)
        {
            if (triggerReleaseActions)
            {
                foreach (var entry in _activeCombos)
                {
                    entry.ReleaseAction?.Invoke();
                }
            }

            _activeCombos.Clear();
        }

        /// <summary>
        /// Removes all registered button sequences.
        /// </summary>
        public void ClearSequences()
        {
            _sequences.Clear();
        }

        /// <summary>
        /// Clears all registered combinations, active combinations and sequences.
        /// </summary>
        public void Clear()
        {
            ClearCombinations();
            ClearSequences();
        }

        /// <summary>
        /// Removes an active button combination.
        /// </summary>
        /// <param name="buttons">
        /// The buttons identifying the active combination.
        /// </param>
        /// <param name="triggerRelease">
        /// <see langword="true"/> to invoke the combination's release action.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="buttons"/> is <see langword="null"/>.
        /// </exception>
        public void RemoveActiveCombination(
            IEnumerable<TButton> buttons,
            bool triggerRelease = true)
        {
            ArgumentNullException.ThrowIfNull(buttons);

            var buttonSet =
                new HashSet<TButton>(buttons);

            for (int i = _activeCombos.Count - 1; i >= 0; i--)
            {
                var entry =
                    _activeCombos[i];

                if (!entry.Buttons.SetEquals(buttonSet))
                    continue;

                if (triggerRelease)
                {
                    entry.ReleaseAction?.Invoke();
                }

                _activeCombos.RemoveAt(i);
            }
        }

        /// <summary>
        /// Updates the interaction manager using the current game context.
        /// </summary>
        /// <param name="context">
        /// The current game context.
        /// </param>
        public void Update(
            GameContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            Update(context.GameTime);
        }

        /// <summary>
        /// Updates the interaction manager using the current game time.
        /// </summary>
        /// <param name="gameTime">
        /// The current game time.
        /// </param>
        public abstract void Update(
            GameTime gameTime);

        /// <summary>
        /// Updates the interaction manager using the current input state.
        /// </summary>
        /// <param name="currentState">
        /// The current input state.
        /// </param>
        /// <param name="elapsed">
        /// The elapsed time since the previous update.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="currentState"/> is <see langword="null"/>.
        /// </exception>
        protected void Update(
            IInputState<TButton> currentState,
            TimeSpan elapsed)
        {
            ArgumentNullException.ThrowIfNull(currentState);

            foreach (var entry in _comboActions)
            {
                var isPressedNow =
                    entry.Buttons.All(
                        button => currentState.IsButtonDown(button));

                var wasPressedBefore =
                    entry.Buttons.All(
                        button => _previousState.IsButtonDown(button));

                if (isPressedNow && !wasPressedBefore)
                {
                    entry.PressAction.Invoke();

                    if (!_activeCombos.Contains(entry))
                    {
                        _activeCombos.Add(entry);
                    }
                }
                else if (!isPressedNow && wasPressedBefore)
                {
                    if (_activeCombos.Remove(entry))
                    {
                        entry.ReleaseAction?.Invoke();
                    }
                }
            }

            foreach (var sequence in _sequences)
            {
                sequence.Update(
                    currentState,
                    _previousState,
                    elapsed);
            }

            _previousState = currentState;
        }

        /// <summary>
        /// Represents a registered button combination and its associated actions.
        /// </summary>
        protected class CombinationEntry
        {
            /// <summary>
            /// Gets the buttons that make up the combination.
            /// </summary>
            public HashSet<TButton> Buttons { get; }

            /// <summary>
            /// Gets the action invoked when the combination is pressed.
            /// </summary>
            public Action PressAction { get; }

            /// <summary>
            /// Gets the optional action invoked when the combination is released.
            /// </summary>
            public Action? ReleaseAction { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="CombinationEntry"/> class.
            /// </summary>
            /// <param name="buttons">
            /// The buttons that make up the combination.
            /// </param>
            /// <param name="pressAction">
            /// The action invoked when the combination is pressed.
            /// </param>
            /// <param name="releaseAction">
            /// The optional action invoked when the combination is released.
            /// </param>
            public CombinationEntry(
                HashSet<TButton> buttons,
                Action pressAction,
                Action? releaseAction)
            {
                Buttons = buttons;
                PressAction = pressAction;
                ReleaseAction = releaseAction;
            }
        }

        /// <summary>
        /// Represents a registered button sequence.
        /// </summary>
        protected class SequenceEntry
        {
            /// <summary>
            /// Gets the buttons that make up the sequence.
            /// </summary>
            public IList<TButton> Sequence { get; }

            /// <summary>
            /// Gets the action invoked when the sequence is completed.
            /// </summary>
            public Action Action { get; }

            /// <summary>
            /// Gets the maximum time allowed between sequence buttons.
            /// </summary>
            public TimeSpan Timeout { get; }

            private int _currentIndex;
            private TimeSpan _timeSinceLastButton;

            /// <summary>
            /// Initializes a new instance of the <see cref="SequenceEntry"/> class.
            /// </summary>
            /// <param name="sequence">
            /// The buttons that make up the sequence.
            /// </param>
            /// <param name="action">
            /// The action invoked when the sequence is completed.
            /// </param>
            /// <param name="timeout">
            /// The maximum time allowed between sequence buttons.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="sequence"/> or <paramref name="action"/> is
            /// <see langword="null"/>.
            /// </exception>
            public SequenceEntry(
                IList<TButton> sequence,
                Action action,
                TimeSpan timeout)
            {
                Sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
                Action = action ?? throw new ArgumentNullException(nameof(action));

                Timeout = timeout;
            }

            /// <summary>
            /// Updates the sequence using the current and previous input states.
            /// </summary>
            /// <param name="current">
            /// The current input state.
            /// </param>
            /// <param name="previous">
            /// The previous input state.
            /// </param>
            /// <param name="elapsed">
            /// The elapsed time since the previous update.
            /// </param>
            public void Update(
                IInputState<TButton> current,
                IInputState<TButton> previous,
                TimeSpan elapsed)
            {
                _timeSinceLastButton += elapsed;

                if (_timeSinceLastButton > Timeout)
                {
                    _currentIndex = 0;
                    _timeSinceLastButton = TimeSpan.Zero;
                }

                if (_currentIndex >= Sequence.Count)
                    return;

                var buttonToMatch =
                    Sequence[_currentIndex];

                if (current.IsButtonDown(buttonToMatch) &&
                    previous.IsButtonUp(buttonToMatch))
                {
                    _currentIndex++;
                    _timeSinceLastButton = TimeSpan.Zero;

                    if (_currentIndex == Sequence.Count)
                    {
                        Action.Invoke();
                        _currentIndex = 0;
                    }
                }
                else if (AnyOtherButtonPressed(
                    current,
                    previous,
                    buttonToMatch))
                {
                    _currentIndex = 0;
                    _timeSinceLastButton = TimeSpan.Zero;
                }
            }

            /// <summary>
            /// Determines whether another button was just pressed.
            /// </summary>
            /// <param name="current">
            /// The current input state.
            /// </param>
            /// <param name="previous">
            /// The previous input state.
            /// </param>
            /// <param name="exclude">
            /// The button that is currently expected by the sequence.
            /// </param>
            /// <returns>
            /// <see langword="true"/> if another button was just pressed;
            /// otherwise, <see langword="false"/>.
            /// </returns>
            private bool AnyOtherButtonPressed(
                IInputState<TButton> current,
                IInputState<TButton> previous,
                TButton exclude)
            {
                foreach (var button in _allButtons)
                {
                    if (button.Equals(exclude))
                        continue;

                    if (current.IsButtonDown(button) &&
                        previous.IsButtonUp(button))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}