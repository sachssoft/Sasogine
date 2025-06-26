/*
 * © 2024 Tobias Sachs
 * TypeSelector
 * 11.07.2024
 * Update 26.05.2025
 */

using System;

namespace sachssoft.Sasogine.Elements;

/// <summary>
/// Repräsentiert eine Typpaarung zwischen einem Eltern- und einem Kindtyp,
/// wobei geprüft wird, ob der Kindtyp zu T kompatibel ist.
/// </summary>
public class TypeSelector<T> : ITypeSelector
{
    private readonly Type _type;
    private readonly Type _child_type;

    /// <summary>
    /// Initialisiert eine neue Instanz des <see cref="TypeSelector{T}"/> mit einem gegebenen Kindtyp.
    /// </summary>
    /// <param name="childType">Der zu prüfende abgeleitete Typ.</param>
    /// <exception cref="ArgumentNullException">Wenn <paramref name="childType"/> null ist.</exception>
    /// <exception cref="ArgumentException">Wenn der Typ nicht kompatibel ist.</exception>
    public TypeSelector(Type childType)
    {
        _type = typeof(T);

        _child_type = childType ?? throw new ArgumentNullException(nameof(childType));

        if (!AllowType(_child_type))
            throw new ArgumentException(
                $"Der Typ {_child_type.FullName} ist kein gültiger Nachfolger von {_type.FullName}.",
                nameof(childType));
    }

    /// <summary>
    /// Gibt den übergeordneten Basistyp zurück.
    /// </summary>
    public Type ParentType => _type;

    /// <summary>
    /// Gibt den erlaubten abgeleiteten Kindtyp zurück.
    /// </summary>
    public Type ChildType => _child_type;

    /// <summary>
    /// Prüft, ob ein angegebener Typ gültig ist (identisch oder abgeleitet).
    /// </summary>
    /// <param name="candidateType">Der zu prüfende Typ.</param>
    /// <returns><c>true</c>, wenn der Typ erlaubt ist; andernfalls <c>false</c>.</returns>
    protected virtual bool AllowType(Type candidateType)
    {
        // Gültig, wenn gleich oder zuweisbar zu Basistyp T
        return candidateType == _type || candidateType.IsAssignableTo(_type);
    }
}
