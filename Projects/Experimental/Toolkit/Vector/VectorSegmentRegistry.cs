using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Registers vector segment definition types together with their runtime engine types.
    /// </summary>
    public sealed class VectorSegmentRegistry
    {
        private readonly Dictionary<Type, Entry> _entriesByDefinition = [];
        private readonly Dictionary<Type, Entry> _entriesByEngine = [];

        /// <summary>
        /// Gets the number of registered vector segment types.
        /// </summary>
        public int Count => _entriesByDefinition.Count;

        /// <summary>
        /// Creates a registry containing all built-in vector segment types.
        /// </summary>
        public static VectorSegmentRegistry CreateDefault()
        {
            var registry = new VectorSegmentRegistry();

            registry.Register<VectorLineSegmentDefinition, VectorLineSegment>(
                definition => new VectorLineSegment(definition));

            registry.Register<VectorQuadraticBezierSegmentDefinition, VectorQuadraticBezierSegment>(
                definition => new VectorQuadraticBezierSegment(definition));

            registry.Register<VectorCubicBezierSegmentDefinition, VectorCubicBezierSegment>(
                definition => new VectorCubicBezierSegment(definition));

            registry.Register<VectorCircularArcSegmentDefinition, VectorCircularArcSegment>(
                definition => new VectorCircularArcSegment(definition));

            registry.Register<VectorBSplineSegmentDefinition, VectorBSplineSegment>(
                definition => new VectorBSplineSegment(definition));

            registry.Register<VectorCatmullRomSegmentDefinition, VectorCatmullRomSegment>(
                definition => new VectorCatmullRomSegment(definition));

            return registry;
        }

        /// <summary>
        /// Registers a definition type and its corresponding runtime engine type.
        /// </summary>
        public void Register<TDefinition, TEngine>(
            Func<TDefinition, TEngine> engineFactory)
            where TDefinition : VectorSegmentDefinition, new()
            where TEngine : class, IVectorSegment
        {
            ArgumentNullException.ThrowIfNull(engineFactory);

            Type definitionType = typeof(TDefinition);
            Type engineType = typeof(TEngine);

            if (_entriesByDefinition.ContainsKey(definitionType))
            {
                throw new InvalidOperationException(
                    $"The definition type '{definitionType.FullName}' is already registered.");
            }

            if (_entriesByEngine.ContainsKey(engineType))
            {
                throw new InvalidOperationException(
                    $"The engine type '{engineType.FullName}' is already registered.");
            }

            var entry = new Entry(
                definitionType,
                engineType,
                static () => new TDefinition(),
                definition => engineFactory((TDefinition)definition));

            _entriesByDefinition.Add(definitionType, entry);
            _entriesByEngine.Add(engineType, entry);
        }

        /// <summary>
        /// Creates a new definition of the specified type.
        /// </summary>
        public TDefinition CreateDefinition<TDefinition>()
            where TDefinition : VectorSegmentDefinition
        {
            Type definitionType = typeof(TDefinition);

            if (!_entriesByDefinition.TryGetValue(definitionType, out Entry? entry))
            {
                throw new KeyNotFoundException(
                    $"The vector segment definition type '{definitionType.FullName}' is not registered.");
            }

            return (TDefinition)entry.CreateDefinition();
        }

        /// <summary>
        /// Creates a new definition of the specified type.
        /// </summary>
        public VectorSegmentDefinition CreateDefinition(Type definitionType)
        {
            ArgumentNullException.ThrowIfNull(definitionType);

            if (!_entriesByDefinition.TryGetValue(definitionType, out Entry? entry))
            {
                throw new KeyNotFoundException(
                    $"The vector segment definition type '{definitionType.FullName}' is not registered.");
            }

            return entry.CreateDefinition();
        }

        /// <summary>
        /// Creates the runtime engine segment corresponding to a definition.
        /// </summary>
        public IVectorSegment CreateEngine(VectorSegmentDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            Type definitionType = definition.GetType();

            if (!_entriesByDefinition.TryGetValue(definitionType, out Entry? entry))
            {
                throw new KeyNotFoundException(
                    $"The vector segment definition type '{definitionType.FullName}' is not registered.");
            }

            return entry.CreateEngine(definition);
        }

        /// <summary>
        /// Creates the runtime engine segment corresponding to a definition.
        /// </summary>
        public TEngine CreateEngine<TEngine>(VectorSegmentDefinition definition)
            where TEngine : class, IVectorSegment
        {
            ArgumentNullException.ThrowIfNull(definition);

            IVectorSegment engine = CreateEngine(definition);

            if (engine is not TEngine result)
            {
                throw new InvalidOperationException(
                    $"The definition type '{definition.GetType().FullName}' " +
                    $"does not create engine type '{typeof(TEngine).FullName}'.");
            }

            return result;
        }

        /// <summary>
        /// Gets the definition type associated with a runtime engine type.
        /// </summary>
        public Type GetDefinitionType(Type engineType)
        {
            ArgumentNullException.ThrowIfNull(engineType);

            if (!_entriesByEngine.TryGetValue(engineType, out Entry? entry))
            {
                throw new KeyNotFoundException(
                    $"The vector segment engine type '{engineType.FullName}' is not registered.");
            }

            return entry.DefinitionType;
        }

        /// <summary>
        /// Gets the runtime engine type associated with a definition type.
        /// </summary>
        public Type GetEngineType(Type definitionType)
        {
            ArgumentNullException.ThrowIfNull(definitionType);

            if (!_entriesByDefinition.TryGetValue(definitionType, out Entry? entry))
            {
                throw new KeyNotFoundException(
                    $"The vector segment definition type '{definitionType.FullName}' is not registered.");
            }

            return entry.EngineType;
        }

        public bool ContainsDefinition(Type definitionType)
        {
            ArgumentNullException.ThrowIfNull(definitionType);
            return _entriesByDefinition.ContainsKey(definitionType);
        }

        public bool ContainsEngine(Type engineType)
        {
            ArgumentNullException.ThrowIfNull(engineType);
            return _entriesByEngine.ContainsKey(engineType);
        }

        private sealed class Entry
        {
            public Entry(
                Type definitionType,
                Type engineType,
                Func<VectorSegmentDefinition> createDefinition,
                Func<VectorSegmentDefinition, IVectorSegment> createEngine)
            {
                DefinitionType = definitionType;
                EngineType = engineType;
                CreateDefinition = createDefinition;
                CreateEngine = createEngine;
            }

            public Type DefinitionType { get; }

            public Type EngineType { get; }

            public Func<VectorSegmentDefinition> CreateDefinition { get; }

            public Func<VectorSegmentDefinition, IVectorSegment> CreateEngine { get; }
        }
    }
}