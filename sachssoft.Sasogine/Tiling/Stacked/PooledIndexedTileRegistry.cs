using sachssoft.Sasogine.Tiling;
using System.Buffers;
using System.Collections.Generic;

using System;
using System.Diagnostics.CodeAnalysis;
using sachssoft.Sasogine.Tiling.Stacked;

/// <summary>
/// Tile-Registry mit fester Kapazität, die intern ein gepooltes Array zur Speicherung nutzt
/// und zusätzlich eine bidirektionale Zuordnung (Dictionary) von Tiles zu IDs ermöglicht.
///
/// Diese Implementierung kombiniert den schnellen Indexzugriff eines Arrays mit einer
/// schnellen Umkehrsuche von Tile-Objekten zu ihren IDs über ein Dictionary.
///
/// Verwendungszweck:
/// - Verwalten einer festen Anzahl von Tile-Elementen mit direktem Indexzugriff.
/// - Effiziente Speicherverwaltung durch Verwendung von ArrayPool zur Vermeidung von
///   unnötigen Speicherallokationen.
/// - Schnelle Suche von Tile -> ID mittels Dictionary.
/// - Automatische Freigabe von IDisposable-Implementierungen bei Entfernen.
/// 
/// Vorteile:
/// - Geringere Speicherallokationen dank ArrayPool.
/// - Schneller Lookup in beide Richtungen (ID -> Tile, Tile -> ID).
/// - Kontrollierte Ressourcenfreigabe.
/// 
/// Einschränkungen:
/// - Feste Kapazität beim Erstellen.
/// - Kein dynamisches Vergrößern möglich.
/// - Tiles werden per Referenz verglichen (ReferenceEquality).
/// </summary>
public sealed class PooledIndexedTileRegistry : ITileElementRegistry, IDisposable
{
    private ITileElement?[] _tiles;                     // Gepooltes Array der Tiles, Index = ID
    private readonly Dictionary<ITileElement, uint> _tile_to_id;  // Umkehr-Map: Tile -> ID

    private readonly int _capacity;                     // Maximale Kapazität
    private bool _disposed;                             // Dispose-Flag

    /// <summary>
    /// Erstellt eine neue Registry mit vorgegebener Kapazität.
    /// </summary>
    /// <param name="capacity">Maximale Anzahl der registrierbaren Tiles.</param>
    public PooledIndexedTileRegistry(int capacity)
    {
        _capacity = capacity;
        // Ausleihen eines Arrays aus dem ArrayPool zur Minimierung von Speicherallokationen
        _tiles = ArrayPool<ITileElement?>.Shared.Rent(capacity);
        Array.Clear(_tiles, 0, capacity);

        // Dictionary für schnellen Rückwärtszugriff (Tile -> ID), mit Referenzgleichheit als Vergleich
        _tile_to_id = new Dictionary<ITileElement, uint>(capacity, ReferenceEqualityComparer.Instance);
    }

    /// <summary>
    /// Registriert ein Tile unter einer bestimmten ID.
    /// </summary>
    /// <param name="id">ID/Index, unter der das Tile registriert wird.</param>
    /// <param name="tile">Tile-Element.</param>
    /// <exception cref="ObjectDisposedException">Wenn die Registry bereits disposed ist.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Wenn die ID außerhalb der Kapazität liegt.</exception>
    /// <exception cref="InvalidOperationException">Wenn Slot besetzt ist oder Tile bereits registriert.</exception>
    public void Register(uint id, ITileElement tile)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledIndexedTileRegistry));
        if (id >= _capacity) throw new ArgumentOutOfRangeException(nameof(id));
        if (_tiles[id] != null) throw new InvalidOperationException("Slot already occupied.");
        if (_tile_to_id.ContainsKey(tile)) throw new InvalidOperationException("Tile already registered.");

        _tiles[id] = tile;
        _tile_to_id[tile] = id;
    }

    /// <summary>
    /// Ersetzt das Tile an einer ID mit einem neuen Tile.
    /// </summary>
    /// <param name="id">ID, die ersetzt wird.</param>
    /// <param name="tile">Neues Tile.</param>
    /// <exception cref="ObjectDisposedException">Wenn disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Ungültige ID.</exception>
    /// <exception cref="InvalidOperationException">Wenn Tile an anderer ID registriert ist.</exception>
    public void Replace(uint id, ITileElement tile)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledIndexedTileRegistry));
        if (id >= _capacity) throw new ArgumentOutOfRangeException(nameof(id));

        var current = _tiles[id];

        if (_tile_to_id.TryGetValue(tile, out var otherId) && otherId != id)
            throw new InvalidOperationException("Tile is already registered under another ID.");

        if (current is IDisposable disposable)
            disposable.Dispose();

        if (current != null)
            _tile_to_id.Remove(current);

        _tiles[id] = tile;
        _tile_to_id[tile] = id;
    }

    /// <summary>
    /// Entfernt ein Tile anhand seiner ID aus der Registry.
    /// </summary>
    /// <param name="id">ID des Tiles.</param>
    /// <exception cref="ObjectDisposedException">Wenn disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Ungültige ID.</exception>
    public void Unregister(uint id)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledIndexedTileRegistry));
        if (id >= _capacity) throw new ArgumentOutOfRangeException(nameof(id));

        var tile = _tiles[id];

        if (tile is IDisposable disposable)
            disposable.Dispose();

        _tiles[id] = null;

        if (tile != null)
            _tile_to_id.Remove(tile);
    }

    /// <summary>
    /// Versucht, ein Tile anhand seiner ID zu finden.
    /// </summary>
    /// <param name="id">ID des Tiles.</param>
    /// <param name="tile">Ausgabe: Tile falls gefunden.</param>
    /// <returns>True, wenn gefunden.</returns>
    /// <exception cref="ObjectDisposedException">Wenn disposed.</exception>
    public bool TryGetTile(uint id, [MaybeNullWhen(false)] out ITileElement? tile)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledIndexedTileRegistry));
        if (id >= _capacity)
        {
            tile = null;
            return false;
        }

        tile = _tiles[id];
        return tile != null;
    }

    /// <summary>
    /// Versucht, die ID eines Tiles zu ermitteln.
    /// </summary>
    /// <param name="tile">Tile-Element.</param>
    /// <param name="id">Ausgabe: ID falls gefunden.</param>
    /// <returns>True, wenn gefunden.</returns>
    /// <exception cref="ObjectDisposedException">Wenn disposed.</exception>
    public bool TryGetId(ITileElement tile, out uint id)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledIndexedTileRegistry));
        return _tile_to_id.TryGetValue(tile, out id);
    }

    /// <summary>
    /// Prüft, ob unter einer ID ein Tile registriert ist.
    /// </summary>
    /// <param name="id">ID zu prüfen.</param>
    /// <returns>True, wenn registriert.</returns>
    /// <exception cref="ObjectDisposedException">Wenn disposed.</exception>
    public bool IsRegistered(uint id)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledIndexedTileRegistry));
        if (id >= _capacity) throw new ArgumentOutOfRangeException(nameof(id));

        return _tiles[id] != null;
    }

    /// <summary>
    /// Prüft, ob ein Tile registriert ist.
    /// </summary>
    /// <param name="tile">Zu prüfendes Tile.</param>
    /// <returns>True, wenn registriert.</returns>
    /// <exception cref="ObjectDisposedException">Wenn disposed.</exception>
    public bool Contains(ITileElement tile)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledIndexedTileRegistry));
        return _tile_to_id.ContainsKey(tile);
    }

    /// <summary>
    /// Löscht alle Registrierungen und gibt ggf. Ressourcen frei.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Wenn disposed.</exception>
    public void Clear()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledIndexedTileRegistry));

        for (int i = 0; i < _capacity; i++)
        {
            if (_tiles[i] is IDisposable disposable)
                disposable.Dispose();

            _tiles[i] = null;
        }

        _tile_to_id.Clear();
    }

    /// <summary>
    /// Gibt Ressourcen frei und gibt das gepoolte Array zurück.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        Clear();
        ArrayPool<ITileElement?>.Shared.Return(_tiles, clearArray: true);
        _tiles = Array.Empty<ITileElement?>();
        _disposed = true;
    }

    /// <summary>
    /// Enumeration aller registrierten Tiles.
    /// </summary>
    public IEnumerator<ITileElement> GetEnumerator()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledIndexedTileRegistry));
        for (int i = 0; i < _capacity; i++)
        {
            var tile = _tiles[i];
            if (tile != null)
                yield return tile;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
