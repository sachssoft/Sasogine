using Sachssoft.Sasogine.Tiling;
using System.Collections.Generic;

using System;
using Sachssoft.Sasogine.Tiling.Stacked;

/// <summary>
/// Registry für Tile-Elemente mit fester, vorgegebener Größe und direktem Indexzugriff.
/// 
/// Diese Klasse verwaltet eine fest dimensionierte Sammlung von Tiles, die über einen
/// ganzzahligen Identifier (Index) angesprochen werden. Jeder Index kann genau ein Tile
/// halten oder null sein (frei).
/// 
/// Verwendungsszenarien:
/// - Wenn die maximale Anzahl der Tiles im Voraus bekannt und begrenzt ist.
/// - Wenn schnelle, einfache und speicherplatz-effiziente Zuordnung von IDs zu Tiles
///   gewünscht ist.
/// - Wenn keine dynamische ID-Vergabe notwendig ist (im Gegensatz zu DynamicTileElementRegistry).
/// 
/// Wichtige Eigenschaften:
/// - Register, Unregister und Replace von Tiles per Index.
/// - Verhindert doppelte Registrierung desselben Tiles unter verschiedenen IDs.
/// - Unterstützt IDisposable für Tiles, um Ressourcen beim Entfernen freizugeben.
/// - Schneller direkter Zugriff über Array-Indexierung.
/// - Einfach zu implementieren und sehr performant bei festem Tile-Bestand.
/// 
/// Einschränkungen:
/// - Maximale Anzahl der Tiles ist bei Konstruktion festgelegt.
/// - Keine automatische ID-Vergabe, IDs müssen explizit bekannt sein.
/// - Keine inhaltsbasierte Suche oder Vergleich, nur Referenzvergleich.
/// </summary>
public sealed class IndexedTileRegistry : ITileElementRegistry, IDisposable
{
    private readonly ITileElement?[] _registered_tiles;

    /// <summary>
    /// Initialisiert eine neue Registry mit fester Größe.
    /// </summary>
    /// <param name="count">Maximale Anzahl der registrierbaren Tiles.</param>
    public IndexedTileRegistry(int count)
    {
        _registered_tiles = new ITileElement?[count];
    }

    /// <summary>
    /// Registriert ein Tile unter einem bestimmten Index.
    /// </summary>
    /// <param name="identifier">Index für das Tile (muss im Bereich sein).</param>
    /// <param name="element">Tile-Element zum Registrieren.</param>
    /// <exception cref="ArgumentOutOfRangeException">Wenn der Index außerhalb des zulässigen Bereichs liegt.</exception>
    /// <exception cref="InvalidOperationException">Wenn der Index bereits belegt oder das Tile schon registriert ist.</exception>
    public void Register(uint identifier, ITileElement element)
    {
        if (identifier >= _registered_tiles.Length)
            throw new ArgumentOutOfRangeException(nameof(identifier));

        if (_registered_tiles[identifier] != null)
            throw new InvalidOperationException($"Identifier {identifier} is already registered.");

        if (Contains(element))
            throw new InvalidOperationException("This tile is already registered under a different identifier.");

        _registered_tiles[identifier] = element;
    }

    /// <summary>
    /// Entfernt ein Tile unter dem angegebenen Index aus der Registry.
    /// Ruft Dispose auf dem Tile auf, falls es IDisposable implementiert.
    /// </summary>
    /// <param name="identifier">Index des zu entfernenden Tiles.</param>
    /// <exception cref="ArgumentOutOfRangeException">Wenn der Index ungültig ist.</exception>
    public void Unregister(uint identifier)
    {
        if (identifier >= _registered_tiles.Length)
            throw new ArgumentOutOfRangeException(nameof(identifier));

        if (_registered_tiles[identifier] is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _registered_tiles[identifier] = null;
    }

    /// <summary>
    /// Ersetzt das Tile an einem bestimmten Index durch ein neues Tile.
    /// </summary>
    /// <param name="identifier">Index des Tiles, das ersetzt werden soll.</param>
    /// <param name="element">Neues Tile-Element.</param>
    /// <exception cref="ArgumentOutOfRangeException">Wenn der Index ungültig ist.</exception>
    /// <exception cref="InvalidOperationException">Wenn das neue Tile bereits an anderer Stelle registriert ist.</exception>
    public void Replace(uint identifier, ITileElement element)
    {
        if (identifier >= _registered_tiles.Length)
            throw new ArgumentOutOfRangeException(nameof(identifier));

        var current = _registered_tiles[identifier];

        if (Contains(element) && !ReferenceEquals(current, element))
            throw new InvalidOperationException("This tile is already registered under a different identifier.");

        if (current is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _registered_tiles[identifier] = element;
    }

    /// <summary>
    /// Prüft, ob unter einem bestimmten Index ein Tile registriert ist.
    /// </summary>
    /// <param name="identifier">Index zur Überprüfung.</param>
    /// <returns>True, wenn ein Tile registriert ist; sonst false.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Wenn der Index ungültig ist.</exception>
    public bool IsRegistered(uint identifier)
    {
        if (identifier >= _registered_tiles.Length)
            throw new ArgumentOutOfRangeException(nameof(identifier));

        return _registered_tiles[identifier] != null;
    }

    /// <summary>
    /// Löscht alle Registrierungen und ruft Dispose auf allen registrierten Tiles auf.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _registered_tiles.Length; i++)
        {
            if (_registered_tiles[i] is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _registered_tiles[i] = null;
        }
    }

    /// <summary>
    /// Gibt Ressourcen frei und leert die Registry.
    /// </summary>
    public void Dispose()
    {
        Clear();
    }

    /// <summary>
    /// Versucht, ein Tile anhand seines Index zu finden.
    /// </summary>
    /// <param name="id">Index des Tiles.</param>
    /// <param name="tile">Ausgabe: Tile, falls gefunden.</param>
    /// <returns>True, wenn das Tile gefunden wurde; sonst false.</returns>
    public bool TryGetTile(uint id, out ITileElement? tile)
    {
        if (id >= _registered_tiles.Length)
        {
            tile = null;
            return false;
        }

        tile = _registered_tiles[id];
        return tile != null;
    }

    /// <summary>
    /// Versucht, die ID (Index) eines registrierten Tiles zu ermitteln.
    /// </summary>
    /// <param name="tile">Das gesuchte Tile.</param>
    /// <param name="id">Ausgabe: Index, falls gefunden.</param>
    /// <returns>True, wenn das Tile gefunden wurde; sonst false.</returns>
    public bool TryGetId(ITileElement tile, out uint id)
    {
        if (tile == null) throw new ArgumentNullException(nameof(tile));

        for (uint i = 0; i < _registered_tiles.Length; i++)
        {
            if (ReferenceEquals(_registered_tiles[i], tile))
            {
                id = i;
                return true;
            }
        }

        id = 0;
        return false;
    }

    /// <summary>
    /// Prüft, ob ein bestimmtes Tile bereits registriert ist.
    /// </summary>
    /// <param name="tile">Zu prüfendes Tile.</param>
    /// <returns>True, wenn das Tile registriert ist; sonst false.</returns>
    public bool Contains(ITileElement tile)
    {
        if (tile == null) throw new ArgumentNullException(nameof(tile));

        for (int i = 0; i < _registered_tiles.Length; i++)
        {
            if (ReferenceEquals(_registered_tiles[i], tile))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Enumerator über alle aktuell registrierten Tiles.
    /// </summary>
    /// <returns>Enumeration aller registrierten Tiles.</returns>
    public IEnumerator<ITileElement> GetEnumerator()
    {
        foreach (var tile in _registered_tiles)
        {
            if (tile != null)
                yield return tile;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
