using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a template capable of creating strongly typed object instances
    /// using either a factory delegate or an <see cref="IFactoryRegistry"/>.
    /// </summary>
    /// <typeparam name="T">Type of object created by the template.</typeparam>
    public class Template<T> : ITemplate
        where T : class
    {
        private readonly TemplateType _templateType;
        private readonly Func<T>? _factory;
        private readonly IFactoryRegistry? _referenceRegistry;
        private readonly string? _targetId;
        private readonly Type? _targetType;

        private enum TemplateType
        {
            Factory,
            Registry
        }

        /// <summary>
        /// Initializes a new template using the specified factory delegate.
        /// </summary>
        /// <param name="factory">Factory used to create object instances.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="factory"/> is null.
        /// </exception>
        public Template(Func<T> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);

            _templateType = TemplateType.Factory;
            _factory = factory;
        }

        /// <summary>
        /// Initializes a new template using a registered factory.
        /// </summary>
        /// <param name="registry">Registry containing the target factory.</param>
        /// <param name="targetId">Identifier of the registered factory.</param>
        /// <param name="targetType">Type associated with the registered factory.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="registry"/>,
        /// <paramref name="targetId"/>, or <paramref name="targetType"/> is null.
        /// </exception>
        public Template(
            IFactoryRegistry registry,
            string targetId,
            Type targetType)
        {
            ArgumentNullException.ThrowIfNull(registry);
            ArgumentNullException.ThrowIfNull(targetId);
            ArgumentNullException.ThrowIfNull(targetType);

            _templateType = TemplateType.Registry;
            _referenceRegistry = registry;
            _targetId = targetId;
            _targetType = targetType;
        }

        /// <summary>
        /// Creates a new object instance using the configured creation strategy.
        /// </summary>
        /// <returns>The newly created object instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the configured factory cannot create a valid
        /// <typeparamref name="T"/> instance.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown when the configured creation strategy is not supported.
        /// </exception>
        public virtual T Create()
        {
            return _templateType switch
            {
                TemplateType.Factory => CreateFromFactory(),
                TemplateType.Registry => CreateFromRegistry(),
                _ => throw new NotSupportedException(
                    $"Creation mode '{_templateType}' is not supported.")
            };
        }

        object ITemplate.Create()
        {
            return Create();
        }

        private T CreateFromFactory()
        {
            return _factory!()
                ?? throw new InvalidOperationException(
                    $"The template factory returned null for type '{typeof(T).Name}'.");
        }

        private T CreateFromRegistry()
        {
            var result = _referenceRegistry!.Create(_targetId!, _targetType!);

            if (result == null)
            {
                throw new InvalidOperationException(
                    $"The factory registry returned null for target '{_targetId}'.");
            }

            if (result is not T typedResult)
            {
                throw new InvalidOperationException(
                    $"The factory registry returned '{result.GetType().Name}' " +
                    $"instead of '{typeof(T).Name}' for target '{_targetId}'.");
            }

            return typedResult;
        }
    }
}