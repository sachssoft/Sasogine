using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>
    /// Provides a registry for vector segment types and maps
    /// <see cref="VectorSegmentDefinition"/> types to their corresponding
    /// runtime <see cref="IVectorSegment"/> types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A registry defines which vector segment implementations are available
    /// within a vector shape hierarchy.
    /// </para>
    /// <para>
    /// Each registered entry provides a one-to-one mapping between a concrete
    /// definition type and a concrete runtime segment type. The registry can
    /// create new definitions, create runtime instances from existing
    /// definitions, and resolve the associated types in either direction.
    /// </para>
    /// </remarks>
    public sealed class VectorSegmentRegistry
    {
        private readonly Dictionary<Type, Entry> _entriesByDefinition = [];
        private readonly Dictionary<Type, Entry> _entriesByInstance = [];

        /// <summary>
        /// Initializes a new empty instance of the
        /// <see cref="VectorSegmentRegistry"/> class.
        /// </summary>
        public VectorSegmentRegistry()
        {
        }

        /// <summary>
        /// Gets the default vector segment registry containing all built-in
        /// vector segment types.
        /// </summary>
        /// <remarks>
        /// The returned registry is shared and should be used when no custom
        /// vector segment registry is required.
        /// </remarks>
        public static VectorSegmentRegistry Default { get; } = CreateDefault();

        /// <summary>
        /// Gets the number of registered vector segment types.
        /// </summary>
        public int Count => _entriesByDefinition.Count;

        /// <summary>
        /// Registers a vector segment definition type together with its
        /// corresponding runtime segment type.
        /// </summary>
        /// <typeparam name="TDefinition">
        /// The concrete vector segment definition type.
        /// </typeparam>
        /// <typeparam name="TInstance">
        /// The concrete runtime vector segment type.
        /// </typeparam>
        /// <param name="createInstance">
        /// The factory used to create a runtime segment instance from
        /// its definition.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="createInstance"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when either the definition type or runtime segment type
        /// is already registered.
        /// </exception>
        public void Register<TDefinition, TInstance>(
            Func<TDefinition, TInstance> createInstance)
            where TDefinition : VectorSegmentDefinition, new()
            where TInstance : class, IVectorSegment
        {
            ArgumentNullException.ThrowIfNull(createInstance);

            Type definitionType = typeof(TDefinition);
            Type instanceType = typeof(TInstance);

            if (_entriesByDefinition.ContainsKey(definitionType))
            {
                throw new InvalidOperationException(
                    $"The vector segment definition type " +
                    $"'{definitionType.FullName}' is already registered.");
            }

            if (_entriesByInstance.ContainsKey(instanceType))
            {
                throw new InvalidOperationException(
                    $"The vector segment runtime type " +
                    $"'{instanceType.FullName}' is already registered.");
            }

            var entry = new Entry(
                definitionType,
                instanceType,
                static () => new TDefinition(),
                definition => createInstance((TDefinition)definition));

            _entriesByDefinition.Add(definitionType, entry);
            _entriesByInstance.Add(instanceType, entry);
        }

        /// <summary>
        /// Creates a new vector segment definition of the specified
        /// registered type.
        /// </summary>
        /// <typeparam name="TDefinition">
        /// The vector segment definition type to create.
        /// </typeparam>
        /// <returns>
        /// A new instance of <typeparamref name="TDefinition"/>.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified definition type is not registered.
        /// </exception>
        public TDefinition CreateDefinition<TDefinition>()
            where TDefinition : VectorSegmentDefinition
        {
            return (TDefinition)CreateDefinition(typeof(TDefinition));
        }

        /// <summary>
        /// Creates a new vector segment definition of the specified
        /// registered type.
        /// </summary>
        /// <param name="definitionType">
        /// The concrete definition type to create.
        /// </param>
        /// <returns>
        /// A new vector segment definition of the specified type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="definitionType"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified definition type is not registered.
        /// </exception>
        public VectorSegmentDefinition CreateDefinition(Type definitionType)
        {
            ArgumentNullException.ThrowIfNull(definitionType);

            if (!_entriesByDefinition.TryGetValue(
                    definitionType,
                    out Entry? entry))
            {
                throw new KeyNotFoundException(
                    $"The vector segment definition type " +
                    $"'{definitionType.FullName}' is not registered.");
            }

            return entry.CreateDefinition();
        }

        /// <summary>
        /// Creates the runtime vector segment instance associated with
        /// the specified definition.
        /// </summary>
        /// <param name="definition">
        /// The vector segment definition from which the runtime instance
        /// is created.
        /// </param>
        /// <returns>
        /// The runtime vector segment instance associated with the concrete
        /// type of <paramref name="definition"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="definition"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the concrete definition type is not registered.
        /// </exception>
        public IVectorSegment CreateInstance(
            VectorSegmentDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            Type definitionType = definition.GetType();

            if (!_entriesByDefinition.TryGetValue(
                    definitionType,
                    out Entry? entry))
            {
                throw new KeyNotFoundException(
                    $"The vector segment definition type " +
                    $"'{definitionType.FullName}' is not registered.");
            }

            return entry.CreateInstance(definition);
        }

        /// <summary>
        /// Creates the runtime vector segment instance associated with
        /// the specified definition and returns it as the requested type.
        /// </summary>
        /// <typeparam name="TInstance">
        /// The expected runtime vector segment type.
        /// </typeparam>
        /// <param name="definition">
        /// The vector segment definition from which the runtime instance
        /// is created.
        /// </param>
        /// <returns>
        /// The created runtime vector segment instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="definition"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the concrete definition type is not registered.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the registered runtime type is not assignable to
        /// <typeparamref name="TInstance"/>.
        /// </exception>
        public TInstance CreateInstance<TInstance>(
            VectorSegmentDefinition definition)
            where TInstance : class, IVectorSegment
        {
            ArgumentNullException.ThrowIfNull(definition);

            IVectorSegment instance = CreateInstance(definition);

            if (instance is not TInstance result)
            {
                throw new InvalidOperationException(
                    $"The vector segment definition type " +
                    $"'{definition.GetType().FullName}' does not create " +
                    $"runtime type '{typeof(TInstance).FullName}'.");
            }

            return result;
        }

        /// <summary>
        /// Gets the vector segment definition type associated with the
        /// specified runtime segment type.
        /// </summary>
        /// <param name="instanceType">
        /// The registered runtime segment type.
        /// </param>
        /// <returns>
        /// The definition type associated with
        /// <paramref name="instanceType"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="instanceType"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the runtime segment type is not registered.
        /// </exception>
        public Type GetDefinitionType(Type instanceType)
        {
            ArgumentNullException.ThrowIfNull(instanceType);

            if (!_entriesByInstance.TryGetValue(
                    instanceType,
                    out Entry? entry))
            {
                throw new KeyNotFoundException(
                    $"The vector segment runtime type " +
                    $"'{instanceType.FullName}' is not registered.");
            }

            return entry.DefinitionType;
        }

        /// <summary>
        /// Gets the runtime vector segment type associated with the
        /// specified definition type.
        /// </summary>
        /// <param name="definitionType">
        /// The registered vector segment definition type.
        /// </param>
        /// <returns>
        /// The runtime segment type associated with
        /// <paramref name="definitionType"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="definitionType"/> is
        /// <see langword="null"/>.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the definition type is not registered.
        /// </exception>
        public Type GetInstanceType(Type definitionType)
        {
            ArgumentNullException.ThrowIfNull(definitionType);

            if (!_entriesByDefinition.TryGetValue(
                    definitionType,
                    out Entry? entry))
            {
                throw new KeyNotFoundException(
                    $"The vector segment definition type " +
                    $"'{definitionType.FullName}' is not registered.");
            }

            return entry.InstanceType;
        }

        /// <summary>
        /// Determines whether the specified vector segment definition type
        /// is registered.
        /// </summary>
        /// <param name="definitionType">
        /// The definition type to check.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the definition type is registered;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool ContainsDefinition(Type definitionType)
        {
            ArgumentNullException.ThrowIfNull(definitionType);

            return _entriesByDefinition.ContainsKey(definitionType);
        }

        /// <summary>
        /// Determines whether the specified runtime vector segment type
        /// is registered.
        /// </summary>
        /// <param name="instanceType">
        /// The runtime segment type to check.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the runtime type is registered;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool ContainsInstance(Type instanceType)
        {
            ArgumentNullException.ThrowIfNull(instanceType);

            return _entriesByInstance.ContainsKey(instanceType);
        }

        /// <summary>
        /// Creates the default registry containing all built-in vector
        /// segment types.
        /// </summary>
        private static VectorSegmentRegistry CreateDefault()
        {
            var registry = new VectorSegmentRegistry();

            registry.Register<
                VectorLineSegmentDefinition,
                VectorLineSegment>(
                definition => new VectorLineSegment(definition));

            registry.Register<
                VectorQuadraticBezierSegmentDefinition,
                VectorQuadraticBezierSegment>(
                definition => new VectorQuadraticBezierSegment(definition));

            registry.Register<
                VectorCubicBezierSegmentDefinition,
                VectorCubicBezierSegment>(
                definition => new VectorCubicBezierSegment(definition));

            registry.Register<
                VectorCircularArcSegmentDefinition,
                VectorCircularArcSegment>(
                definition => new VectorCircularArcSegment(definition));

            registry.Register<
                VectorBSplineSegmentDefinition,
                VectorBSplineSegment>(
                definition => new VectorBSplineSegment(definition));

            registry.Register<
                VectorCatmullRomSegmentDefinition,
                VectorCatmullRomSegment>(
                definition => new VectorCatmullRomSegment(definition));

            return registry;
        }

        /// <summary>
        /// Represents a registered mapping between a vector segment
        /// definition type and its corresponding runtime segment type.
        /// </summary>
        private sealed class Entry
        {
            private readonly Func<VectorSegmentDefinition> _createDefinition;
            private readonly Func<VectorSegmentDefinition, IVectorSegment>
                _createInstance;

            /// <summary>
            /// Initializes a new registry entry.
            /// </summary>
            public Entry(
                Type definitionType,
                Type instanceType,
                Func<VectorSegmentDefinition> createDefinition,
                Func<VectorSegmentDefinition, IVectorSegment> createInstance)
            {
                DefinitionType = definitionType;
                InstanceType = instanceType;
                _createDefinition = createDefinition;
                _createInstance = createInstance;
            }

            /// <summary>
            /// Gets the registered definition type.
            /// </summary>
            public Type DefinitionType { get; }

            /// <summary>
            /// Gets the registered runtime segment type.
            /// </summary>
            public Type InstanceType { get; }

            /// <summary>
            /// Creates a new definition instance.
            /// </summary>
            public VectorSegmentDefinition CreateDefinition()
            {
                return _createDefinition();
            }

            /// <summary>
            /// Creates a runtime segment instance from the specified
            /// definition.
            /// </summary>
            public IVectorSegment CreateInstance(
                VectorSegmentDefinition definition)
            {
                return _createInstance(definition);
            }
        }
    }
}