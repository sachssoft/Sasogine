using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

/// <summary>
/// Tracks modified properties and their associated dirty actions.
/// Allows querying, consuming, and clearing pending changes.
/// </summary>
public sealed class DirtyMark
{
    private readonly Dictionary<string, DirtyAction> _marks = new();

    /// <summary>
    /// Gets a value indicating whether any dirty marks are active.
    /// </summary>
    public bool Active => _marks.Count > 0;

    /// <summary>
    /// Marks a property as dirty with the specified action flags.
    /// </summary>
    /// <param name="propertyName">The name of the modified property.</param>
    /// <param name="action">The dirty actions associated with the modification.</param>
    public void Add(
        string propertyName,
        DirtyAction action = DirtyAction.None)
    {
        if (_marks.TryGetValue(propertyName, out var current))
        {
            _marks[propertyName] = current | action;
        }
        else
        {
            _marks.Add(propertyName, action);
        }
    }

    /// <summary>
    /// Gets the dirty actions assigned to a property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The dirty actions assigned to the property.</returns>
    public DirtyAction Get(string propertyName)
    {
        return _marks.TryGetValue(propertyName, out var action)
            ? action
            : DirtyAction.None;
    }

    /// <summary>
    /// Consumes all dirty actions of a property and removes the property mark.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The consumed dirty actions.</returns>
    public DirtyAction Consume(string propertyName)
    {
        if (_marks.TryGetValue(propertyName, out var action))
        {
            _marks.Remove(propertyName);
            return action;
        }

        return DirtyAction.None;
    }

    /// <summary>
    /// Consumes the dirty actions of a property and checks whether the specified action flags were set.
    /// </summary>
    /// <param name="propertyName">The name of the property to consume.</param>
    /// <param name="action">The dirty action flags to check.</param>
    /// <returns>
    /// True if the consumed dirty actions contain the specified flags; otherwise false.
    /// </returns>
    public bool Consume(string propertyName, DirtyAction action)
    {
        return (Consume(propertyName) & action) != 0;
    }

    /// <summary>
    /// Consumes all properties containing the specified dirty action flags.
    /// </summary>
    /// <param name="action">The dirty action flags to consume.</param>
    /// <returns>
    /// True if at least one property contained the specified action flags;
    /// otherwise false.
    /// </returns>
    public bool Consume(DirtyAction action)
    {
        var consumed = false;
        var properties = new List<string>();

        foreach (var pair in _marks)
        {
            if ((pair.Value & action) != 0)
            {
                consumed = true;

                var remaining = pair.Value & ~action;

                if (remaining == DirtyAction.None)
                {
                    properties.Add(pair.Key);
                }
                else
                {
                    _marks[pair.Key] = remaining;
                }
            }
        }

        foreach (var propertyName in properties)
        {
            _marks.Remove(propertyName);
        }

        return consumed;
    }

    /// <summary>
    /// Determines whether any property contains the specified dirty action flags.
    /// </summary>
    /// <param name="action">The dirty action flags to check.</param>
    /// <returns>
    /// True if a matching dirty action exists; otherwise false.
    /// </returns>
    public bool HasAction(DirtyAction action)
    {
        foreach (var value in _marks.Values)
        {
            if ((value & action) != 0)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets and consumes all properties containing the specified dirty action flags.
    /// </summary>
    /// <param name="action">The dirty action flags to match.</param>
    /// <returns>The names of consumed properties.</returns>
    public IEnumerable<string> TakeProperties(DirtyAction action)
    {
        var result = new List<string>();

        foreach (var pair in _marks)
        {
            if ((pair.Value & action) != 0)
            {
                result.Add(pair.Key);
            }
        }

        foreach (var propertyName in result)
        {
            Consume(propertyName);
        }

        return result;
    }

    /// <summary>
    /// Removes all dirty marks.
    /// </summary>
    public void Clear()
    {
        _marks.Clear();
    }
}