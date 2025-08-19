/*
 * © 2024 Tobias Sachs
 * ITypeSelector
 * 11.07.2024
 * Update 26.05.2025
 */

using System;

namespace Sachssoft.Sasogine.Elements;

/// <summary>
/// Represents a type-based constraint between a parent type and a child type.
/// </summary>
public interface ITypeSelector
{
    /// <summary>
    /// Checks whether the specified type is a concrete generic <see cref="TypeSelector{T}"/>.
    /// </summary>
    public static bool IsTypeSelectorType(Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(TypeSelector<>) &&
               typeof(ITypeSelector).IsAssignableFrom(type);
    }

    /// <summary>
    /// Creates a new instance of <see cref="TypeSelector{T}"/> for the given parent and child types.
    /// </summary>
    /// <remarks>
    /// This method uses reflection and is not compatible with AOT environments.
    /// Use <c>Create&lt;T&gt;(Func&lt;TypeSelector&lt;T&gt;&gt;)</c> instead for AOT-safe usage.
    /// </remarks>
    [Obsolete("This method uses reflection and is not AOT-compatible. Use Create<TTile>(Func<TypeSelector<TTile>>) instead.", DiagnosticId = "SAS0001")]
    public static ITypeSelector Create(Type parentType, Type childType)
    {
        if (parentType == null)
            throw new ArgumentNullException(nameof(parentType));
        if (childType == null)
            throw new ArgumentNullException(nameof(childType));

        var genericType = typeof(TypeSelector<>).MakeGenericType(parentType);

        if (!typeof(ITypeSelector).IsAssignableFrom(genericType))
            throw new ArgumentException($"The generated type does not implement ITypeSelector: {genericType}");

        return (ITypeSelector)Activator.CreateInstance(genericType, new object[] { childType })!;
    }

    /// <summary>
    /// AOT-safe factory method for creating a <see cref="TypeSelector{T}"/>.
    /// </summary>
    /// <typeparam name="T">The generic parent type.</typeparam>
    /// <param name="factory">A factory function that returns the instance.</param>
    /// <returns>An instance of <see cref="TypeSelector{T}"/> as <see cref="ITypeSelector"/>.</returns>
    public static ITypeSelector Create<T>(Func<TypeSelector<T>> factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        return factory();
    }

    /// <summary>
    /// Gets the parent type (e.g., a base class).
    /// </summary>
    Type ParentType { get; }

    /// <summary>
    /// Gets the actual allowed child type.
    /// </summary>
    Type ChildType { get; }
}
