using System.Collections.Generic;

namespace Sachssoft.Engine.Controls.Styling;
/// <summary>Stores a direct effective value and the latest style value without a property registry.</summary>
/// <typeparam name = "T">The compile-time-known value type.</typeparam>
public struct StyleProperty<T>
{
    private T _value, _styled;
    private bool _hasLocalValue, _applying;
    /// <summary>Creates a property with its fallback value.</summary>
    public StyleProperty(T fallback)
    {
        _value = _styled = fallback;
        _hasLocalValue = false;
        _applying = false;
    }

    /// <summary>Gets the effective value with no style lookup.</summary>
    public readonly T Value => _value;
    /// <summary>Gets whether an explicit assignment overrides styling, even when equal to the default.</summary>
    public readonly bool HasLocalValue => _hasLocalValue;

    /// <summary>Assigns a local value and reports an effective change.</summary>
    public bool SetLocal(T value)
    {
        _hasLocalValue = true;
        return Set(value);
    }

    /// <summary>Assigns a local value using an explicit non-boxing comparer for external value types.</summary>
    public bool SetLocal(T value, IEqualityComparer<T> comparer)
    {
        _hasLocalValue = true;
        if (comparer.Equals(_value, value))
            return false;
        _value = value;
        return true;
    }

    /// <summary>Stores the latest style value and applies it unless locally overridden.</summary>
    public bool Apply(T value)
    {
        _styled = value;
        return !_applying && !_hasLocalValue && Set(value);
    }

    /// <summary>Clears the local assignment and restores the latest style value.</summary>
    public bool ClearLocal()
    {
        _hasLocalValue = false;
        _applying = false;
        return Set(_styled);
    }

    private bool Set(T value)
    {
        if (EqualityComparer<T>.Default.Equals(_value, value))
            return false;
        _value = value;
        return true;
    }

    /// <summary>Starts a style transaction without exposing intermediate inheritance values.</summary>
    public bool BeginStyle(T fallback)
    {
        _applying = true;
        _styled = fallback;
        return false;
    }

    /// <summary>Commits the final inherited value and reports one effective change.</summary>
    public bool CommitStyle()
    {
        _applying = false;
        return !_hasLocalValue && Set(_styled);
    }
}
