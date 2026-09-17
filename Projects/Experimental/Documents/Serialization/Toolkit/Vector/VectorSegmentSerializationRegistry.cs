using Sachssoft.Sasogine.Experimental.Components.Tools.Vector;
using Sachssoft.Sasogine.Extensions.Sasodoc;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Documents.Serialization.Toolkit;

/// <summary>
/// Provides a registry for serialization handlers associated exclusively with
/// vector segment definition types.
/// </summary>
/// <remarks>
/// The registry maintains mappings by both definition name and definition type.
/// This allows a serialization handler, definition type, and definition factory
/// to be resolved from a serialized name during reading, while writing can resolve
/// the corresponding name and serialization handler from the definition type.
/// </remarks>
public sealed class VectorSegmentSerializationRegistry
{
    private readonly Dictionary<string, Entry> _byName = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, Entry> _byType = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorSegmentSerializationRegistry"/> class
    /// and registers the built-in vector segment serialization handlers.
    /// </summary>
    public VectorSegmentSerializationRegistry()
    {
        Register(
            "Line",
            new VectorLineSegmentSerialization());

        Register(
            "Arc",
            new VectorCircularArcSegmentSerialization());

        Register(
            "QuadraticBezier",
            new VectorQuadraticBezierSegmentSerialization());

        Register(
            "CubicBezier",
            new VectorCubicBezierSegmentSerialization());

        Register(
            "CatmullRom",
            new VectorCatmullRomSegmentSerialization());

        Register(
            "BSpline",
            new VectorBSplineSegmentSerialization());
    }

    /// <summary>
    /// Registers a serialization handler for the specified vector segment definition type.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The vector segment definition type associated with the serialization handler.
    /// </typeparam>
    /// <param name="name">
    /// The name used to identify the vector segment definition during serialization
    /// and deserialization.
    /// </param>
    /// <param name="serialization">
    /// The serialization handler responsible for reading and writing the vector segment definition.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is empty or consists only of white-space characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/> or <paramref name="serialization"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void Register<TDefinition>(
        string name,
        SerializationBase<TDefinition> serialization)
        where TDefinition : VectorSegmentDefinition, new()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(serialization);

        Type definitionType = typeof(TDefinition);

        var entry = new Entry(
            name,
            definitionType,
            serialization,
            static () => new TDefinition());

        _byName[name] = entry;
        _byType[definitionType] = entry;
    }

    /// <summary>
    /// Gets the registered vector segment serialization entry associated with the specified name.
    /// </summary>
    /// <param name="name">
    /// The registered name of the vector segment definition.
    /// </param>
    /// <returns>
    /// The serialization entry associated with <paramref name="name"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is empty or consists only of white-space characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no vector segment serialization is registered for the specified name.
    /// </exception>
    public Entry Get(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (_byName.TryGetValue(name, out Entry? entry))
            return entry;

        throw new KeyNotFoundException(
            $"No vector segment serialization is registered for '{name}'.");
    }

    /// <summary>
    /// Gets the registered vector segment serialization entry associated with the specified
    /// definition type.
    /// </summary>
    /// <param name="definitionType">
    /// The vector segment definition type to resolve.
    /// </param>
    /// <returns>
    /// The serialization entry associated with <paramref name="definitionType"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="definitionType"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no vector segment serialization is registered for the specified definition type.
    /// </exception>
    public Entry Get(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        if (_byType.TryGetValue(definitionType, out Entry? entry))
            return entry;

        throw new KeyNotFoundException(
            $"No vector segment serialization is registered for '{definitionType.FullName}'.");
    }

    /// <summary>
    /// Gets the registered vector segment serialization entry associated with the specified
    /// definition type.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The vector segment definition type to resolve.
    /// </typeparam>
    /// <returns>
    /// The serialization entry associated with <typeparamref name="TDefinition"/>.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no vector segment serialization is registered for the specified definition type.
    /// </exception>
    public Entry Get<TDefinition>()
        where TDefinition : VectorSegmentDefinition
    {
        return Get(typeof(TDefinition));
    }

    /// <summary>
    /// Determines whether a vector segment serialization is registered for the specified name.
    /// </summary>
    /// <param name="name">
    /// The registered name of the vector segment definition.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an entry is registered for <paramref name="name"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> is empty or consists only of white-space characters.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    public bool Contains(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return _byName.ContainsKey(name);
    }

    /// <summary>
    /// Determines whether a vector segment serialization is registered for the specified
    /// definition type.
    /// </summary>
    /// <param name="definitionType">
    /// The vector segment definition type to check.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an entry is registered for <paramref name="definitionType"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="definitionType"/> is <see langword="null"/>.
    /// </exception>
    public bool Contains(Type definitionType)
    {
        ArgumentNullException.ThrowIfNull(definitionType);

        return _byType.ContainsKey(definitionType);
    }

    /// <summary>
    /// Determines whether a vector segment serialization is registered for the specified
    /// definition type.
    /// </summary>
    /// <typeparam name="TDefinition">
    /// The vector segment definition type to check.
    /// </typeparam>
    /// <returns>
    /// <see langword="true"/> if an entry is registered for <typeparamref name="TDefinition"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains<TDefinition>()
        where TDefinition : VectorSegmentDefinition
    {
        return Contains(typeof(TDefinition));
    }

    /// <summary>
    /// Describes a registered vector segment serialization and its associated definition identity.
    /// </summary>
    public sealed class Entry
    {
        private readonly Func<VectorSegmentDefinition> _factory;

        internal Entry(
            string name,
            Type definitionType,
            ISerialization serialization,
            Func<VectorSegmentDefinition> factory)
        {
            Name = name;
            DefinitionType = definitionType;
            Serialization = serialization;
            _factory = factory;
        }

        /// <summary>
        /// Gets the name used to identify the vector segment definition during serialization
        /// and deserialization.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the vector segment definition type associated with this entry.
        /// </summary>
        public Type DefinitionType { get; }

        /// <summary>
        /// Gets the serialization handler associated with the vector segment definition.
        /// </summary>
        public ISerialization Serialization { get; }

        /// <summary>
        /// Creates a new vector segment definition instance associated with this entry.
        /// </summary>
        /// <returns>
        /// A newly created vector segment definition instance.
        /// </returns>
        public VectorSegmentDefinition CreateDefinition()
        {
            return _factory();
        }
    }
}