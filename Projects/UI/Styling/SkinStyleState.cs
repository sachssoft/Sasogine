namespace Sachssoft.Engine.Controls.Styling;
/// <summary>Remembers one skin contribution while preserving subsequent application style edits.</summary>
internal sealed class SkinStyleState
{
    private readonly Style? _original;
    private readonly Style _applied;
    internal SkinStyleState(Style? original, Style applied)
    {
        _original = original is null ? null : Copy(original);
        _applied = Copy(applied);
    }

    internal static Style Copy(Style source)
    {
        var result = new Style
        {
            BasedOn = source.BasedOn
        };
        foreach (var entry in source.Values)
            result[entry.Key] = entry.Value;
        return result;
    }

    internal Style? Restore(Style current)
    {
        Style result = _original is null ? new Style
        {
            BasedOn = "Control"
        }

        : Copy(_original);
        bool edited = false;
        if (current.BasedOn != _applied.BasedOn)
        {
            result.BasedOn = current.BasedOn;
            edited = true;
        }

        foreach (var entry in current.Values)
            if (!_applied.Values.TryGetValue(entry.Key, out string? applied) || applied != entry.Value)
            {
                result[entry.Key] = entry.Value;
                edited = true;
            }

        foreach (var entry in _applied.Values)
            if (!current.Values.ContainsKey(entry.Key))
            {
                result.Remove(entry.Key);
                edited = true;
            }

        return _original is null && !edited ? null : result;
    }
}
