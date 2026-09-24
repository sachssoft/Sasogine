using Sachssoft.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Sachssoft.Engine.Components.Definitions;

/// <summary>
/// Provides access to properties and property metadata for definition types.
/// </summary>
public static class DefinitionProperties
{
    private static readonly ConcurrentDictionary<Type, ReflectionCacheEntry> _reflectionCache = new();

    /// <summary>
    /// Gets the property with the specified name using metadata provided by the definition type.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property associated with the specified name.</returns>
    public static IDefinitionProperty Get<TDefinition>(string name)
        where TDefinition : IDefinitionMetadata
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return TDefinition.GetProperty(name);
    }

    /// <summary>
    /// Gets all properties using metadata provided by the definition type.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <returns>A read-only list containing the properties.</returns>
    public static IReadOnlyList<IDefinitionProperty> GetAll<TDefinition>()
        where TDefinition : IDefinitionMetadata
    {
        return TDefinition.GetProperties();
    }

    /// <summary>
    /// Gets the attributes associated with the specified property.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>A read-only list containing the property attributes.</returns>
    public static IReadOnlyList<Attribute> GetAttributes<TDefinition>(string name)
        where TDefinition : IDefinitionMetadata
    {
        return Get<TDefinition>(name).Attributes;
    }

    /// <summary>
    /// Gets the first attribute of the specified type associated with the specified property.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <typeparam name="TAttribute">The attribute type.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>
    /// The first matching attribute, or <see langword="null"/> if no matching attribute exists.
    /// </returns>
    public static TAttribute? GetAttribute<TDefinition, TAttribute>(string name)
        where TDefinition : IDefinitionMetadata
        where TAttribute : Attribute
    {
        return Get<TDefinition>(name).GetAttribute<TAttribute>();
    }

    /// <summary>
    /// Determines whether an attribute of the specified type is associated with the specified property.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <typeparam name="TAttribute">The attribute type.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>
    /// <see langword="true"/> if a matching attribute exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool HasAttribute<TDefinition, TAttribute>(string name)
        where TDefinition : IDefinitionMetadata
        where TAttribute : Attribute
    {
        return Get<TDefinition>(name).HasAttribute<TAttribute>();
    }

    /// <summary>
    /// Gets the property with the specified name using reflection.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property associated with the specified name.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Reflection-based functionality is not supported by the current runtime or platform.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// No property with the specified <paramref name="name"/> exists.
    /// </exception>
    public static IDefinitionProperty GetFromReflection<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDefinition>(string name)
        where TDefinition : IDefinition
    {
        RuntimeCapabilities.EnsureReflectionSupported();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entry = GetReflectionCacheEntry(typeof(TDefinition));

        if (entry.PropertiesByName.TryGetValue(name, out var property))
            return property;

        throw new KeyNotFoundException(
            $"Property '{name}' was not found on definition type '{typeof(TDefinition).FullName}'.");
    }

    /// <summary>
    /// Gets all properties of the specified definition type using reflection.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <returns>A read-only list containing the properties.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Reflection-based functionality is not supported by the current runtime or platform.
    /// </exception>
    public static IReadOnlyList<IDefinitionProperty> GetAllFromReflection<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDefinition>()
        where TDefinition : IDefinition
    {
        RuntimeCapabilities.EnsureReflectionSupported();

        return GetReflectionCacheEntry(typeof(TDefinition)).Properties;
    }

    /// <summary>
    /// Gets the attributes associated with the specified property using reflection.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>A read-only list containing the property attributes.</returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Reflection-based functionality is not supported by the current runtime or platform.
    /// </exception>
    public static IReadOnlyList<Attribute> GetAttributesFromReflection<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDefinition>(string name)
        where TDefinition : IDefinition
    {
        RuntimeCapabilities.EnsureReflectionSupported();

        return GetFromReflection<TDefinition>(name).Attributes;
    }

    /// <summary>
    /// Gets the first attribute of the specified type associated with the specified property using reflection.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <typeparam name="TAttribute">The attribute type.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>
    /// The first matching attribute, or <see langword="null"/> if no matching attribute exists.
    /// </returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Reflection-based functionality is not supported by the current runtime or platform.
    /// </exception>
    public static TAttribute? GetAttributeFromReflection<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDefinition,
        TAttribute>(string name)
        where TDefinition : IDefinition
        where TAttribute : Attribute
    {
        RuntimeCapabilities.EnsureReflectionSupported();

        return GetFromReflection<TDefinition>(name).GetAttribute<TAttribute>();
    }

    /// <summary>
    /// Determines whether an attribute of the specified type is associated with the specified property using reflection.
    /// </summary>
    /// <typeparam name="TDefinition">The definition type.</typeparam>
    /// <typeparam name="TAttribute">The attribute type.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>
    /// <see langword="true"/> if a matching attribute exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="PlatformNotSupportedException">
    /// Reflection-based functionality is not supported by the current runtime or platform.
    /// </exception>
    public static bool HasAttributeFromReflection<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDefinition,
        TAttribute>(string name)
        where TDefinition : IDefinition
        where TAttribute : Attribute
    {
        RuntimeCapabilities.EnsureReflectionSupported();

        return GetFromReflection<TDefinition>(name).HasAttribute<TAttribute>();
    }

    private static ReflectionCacheEntry GetReflectionCacheEntry(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type definitionType)
    {
        RuntimeCapabilities.EnsureReflectionSupported();

        if (_reflectionCache.TryGetValue(definitionType, out var entry))
            return entry;

        entry = CreateReflectionCacheEntry(definitionType);

        return _reflectionCache.GetOrAdd(definitionType, entry);
    }

    private static ReflectionCacheEntry CreateReflectionCacheEntry(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type definitionType)
    {
        RuntimeCapabilities.EnsureReflectionSupported();

        var propertyInfos = definitionType
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetIndexParameters().Length == 0)
            .ToArray();

        var properties = new IDefinitionProperty[propertyInfos.Length];
        var propertiesByName = new Dictionary<string, IDefinitionProperty>(
            propertyInfos.Length,
            StringComparer.Ordinal);

        for (int i = 0; i < propertyInfos.Length; i++)
        {
            var property = new ReflectionDefinitionProperty(propertyInfos[i]);

            properties[i] = property;
            propertiesByName.Add(property.Name, property);
        }

        return new ReflectionCacheEntry(properties, propertiesByName);
    }

    private sealed class ReflectionCacheEntry
    {
        public ReflectionCacheEntry(
            IReadOnlyList<IDefinitionProperty> properties,
            IReadOnlyDictionary<string, IDefinitionProperty> propertiesByName)
        {
            Properties = properties;
            PropertiesByName = propertiesByName;
        }

        public IReadOnlyList<IDefinitionProperty> Properties { get; }

        public IReadOnlyDictionary<string, IDefinitionProperty> PropertiesByName { get; }
    }

    private sealed class ReflectionDefinitionProperty : IDefinitionProperty
    {
        private readonly PropertyInfo _property;
        private readonly IReadOnlyList<Attribute> _attributes;

        public ReflectionDefinitionProperty(PropertyInfo property)
        {
            ArgumentNullException.ThrowIfNull(property);

            RuntimeCapabilities.EnsureReflectionSupported();

            _property = property;
            _attributes = property.GetCustomAttributes<Attribute>(true).ToArray();
        }

        public event EventHandler<DefinitionPropertyChangedEventArgs>? ValueChanged;

        public string Name => _property.Name;

        public Type Type => _property.PropertyType;

        public bool IsReadOnly => _property.SetMethod == null;

        public IReadOnlyList<Attribute> Attributes => _attributes;

        public TAttribute? GetAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            foreach (var attribute in _attributes)
            {
                if (attribute is TAttribute result)
                    return result;
            }

            return null;
        }

        public bool HasAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            foreach (var attribute in _attributes)
            {
                if (attribute is TAttribute)
                    return true;
            }

            return false;
        }

        public object? GetValue(IDefinition source)
        {
            RuntimeCapabilities.EnsureReflectionSupported();
            ArgumentNullException.ThrowIfNull(source);

            ValidateSource(source);

            return _property.GetValue(source);
        }

        public bool CanSetValue(IDefinition source, object? value)
        {
            RuntimeCapabilities.EnsureReflectionSupported();
            ArgumentNullException.ThrowIfNull(source);

            if (IsReadOnly || !IsValidSource(source))
                return false;

            if (value == null)
                return !Type.IsValueType || Nullable.GetUnderlyingType(Type) != null;

            return Type.IsInstanceOfType(value);
        }

        public void SetValue(IDefinition source, object? value)
        {
            RuntimeCapabilities.EnsureReflectionSupported();
            ArgumentNullException.ThrowIfNull(source);

            ValidateSource(source);

            if (IsReadOnly)
                throw new InvalidOperationException(
                    $"Property '{Name}' is read-only.");

            if (!CanSetValue(source, value))
                throw new ArgumentException(
                    $"Value must be of type '{Type.FullName}'.",
                    nameof(value));

            var oldValue = _property.GetValue(source);

            if (Equals(oldValue, value))
                return;

            _property.SetValue(source, value);

            var newValue = _property.GetValue(source);

            if (!Equals(oldValue, newValue))
                OnValueChanged(source, oldValue, newValue);
        }

        private bool IsValidSource(IDefinition source)
        {
            return _property.DeclaringType?.IsInstanceOfType(source) == true;
        }

        private void ValidateSource(IDefinition source)
        {
            if (!IsValidSource(source))
                throw new ArgumentException(
                    $"Definition must be compatible with type '{_property.DeclaringType?.FullName}'.",
                    nameof(source));
        }

        private void OnValueChanged(
            IDefinition source,
            object? oldValue,
            object? newValue)
        {
            ValueChanged?.Invoke(
                this,
                new DefinitionPropertyChangedEventArgs(
                    source,
                    oldValue,
                    newValue));
        }
    }
}