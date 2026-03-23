using Sachssoft.Sasogine.Components;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Generic registry for definitions (Component, Asset, etc.) per target type.
    /// </summary>
    /// <typeparam name="TTarget">Runtime object type (Component, Asset, etc.)</typeparam>
    /// <typeparam name="TDefinition">Definition interface type (must be interface)</typeparam>
    public abstract class DefinitionRegistry<TTarget, TDefinition>
        where TDefinition : class, IElementDefinition
        where TTarget : class
    {
        // Key to uniquely identify Target+Definition type
        private readonly record struct RegistryKey(Type TargetType, Type DefinitionType);

        // Factories for each combination
        private readonly Dictionary<RegistryKey, Func<IElementDefinition>> _factories = new();

        // Static constructor validates type parameters
        static DefinitionRegistry()
        {
            // TDefinition must be interface
            if (!typeof(TDefinition).IsInterface)
                throw new InvalidOperationException($"{typeof(TDefinition).Name} must be an interface.");

            // Optional: TTarget must be class (already enforced by generic constraint)
            if (!typeof(TTarget).IsClass)
                throw new InvalidOperationException($"{typeof(TTarget).Name} must be a class.");
        }

        /// <summary>
        /// Registers a factory for a definition.
        /// </summary>
        /// <param name="definitionFactory">Factory function to create the concrete definition</param>
        public void Register(Func<TDefinition> definitionFactory)
        {
            if (definitionFactory == null)
                throw new ArgumentNullException(nameof(definitionFactory));

            var key = new RegistryKey(typeof(TTarget), typeof(TDefinition));

            if (_factories.ContainsKey(key))
                throw new InvalidOperationException($"Factory for target {typeof(TTarget).Name} and definition {typeof(TDefinition).Name} is already registered.");

            _factories[key] = () => definitionFactory();
        }

        /// <summary>
        /// Creates a new definition instance for the given target type.
        /// </summary>
        /// <param name="targetType">Target class type</param>
        /// <param name="definitionType">Definition interface type</param>
        /// <returns>New instance implementing TDefinition</returns>
        public TDefinition Create(Type targetType, Type definitionType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (definitionType == null)
                throw new ArgumentNullException(nameof(definitionType));

            var key = new RegistryKey(targetType, definitionType);

            if (!_factories.TryGetValue(key, out var factory))
                throw new InvalidOperationException($"No factory registered for target {targetType.Name} and definition {definitionType.Name}.");

            var instance = factory();

            if (instance == null)
                throw new InvalidOperationException($"Factory for {definitionType.Name} returned null.");

            return (TDefinition)instance;
        }
    }
}