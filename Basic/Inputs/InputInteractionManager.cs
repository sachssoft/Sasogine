using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Input
{

    public abstract class InputInteractionManager<TButton> where TButton : struct, Enum
    {
        protected static readonly TButton[] _allButtons = Enum.GetValues<TButton>().Cast<TButton>().ToArray();

        protected IInputState<TButton> _previousState;

        private readonly Dictionary<HashSet<TButton>, (Action pressAction, Action? releaseAction)> _combo_actions = new(new HashSetComparer<TButton>());
        private readonly List<HashSet<TButton>> _active_combos = new();
        private readonly List<SequenceEntry> _sequences = new();

        public InputInteractionManager(IInputState<TButton> initialState)
        {
            _previousState = initialState ?? throw new ArgumentNullException(nameof(initialState));
        }

        public void AddCombination(IEnumerable<TButton> buttons, Action pressAction, Action? releaseAction = null)
        {
            var button_set = new HashSet<TButton>(buttons);
            _combo_actions[button_set] = (pressAction, releaseAction);
        }

        public void Add(TButton button, Action pressAction, Action? releaseAction = null)
        {
            AddCombination(new[] { button }, pressAction, releaseAction);
        }

        public void RemoveCombination(IEnumerable<TButton> buttons)
        {
            var button_set = new HashSet<TButton>(buttons);
            _combo_actions.Remove(button_set);
        }

        public void AddSequence(IList<TButton> sequence, Action action, TimeSpan? timeout = null)
        {
            _sequences.Add(new SequenceEntry(sequence, action, timeout ?? TimeSpan.FromSeconds(1)));
        }

        public void RemoveSequence(IList<TButton> sequence)
        {
            _sequences.RemoveAll(s => s.Sequence.SequenceEqual(sequence));
        }

        public void ClearCombinations()
        {
            _combo_actions.Clear();
            _active_combos.Clear();
        }

        public void ClearActiveCombinations(bool triggerReleaseActions = true)
        {
            if (triggerReleaseActions)
            {
                foreach (var combo in _active_combos)
                {
                    if (_combo_actions.TryGetValue(combo, out var actions))
                        actions.releaseAction?.Invoke();
                }
            }

            _active_combos.Clear();
        }

        public void ClearSequences()
        {
            _sequences.Clear();
        }

        /// <summary>
        /// Löscht alle Kombinationen, aktiven Kombinationen und Sequenzen.
        /// </summary>
        public void Clear()
        {
            ClearCombinations();
            ClearSequences();
        }

        public void RemoveActiveCombination(IEnumerable<TButton> buttons, bool triggerRelease = true)
        {
            var buttonSet = new HashSet<TButton>(buttons);
            var match = _active_combos.FirstOrDefault(c => c.SetEquals(buttonSet));

            if (match != null)
            {
                if (triggerRelease && _combo_actions.TryGetValue(match, out var actions))
                    actions.releaseAction?.Invoke();

                _active_combos.Remove(match);
            }
        }

        public void Update(GameContext context) => Update(context.GameTime);

        public abstract void Update(GameTime gameTime);

        protected void Update(IInputState<TButton> currentState, TimeSpan elapsed)
        {
            if (currentState == null)
                throw new ArgumentNullException(nameof(currentState));

            foreach (var kvp in _combo_actions)
            {
                var combo = kvp.Key;
                var (press_action, release_action) = kvp.Value;

                bool is_pressed_now = combo.All(b => currentState.IsButtonDown(b));
                bool was_pressed_before = combo.All(b => _previousState.IsButtonDown(b));

                if (is_pressed_now && !was_pressed_before)
                {
                    press_action.Invoke();
                    if (!_active_combos.Any(c => c.SetEquals(combo)))
                        _active_combos.Add(combo);
                }
                else if (!is_pressed_now && was_pressed_before)
                {
                    if (_active_combos.Any(c => c.SetEquals(combo)))
                    {
                        release_action?.Invoke();
                        _active_combos.RemoveAll(c => c.SetEquals(combo));
                    }
                }
            }

            foreach (var seq in _sequences)
            {
                seq.Update(currentState, _previousState, elapsed);
            }

            _previousState = currentState;
        }

        protected class SequenceEntry
        {
            public IList<TButton> Sequence { get; }
            public Action Action { get; }
            public TimeSpan Timeout { get; }

            private int _current_index = 0;
            private TimeSpan _time_since_last_button = TimeSpan.Zero;

            public SequenceEntry(IList<TButton> sequence, Action action, TimeSpan timeout)
            {
                Sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
                Action = action ?? throw new ArgumentNullException(nameof(action));
                Timeout = timeout;
            }

            public void Update(IInputState<TButton> current, IInputState<TButton> previous, TimeSpan elapsed)
            {
                _time_since_last_button += elapsed;

                if (_time_since_last_button > Timeout)
                {
                    _current_index = 0;
                    _time_since_last_button = TimeSpan.Zero;
                }

                if (_current_index >= Sequence.Count)
                    return;

                var button_to_match = Sequence[_current_index];

                if (current.IsButtonDown(button_to_match) && previous.IsButtonUp(button_to_match))
                {
                    _current_index++;
                    _time_since_last_button = TimeSpan.Zero;

                    if (_current_index == Sequence.Count)
                    {
                        Action.Invoke();
                        _current_index = 0;
                    }
                }
                else if (AnyOtherButtonPressed(current, previous, button_to_match))
                {
                    _current_index = 0;
                    _time_since_last_button = TimeSpan.Zero;
                }
            }

            private bool AnyOtherButtonPressed(IInputState<TButton> current, IInputState<TButton> previous, TButton exclude)
            {
                foreach (var btn in _allButtons)
                {
                    if (btn.Equals(exclude))
                        continue;

                    if (current.IsButtonDown(btn) && previous.IsButtonUp(btn))
                        return true;
                }

                return false;
            }
        }

        private class HashSetComparer<T> : IEqualityComparer<HashSet<T>>
        {
            public bool Equals(HashSet<T>? x, HashSet<T>? y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.SetEquals(y);
            }

            public int GetHashCode(HashSet<T>? obj)
            {
                if (obj == null)
                    return 0;

                int hash = 0;
                foreach (var item in obj)
                    hash ^= item?.GetHashCode() ?? 0;

                return hash;
            }
        }
    }
}