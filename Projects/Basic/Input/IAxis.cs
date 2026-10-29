using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Input;

/// <summary>
/// Provides non-generic access to two-dimensional values stored by axis identifier.
/// </summary>
/// <remarks>
/// Identifiers are the numeric values of the owning axis enum. Their meaning is scoped
/// to that axis collection; different collections may use the same numeric identifiers.
/// </remarks>
public interface IAxis
{
    /// <summary>
    /// Sets an axis value. Identifiers outside the collection's storage range are ignored.
    /// </summary>
    /// <param name="axis">The numeric axis identifier.</param>
    /// <param name="value">The two-dimensional input value, in application-defined units.</param>
    void Set(ulong axis, Vector2 value);

    /// <summary>
    /// Gets the stored value of an axis.
    /// </summary>
    /// <param name="axis">The numeric axis identifier.</param>
    /// <returns>The stored value, or zero if the identifier is outside the storage range.</returns>
    Vector2 Get(ulong axis);

    /// <summary>
    /// Invokes a callback for each non-zero axis in ascending identifier order.
    /// </summary>
    /// <param name="action">The callback receiving the numeric identifier and stored value.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    void ForEachNonZero(Action<ulong, Vector2> action);

    /// <summary>
    /// Resets all stored axis values to zero.
    /// </summary>
    void Clear();
}
