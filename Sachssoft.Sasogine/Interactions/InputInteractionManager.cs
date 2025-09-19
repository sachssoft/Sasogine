using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Interactions;

public abstract class InputInteractionManager<TButton> where TButton : struct, Enum
{
    protected static readonly TButton[] _all_buttons = Enum.GetValues<TButton>().Cast<TButton>().ToArray();

    protected IInputState<TButton> _previous_state;

    private readonly Dictionary<HashSet<TButton>, (Action press_action, Action? release_action)> _combo_actions = new(new HashSetComparer<TButton>());
    private readonly List<HashSet<TButton>> _active_combos = new();
    private readonly List<SequenceEntry> _sequences = new();

    public InputInteractionManager(IInputState<TButton> initial_state)
    {
        _previous_state = initial_state ?? throw new ArgumentNullException(nameof(initial_state));
    }

    public void AddCombination(IEnumerable<TButton> buttons, Action press_action, Action? release_action = null)
    {
        var button_set = new HashSet<TButton>(buttons);
        _combo_actions[button_set] = (press_action, release_action);
    }

    public void Add(TButton button, Action press_action, Action? release_action = null)
    {
        AddCombination(new[] { button }, press_action, release_action);
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

    public void ClearActiveCombinations(bool trigger_release_actions = true)
    {
        if (trigger_release_actions)
        {
            foreach (var combo in _active_combos)
            {
                if (_combo_actions.TryGetValue(combo, out var actions))
                    actions.release_action?.Invoke();
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

    public void RemoveActiveCombination(IEnumerable<TButton> buttons, bool trigger_release = true)
    {
        var button_set = new HashSet<TButton>(buttons);
        var match = _active_combos.FirstOrDefault(c => c.SetEquals(button_set));

        if (match != null)
        {
            if (trigger_release && _combo_actions.TryGetValue(match, out var actions))
                actions.release_action?.Invoke();

            _active_combos.Remove(match);
        }
    }

    public abstract void Update(GameFrameContext context);

    protected void Update(IInputState<TButton> current_state, TimeSpan elapsed)
    {
        if (current_state == null)
            throw new ArgumentNullException(nameof(current_state));

        foreach (var kvp in _combo_actions)
        {
            var combo = kvp.Key;
            var (press_action, release_action) = kvp.Value;

            bool is_pressed_now = combo.All(b => current_state.IsButtonDown(b));
            bool was_pressed_before = combo.All(b => _previous_state.IsButtonDown(b));

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
            seq.Update(current_state, _previous_state, elapsed);
        }

        _previous_state = current_state;
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
            foreach (var btn in _all_buttons)
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
