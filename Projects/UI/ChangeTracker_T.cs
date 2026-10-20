using System.Collections.Generic;

namespace Sachssoft.Engine.Controls;
/// <summary>Tracks algorithm state such as selection transitions; never stores style or local overrides.</summary>
/// <typeparam name = "T">The tracked state type.</typeparam>
internal struct ChangeTracker<T>
{
    private T _value;
    internal bool WasEnsured { get; private set; }
    internal readonly T NewValue => _value;

    internal bool EnsureChange(T value)
    {
        bool changed = !WasEnsured || !EqualityComparer<T>.Default.Equals(_value, value);
        _value = value;
        WasEnsured = true;
        return changed;
    }

    internal void Set(T value)
    {
        _value = value;
        WasEnsured = true;
    }
}
