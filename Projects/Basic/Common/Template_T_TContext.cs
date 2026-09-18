using System;

namespace Sachssoft.Sasogine.Common
{
    /// <summary>
    /// Represents a template capable of creating strongly typed object instances
    /// using a context-aware factory delegate.
    /// </summary>
    /// <typeparam name="T">Type of object created by the template.</typeparam>
    /// <typeparam name="TContext">Type of context supplied during object creation.</typeparam>
    public class Template<T, TContext> : ITemplate
        where T : class
    {
        private readonly Func<TContext, T> _factory;

        /// <summary>
        /// Initializes a new template using the specified context-aware factory delegate.
        /// </summary>
        /// <param name="factory">
        /// Factory used to create object instances using the supplied context.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="factory"/> is null.
        /// </exception>
        public Template(Func<TContext, T> factory)
        {
            ArgumentNullException.ThrowIfNull(factory);

            _factory = factory;
        }

        /// <summary>
        /// Creates a new object instance using the specified context.
        /// </summary>
        /// <param name="context">Context supplied to the factory.</param>
        /// <returns>The newly created object instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the configured factory returns null.
        /// </exception>
        public virtual T Create(TContext context)
        {
            return _factory(context)
                ?? throw new InvalidOperationException(
                    $"The template factory returned null for type '{typeof(T).Name}'.");
        }

        object ITemplate.Create(object? context)
        {
            if (context is not TContext typedContext)
            {
                throw new ArgumentException(
                    $"The context must be of type '{typeof(TContext).Name}'.",
                    nameof(context));
            }

            return Create(typedContext);
        }
    }
}