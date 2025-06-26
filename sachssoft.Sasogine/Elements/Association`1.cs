/*
 * © 2024 Tobias Sachs
 * Association
 * 17.07.2024
 * Update 26.05.2025
 */

using System;
using System.Collections;

namespace sachssoft.Sasogine.Elements;

/// <summary>
/// Repräsentiert eine ID-basierte Assoziation zu einem <see cref="GameObject"/> eines bestimmten Typs.
/// </summary>
/// <typeparam name="T">Der Zieltyp, abgeleitet von <see cref="GameObject"/>.</typeparam>
public struct Association<T> : IAssociation where T : GameObject
{
    private string? _id;

    /// <summary>
    /// Gibt eine leere Assoziation ohne ID zurück.
    /// </summary>
    public static readonly Association<T> Empty = new Association<T>();

    /// <summary>
    /// Erstellt eine neue, leere Assoziation.
    /// </summary>
    public Association()
        : this(null)
    {
    }

    /// <summary>
    /// Erstellt eine neue Assoziation mit der angegebenen ID.
    /// </summary>
    public Association(string? id)
    {
        _id = id;
    }

    /// <summary>
    /// Die ID des referenzierten Objekts.
    /// </summary>
    public string? ID
    {
        get => _id;
        set => _id = value;
    }

    /// <summary>
    /// Gibt den Typ des referenzierten Objekts zurück.
    /// </summary>
    public Type Type => typeof(T);

    /// <summary>
    /// Gibt an, ob die Assoziation gültig ist (d.h. eine nicht-leere ID enthält).
    /// </summary>
    public bool IsValid => !string.IsNullOrWhiteSpace(_id);

    /// <summary>
    /// Prüft, ob in der übergebenen Sammlung mehr als ein Objekt mit der referenzierten ID vorhanden ist.
    /// </summary>
    /// <param name="collection">Eine Sammlung von Objekten.</param>
    public bool ContainsAmbiguous(IEnumerable collection)
    {
        int count = 0;

        foreach (var item in collection)
        {
            if (item is GameObject obj && obj.ID == _id)
            {
                count++;
                if (count > 1)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sucht das zugehörige Objekt in einer Sammlung anhand der ID.
    /// </summary>
    /// <param name="collection">Eine Sammlung von Objekten.</param>
    /// <returns>Das gefundene Objekt vom Typ <typeparamref name="T"/> oder <c>null</c>, wenn nicht gefunden.</returns>
    public T? Find(IEnumerable collection)
    {
        foreach (var item in collection)
        {
            if (item is T obj && obj.ID == _id)
            {
                return obj;
            }
        }

        return null;
    }

    /// <inheritdoc />
    object? IAssociation.Find(IEnumerable collection) => Find(collection);

    /// <inheritdoc />
    public override string ToString() => $"{typeof(T).Name}:{_id}";

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is Association<T> other && string.Equals(_id, other._id, StringComparison.Ordinal);

    /// <inheritdoc />
    public override int GetHashCode() => _id?.GetHashCode() ?? 0;

    /// <summary>
    /// Vergleicht zwei Assoziationen auf Gleichheit.
    /// </summary>
    public static bool operator ==(Association<T> left, Association<T> right) => left.Equals(right);

    /// <summary>
    /// Vergleicht zwei Assoziationen auf Ungleichheit.
    /// </summary>
    public static bool operator !=(Association<T> left, Association<T> right) => !left.Equals(right);
}
