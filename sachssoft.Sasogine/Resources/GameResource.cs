using System.Collections.Generic;

namespace sachssoft.Sasogine.Resource;

public abstract class GameResource
{
    private Dictionary<string, object> _entries;

    public GameResource()
    {
        _entries = new();
    }

    public object? GetResource(string key)
    {
        if (_entries.TryGetValue(key, out var value)) 
            return value;

        throw new GameException("Resource key not found");
    }

}
