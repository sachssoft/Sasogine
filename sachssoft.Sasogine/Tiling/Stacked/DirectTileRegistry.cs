using sachssoft.Sasogine.Tiling;
using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Eine speicher- und performance-optimierte Registry für Tile-Elemente mit fester Kapazität.
/// 
/// Intern wird ein Array von Referenzen auf <typeparamref name="T"/> verwendet, 
/// wobei <typeparamref name="T"/> eine Klasse sein muss, die <see cref="ITileElement"/> implementiert.
/// 
/// Der Zugriff auf Elemente erfolgt primär über einen numerischen Identifier (Index).
/// Die Klasse bietet schnelle direkte Referenzzugriffe (via <see cref="GetElementByRef"/>) ohne zusätzliche Überprüfungen,
/// wodurch sie sich insbesondere für Performance-kritische Szenarien eignet, in denen bekannte und geprüfte Indizes verwendet werden.
/// 
/// Kein automatisches Wachstum des internen Speichers: Die Kapazität wird bei der Instanziierung festgelegt und bleibt konstant.
/// 
/// Wichtig:
/// - Die Methoden prüfen Bounds und werfen Ausnahmen bei ungültigen Indizes.
/// - Die direkte Referenzmethode <see cref="GetElementByRef"/> verzichtet aus Performance-Gründen auf Bounds-Checks.
/// - Es wird keine doppelte Registrierung desselben Tile-Elements erlaubt.
/// 
/// Diese Klasse eignet sich besonders für Spiele- oder Editor-Systeme, 
/// in denen Tiles mit festen Indizes und hoher Zugriffsgeschwindigkeit verwaltet werden sollen,
/// z. B. bei Tile-Maps, Level-Editoren oder Rendering-Systemen.
/// </summary>
/// <typeparam name="T">Der Tile-Element-Typ, muss Klasse und <see cref="ITileElement"/> sein.</typeparam>
public unsafe class DirectTileRegistry<T> where T : class, ITileElement
{
    private readonly T?[] _tiles;

    /// <summary>
    /// Erstellt eine neue Registry mit der angegebenen Kapazität.
    /// </summary>
    /// <param name="capacity">Maximale Anzahl von Tile-Elementen, die registriert werden können.</param>
    /// <exception cref="ArgumentOutOfRangeException">Wenn capacity kleiner oder gleich 0 ist.</exception>
    public DirectTileRegistry(int capacity)
    {
        if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _tiles = new T?[capacity];
    }

    /// <summary>
    /// Anzahl der maximal registrierbaren Tile-Elemente.
    /// </summary>
    public int Capacity => _tiles.Length;

    /// <summary>
    /// Registriert ein Tile-Element unter einem bestimmten Identifier.
    /// </summary>
    /// <param name="identifier">Der Index, unter dem das Tile gespeichert wird. Muss im Bereich [0, Capacity) liegen.</param>
    /// <param name="tile">Das Tile-Element, das registriert werden soll.</param>
    /// <exception cref="ArgumentOutOfRangeException">Wenn der Identifier außerhalb des gültigen Bereichs liegt.</exception>
    /// <exception cref="InvalidOperationException">Wenn bereits ein Tile unter diesem Identifier registriert ist oder das Tile bereits woanders registriert wurde.</exception>
    public void Register(uint identifier, T tile)
    {
        if (identifier >= _tiles.Length) throw new ArgumentOutOfRangeException(nameof(identifier));
        if (_tiles[identifier] != null) throw new InvalidOperationException($"Identifier {identifier} already registered.");
        if (Contains(tile)) throw new InvalidOperationException("Tile already registered.");
        _tiles[identifier] = tile;
    }

    /// <summary>
    /// Entfernt das Tile-Element am angegebenen Identifier.
    /// </summary>
    /// <param name="identifier">Der Index, dessen Element entfernt werden soll.</param>
    /// <exception cref="ArgumentOutOfRangeException">Wenn der Identifier außerhalb des gültigen Bereichs liegt.</exception>
    public void Unregister(uint identifier)
    {
        if (identifier >= _tiles.Length) throw new ArgumentOutOfRangeException(nameof(identifier));
        _tiles[identifier] = null;
    }

    /// <summary>
    /// Versucht, das Tile-Element für einen bestimmten Identifier abzurufen.
    /// </summary>
    /// <param name="identifier">Der Index, dessen Tile gesucht wird.</param>
    /// <param name="tile">Gibt das gefundene Tile zurück oder null, falls keines vorhanden ist.</param>
    /// <returns>True, wenn ein Tile an diesem Identifier existiert, sonst false.</returns>
    public bool TryGet(uint identifier, out T? tile)
    {
        if (identifier >= _tiles.Length)
        {
            tile = null;
            return false;
        }
        tile = _tiles[identifier];
        return tile != null;
    }

    /// <summary>
    /// Prüft, ob ein Tile-Element bereits in der Registry registriert ist (Vergleich per Referenz).
    /// </summary>
    /// <param name="tile">Das Tile-Element, das überprüft werden soll.</param>
    /// <returns>True, wenn das Tile bereits registriert ist, sonst false.</returns>
    public bool Contains(T tile)
    {
        foreach (var t in _tiles)
        {
            if (ReferenceEquals(t, tile)) return true;
        }
        return false;
    }

    /// <summary>
    /// Liefert eine direkte Referenz auf das Tile-Element am angegebenen Identifier.
    /// <para/>
    /// Vorsicht: Diese Methode führt keinen Bounds-Check durch und kann zu undefiniertem Verhalten führen,
    /// wenn ein ungültiger Identifier übergeben wird.
    /// <para/>
    /// Änderungen an der zurückgegebenen Referenz wirken sich direkt auf das interne Array aus.
    /// </summary>
    /// <param name="identifier">Der Index des Elements.</param>
    /// <returns>Referenz auf das Tile-Element oder null, wenn dort kein Element registriert ist.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T? GetElementByRef(uint identifier)
    {
        return ref _tiles[identifier];
    }
}
