using sachssoft.Sasogine.Tiling;
using sachssoft.Sasogine.Tiling.Stacked;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


/// <summary>
/// Dynamisches Registry für Tile-Elemente mit eindeutiger ID-Zuordnung.
/// 
/// Diese Klasse verwaltet Tiles (Kacheln) und ordnet ihnen automatisch IDs zu.
/// Sie unterstützt das:
/// - Registrieren neuer Tiles mit automatischer ID-Vergabe.
/// - Reservieren von IDs für spätere Tile-Registrierungen (z. B. für System-Tiles).
/// - Wiederfinden von Tiles sowohl per Referenz als auch per inhaltsgleichem Vergleich (Equals).
/// - Entfernen und Aufräumen von registrierten Tiles mit optionalem Dispose-Aufruf.
/// 
/// Die Klasse eignet sich insbesondere für Spiele- oder Editor-Anwendungen,
/// bei denen Tiles flexibel, eindeutig identifizierbar und dynamisch verwaltet werden müssen.
/// 
/// Wichtige Eigenschaften:
/// - Keine doppelte Registrierung gleicher Tiles möglich.
/// - Verhindert ID-Duplikate durch Reservierungssystem.
/// - Effiziente Lookup-Methoden sowohl für ID → Tile als auch Tile → ID.
/// - Unterstützt IDisposable für Tiles, um Ressourcen sauber freizugeben.
/// 
/// Hinweis:
/// - Für korrekte Funktion sollte ITileElement.Equals und GetHashCode sinnvoll implementiert sein,
///   falls inhaltsbasierte Vergleiche gewünscht sind.
/// </summary>
public sealed class DynamicTileElementRegistry : ITileElementRegistry, IDisposable
{
    private uint _next_free_id = 0;
    private readonly Dictionary<uint, ITileElement> _id_to_tile = new();
    private readonly Dictionary<ITileElement, uint> _tile_to_id = new();
    private readonly HashSet<uint> _reserved_ids = new();

    private readonly IReadOnlyDictionary<uint, ITileElement> _readonly_id_to_tile;
    private readonly IReadOnlyDictionary<ITileElement, uint> _readonly_tile_to_id;

    public DynamicTileElementRegistry()
    {
        _readonly_id_to_tile = new ReadOnlyDictionary<uint, ITileElement>(_id_to_tile);
        _readonly_tile_to_id = new ReadOnlyDictionary<ITileElement, uint>(_tile_to_id);
    }

    /// <summary>
    /// Registriert ein neues Tile und gibt eine eindeutige ID zurück.
    /// Falls das Tile bereits registriert ist, wird dessen existierende ID zurückgegeben.
    /// </summary>
    /// <param name="tile">Das zu registrierende Tile-Element.</param>
    /// <returns>Eindeutige ID des Tiles.</returns>
    /// <exception cref="InvalidOperationException">Wenn das ID-Limit erreicht ist.</exception>
    public uint Register(ITileElement tile)
    {
        if (_tile_to_id.TryGetValue(tile, out var existing_id))
        {
            // Tile ist schon registriert, gib existierende ID zurück
            return existing_id;
        }

        _next_free_id++;

        if (_next_free_id == uint.MaxValue)
            throw new InvalidOperationException("Tile Id limit reached.");

        uint id = _next_free_id;
        _id_to_tile[id] = tile;
        _tile_to_id[tile] = id;

        return id;
    }

    /// <summary>
    /// Reserviert eine ID für späteres Registrieren eines Tiles.
    /// Nützlich für System-Tiles oder vorab definierte IDs.
    /// </summary>
    /// <returns>Reservierte ID.</returns>
    public uint ReserveId()
    {
        var id = _next_free_id++;
        _reserved_ids.Add(id);
        return id;
    }

    /// <summary>
    /// Registriert ein Tile unter einer zuvor reservierten ID.
    /// </summary>
    /// <param name="reserved_id">Vorab reservierte ID.</param>
    /// <param name="tile">Tile-Element zum Registrieren.</param>
    /// <returns>Die registrierte ID (reserved_id).</returns>
    /// <exception cref="InvalidOperationException">Bei nicht reservierter oder bereits belegter ID oder doppelter Tile-Registrierung.</exception>
    public uint RegisterReserved(uint reserved_id, ITileElement tile)
    {
        if (_id_to_tile.ContainsKey(reserved_id))
            throw new InvalidOperationException($"ID {reserved_id} is already registered.");

        if (_tile_to_id.ContainsKey(tile))
            throw new InvalidOperationException($"Tile {tile} is already registered.");

        if (!_reserved_ids.Contains(reserved_id))
            throw new InvalidOperationException($"ID {reserved_id} was not reserved.");

        _id_to_tile[reserved_id] = tile;
        _tile_to_id[tile] = reserved_id;
        _reserved_ids.Remove(reserved_id);

        return reserved_id;
    }

    /// <summary>
    /// Entfernt ein registriertes Tile aus der Registry.
    /// Ruft Dispose auf dem Tile auf, falls es IDisposable implementiert.
    /// </summary>
    /// <param name="tile">Das zu entfernende Tile.</param>
    /// <returns>True, wenn Tile gefunden und entfernt wurde; sonst false.</returns>
    public bool Unregister(ITileElement tile)
    {
        if (_tile_to_id.TryGetValue(tile, out var id))
        {
            _tile_to_id.Remove(tile);
            _id_to_tile.Remove(id);

            if (tile is IDisposable disposable)
                disposable.Dispose();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Entfernt ein registriertes Tile über die ID aus der Registry.
    /// Ruft Dispose auf dem Tile auf, falls es IDisposable implementiert.
    /// </summary>
    /// <param name="id">ID des zu entfernenden Tiles.</param>
    /// <returns>True, wenn Tile gefunden und entfernt wurde; sonst false.</returns>
    public bool Unregister(uint id)
    {
        if (_id_to_tile.TryGetValue(id, out var tile))
        {
            _id_to_tile.Remove(id);
            _tile_to_id.Remove(tile);

            if (tile is IDisposable disposable)
                disposable.Dispose();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Löscht alle registrierten Tiles und setzt die Registry zurück.
    /// Ruft Dispose auf allen Tiles auf, die IDisposable implementieren.
    /// </summary>
    public void Clear()
    {
        foreach (var tile in _id_to_tile.Values)
        {
            if (tile is IDisposable disposable)
                disposable.Dispose();
        }

        _id_to_tile.Clear();
        _tile_to_id.Clear();
        _reserved_ids.Clear();
        _next_free_id = 0;
    }

    /// <summary>
    /// Gibt Ressourcen frei und leert die Registry.
    /// </summary>
    public void Dispose()
    {
        Clear(); // Dispose aller registrierten Tiles intern aufrufen
    }

    /// <summary>
    /// Versucht, ein inhaltsgleiches Tile (basierend auf Equals) im Register zu finden.
    /// </summary>
    /// <param name="tile">Das zu suchende Tile.</param>
    /// <param name="id">Ausgabe: ID des gefundenen Tiles.</param>
    /// <returns>True, wenn ein inhaltsgleiches Tile gefunden wurde; sonst false.</returns>
    /// <remarks>
    /// Diese Methode ist hilfreich im Editor, wenn Tiles nicht nur über Referenzen,
    /// sondern auch über ihren Inhalt als identisch erkannt werden sollen.
    /// </remarks>
    public bool TryFindSameTile(ITileElement tile, out uint id)
    {
        foreach (var existing_tile in _tile_to_id.Keys)
        {
            if (existing_tile.Equals(tile))
            {
                id = _tile_to_id[existing_tile];
                return true;
            }
        }
        id = 0;
        return false;
    }

    /// <summary>
    /// Versucht, ein Tile anhand seiner ID zu finden.
    /// </summary>
    /// <param name="id">ID des Tiles.</param>
    /// <param name="tile">Ausgabe: Gefundenes Tile oder null.</param>
    /// <returns>True, wenn das Tile gefunden wurde; sonst false.</returns>
    public bool TryGetTile(uint id, out ITileElement? tile) => _id_to_tile.TryGetValue(id, out tile);

    /// <summary>
    /// Versucht, die ID eines registrierten Tiles zu ermitteln.
    /// </summary>
    /// <param name="tile">Das Tile, dessen ID gesucht wird.</param>
    /// <param name="id">Ausgabe: ID des Tiles.</param>
    /// <returns>True, wenn das Tile registriert ist; sonst false.</returns>
    public bool TryGetId(ITileElement tile, out uint id) => _tile_to_id.TryGetValue(tile, out id);

    /// <summary>
    /// Die nächste freie ID, die bei Register() verwendet wird.
    /// </summary>
    public uint NextFreeId => _next_free_id;

    /// <summary>
    /// Readonly-View auf die Zuordnung von IDs zu Tiles.
    /// </summary>
    public IReadOnlyDictionary<uint, ITileElement> IdToTile => _readonly_id_to_tile;

    /// <summary>
    /// Readonly-View auf die Zuordnung von Tiles zu IDs.
    /// </summary>
    public IReadOnlyDictionary<ITileElement, uint> TileToId => _readonly_tile_to_id;

    /// <summary>
    /// Enumerator über alle registrierten Tiles.
    /// </summary>
    /// <returns>Enumerator über alle Tiles.</returns>
    public IEnumerator<ITileElement> GetEnumerator() => _id_to_tile.Values.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Debug-Ausgabe mit Anzahl registrierter Tiles und nächster freier ID.
    /// </summary>
    public override string ToString()
    {
        return $"DynamicTileElementRegistry [Count={_id_to_tile.Count}, NextFreeId={_next_free_id}]";
    }
}
