using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls.Styling;
/// <summary>Stores explicitly registered style IDs and polls style versions outside control value reads.</summary>
public sealed class StyleRegistry
{
    private readonly Dictionary<string, StyleRegistryEntry> _styles = new(StringComparer.Ordinal);
    private int _revision;
    internal int Revision => _revision;
    /// <summary>Gets the number of named styles.</summary>
    public int Count => _styles.Count;
    /// <summary>Gets style IDs.</summary>
    public IEnumerable<string> Keys => _styles.Keys;

    /// <summary>Gets a registered style.</summary>
    public Style this[string id] => _styles[id].Style;
    internal void RemoveSkinStyle(string id)
    {
        if (_styles.Remove(id))
            unchecked
            {
                _revision++;
            }
    }

    /// <summary>Registers a new named style.</summary>
    public void Register(string id, Style style)
    {
        ValidateIdentity(id, style);
        _styles.Add(id, new StyleRegistryEntry(style));
        unchecked
        {
            _revision++;
        }
    }

    /// <summary>Registers a style using its ID.</summary>
    public void Register(Style style) => Register(style.Id, style);
    /// <summary>Registers or replaces a style.</summary>
    public void Set(string id, Style style)
    {
        ValidateIdentity(id, style);
        _styles[id] = new StyleRegistryEntry(style);
        unchecked
        {
            _revision++;
        }
    }

    private static void ValidateIdentity(string id, Style style)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(style);
        if (style.Id.Length > 0 && style.Id != id)
            throw new ArgumentException("Style ID differs from its registry key.", nameof(style));
        style.Id = id;
    }

    /// <summary>Gets a style without discovery.</summary>
    public bool TryGet(string id, out Style? style)
    {
        bool found = _styles.TryGetValue(id, out var entry);
        style = entry?.Style;
        return found;
    }

    internal void PollChanges()
    {
        foreach (var entry in _styles.Values)
            if (entry.Version != entry.Style.Version)
            {
                entry.Version = entry.Style.Version;
                unchecked
                {
                    _revision++;
                }
            }
    }

    /// <summary>Validates every base ID and rejects cycles.</summary>
    public void Validate(string id)
    {
        string? current = id;
        int depth = 0;
        while (current is not null)
        {
            if (++depth > Count)
                throw new InvalidOperationException("Cyclic style inheritance.");
            current = this[current].BasedOn;
        }
    }

    internal void ApplyChain(Control control, string id, int depth = 0)
    {
        if (depth >= Count)
            throw new InvalidOperationException("Cyclic style inheritance.");
        Style style = this[id];
        if (style.BasedOn is string parent)
            ApplyChain(control, parent, depth + 1);
        control.ApplyStyle(style);
    }

    internal bool TryResolve(string id, string property, out string? value)
    {
        int depth = 0;
        string? current = id;
        while (current is not null)
        {
            if (++depth > Count)
                throw new InvalidOperationException("Cyclic style inheritance.");
            Style style = this[current];
            if (style.Values.TryGetValue(property, out value))
                return true;
            current = style.BasedOn;
        }

        value = null;
        return false;
    }
}
