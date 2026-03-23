using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common
{
    // Lightweight Buffer für Werteänderungen.
    // ------------------------------------------------------------------------------------------------------
    // Diese Struktur speichert den alten Wert (OldValue) und den aktuellenneuen Wert (NewValue).
    // Sie wird typischerweise in der Game-Update-Schleife verwendet, um Änderungen framebasiert zu erkennen.

    // Vorteile gegenüber Event-Handlern:
    // - Sehr performant, da keine Events oder Delegates aufgerufen werden.
    // - Keine Heap-Allocation (Struct statt Class), minimaler Speicherverbrauch.
    // - Ideal für häufige Updates in MonoGame (z.B.Score, Position, Status).

    /// <summary>
    /// Lightweight buffer holding old and new values.
    /// 
    /// Tracks changes of a value frame-by-frame or per update.
    /// The first assignment (via <see cref="Set"/> or <see cref="EnsureChange"/>)
    /// will always mark a change (returns true) to ensure initialization logic runs.
    /// 
    /// Advantages over events:
    /// - Very performant (no delegates/events called)
    /// - Struct-based, no heap allocation
    /// - Ideal for frequent updates in MonoGame or gameplay loops (score, position, status)
    /// </summary>
    /// <typeparam name="T">Type of the buffered value.</typeparam>
    public struct ValueBuffer<T>
    {
        private bool _wasEnsured;

        /// <summary>Previous value.</summary>
        public T OldValue { get; private set; }

        /// <summary>Current / new value.</summary>
        public T NewValue { get; private set; }

        /// <summary>True if OldValue and NewValue differ.</summary>
        public bool HasChanged => !EqualityComparer<T>.Default.Equals(OldValue, NewValue);

        /// <summary>True if the buffer has never been set or ensured.</summary>
        public bool WasEnsured => _wasEnsured;

        /// <summary>
        /// Initializes the buffer as empty (first set will initialize both Old and New).
        /// </summary>
        public ValueBuffer()
        {
            OldValue = default!;
            NewValue = default!;
        }

        /// <summary>
        /// Explicitly sets a new value.
        /// Updates OldValue to the previous NewValue.
        /// First assignment initializes both Old and New.
        /// </summary>
        public void Set(T value)
        {
            OldValue = NewValue;
            NewValue = value;
        }

        /// <summary>
        /// Checks if the value changed and automatically updates OldValue -> NewValue if it did.
        /// Returns true if a change occurred, or if it's the first assignment.
        /// </summary>
        public bool EnsureChange(T value)
        {
            if (!_wasEnsured || !EqualityComparer<T>.Default.Equals(NewValue, value))
            {
                OldValue = NewValue;
                NewValue = value;
                _wasEnsured = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Resets both OldValue and NewValue to the specified value.
        /// Marks first assignment as handled.
        /// </summary>
        public void Reset(T value)
        {
            OldValue = value;
            NewValue = value;
        }

        public override string ToString() => $"Old: {OldValue}, New: {NewValue}, First: {_wasEnsured}";
    }
}

