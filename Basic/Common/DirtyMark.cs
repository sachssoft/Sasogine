using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

public sealed class DirtyMark
{
    private readonly Dictionary<string, DirtyAction> _marks = new();

    public bool Active => _marks.Count > 0;

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

    public DirtyAction Get(string propertyName)
    {
        return _marks.TryGetValue(propertyName, out var action)
            ? action
            : DirtyAction.None;
    }

    public DirtyAction Take(string propertyName)
    {
        if (_marks.TryGetValue(propertyName, out var action))
        {
            _marks.Remove(propertyName);
            return action;
        }

        return DirtyAction.None;
    }

    public bool HasAction(DirtyAction actions)
    {
        foreach (var action in _marks.Values)
        {
            if ((action & actions) != 0)
                return true;
        }

        return false;
    }

    public IEnumerable<string> TakeProperties(DirtyAction actions)
    {
        var result = new List<string>();

        foreach (var pair in _marks)
        {
            if ((pair.Value & actions) != 0)
                result.Add(pair.Key);
        }

        foreach (var propertyName in result)
        {
            _marks.Remove(propertyName);
        }

        return result;
    }

    public void Clear()
    {
        _marks.Clear();
    }
}