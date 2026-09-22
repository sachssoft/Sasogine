using System;

namespace Sachssoft.Engine.Common;

/// <summary>
/// Provides a fluent builder for creating a context-aware
/// <see cref="IDefinitionBindingFactory{TDefinition, TObject, TContext}"/>
/// from delegates that create, attach, and release engine object instances.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to create and bind engine objects.
/// </typeparam>
/// <typeparam name="TObject">
/// The type of engine object created and managed by the resulting factory.
/// </typeparam>
/// <typeparam name="TContext">
/// The type of context supplied to the factory operations.
/// </typeparam>
public sealed class DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
    where TContext : class
{
    private Func<TDefinition, TContext, TObject>? _create;
    private Action<TDefinition, TObject, TContext>? _attach;
    private Action<TDefinition, TObject, TContext>? _release;

    private DefinitionBindingFactoryBuilder()
    {
    }

    /// <summary>
    /// Creates a new builder for configuring a context-aware
    /// definition binding factory.
    /// </summary>
    /// <returns>
    /// A new builder instance.
    /// </returns>
    public static DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext> Create()
    {
        return new DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext>();
    }

    /// <summary>
    /// Configures the delegate used to create an engine object from
    /// a definition and context.
    /// </summary>
    /// <param name="create">
    /// The delegate that creates an engine object from a definition
    /// and the current context.
    /// </param>
    /// <returns>
    /// This builder instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="create"/> is <see langword="null"/>.
    /// </exception>
    public DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext> WithCreateInstance(
        Func<TDefinition, TContext, TObject> create)
    {
        ArgumentNullException.ThrowIfNull(create);

        _create = create;

        return this;
    }

    /// <summary>
    /// Configures the optional delegate invoked when an engine object
    /// is attached to its definition within a context.
    /// </summary>
    /// <param name="attach">
    /// The delegate that handles attachment of an engine object
    /// to its definition.
    /// </param>
    /// <returns>
    /// This builder instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="attach"/> is <see langword="null"/>.
    /// </exception>
    public DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext> WithAttachInstance(
        Action<TDefinition, TObject, TContext> attach)
    {
        ArgumentNullException.ThrowIfNull(attach);

        _attach = attach;

        return this;
    }

    /// <summary>
    /// Configures the optional delegate invoked when an engine object
    /// is released from its definition within a context.
    /// </summary>
    /// <param name="release">
    /// The delegate that handles release of an engine object
    /// from its definition.
    /// </param>
    /// <returns>
    /// This builder instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="release"/> is <see langword="null"/>.
    /// </exception>
    public DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext> WithReleaseInstance(
        Action<TDefinition, TObject, TContext> release)
    {
        ArgumentNullException.ThrowIfNull(release);

        _release = release;

        return this;
    }

    /// <summary>
    /// Builds a definition binding factory using the configured delegates.
    /// </summary>
    /// <returns>
    /// A definition binding factory that uses the configured create,
    /// attach, and release operations.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no create delegate has been configured.
    /// </exception>
    public IDefinitionBindingFactory<TDefinition, TObject, TContext> Build()
    {
        if (_create is null)
        {
            throw new InvalidOperationException(
                "A create delegate must be configured.");
        }

        return new Factory(
            _create,
            _attach,
            _release);
    }

    private sealed class Factory :
        IDefinitionBindingFactory<TDefinition, TObject, TContext>
    {
        private readonly Func<TDefinition, TContext, TObject> _create;
        private readonly Action<TDefinition, TObject, TContext>? _attach;
        private readonly Action<TDefinition, TObject, TContext>? _release;

        public Factory(
            Func<TDefinition, TContext, TObject> create,
            Action<TDefinition, TObject, TContext>? attach,
            Action<TDefinition, TObject, TContext>? release)
        {
            _create = create;
            _attach = attach;
            _release = release;
        }

        public TObject CreateInstance(
            TDefinition definition,
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNull(context);

            var instance =
                _create(
                    definition,
                    context);

            if (instance is null)
            {
                throw new InvalidOperationException(
                    "The configured create delegate returned null.");
            }

            return instance;
        }

        public void AttachInstance(
            TDefinition definition,
            TObject instance,
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentNullException.ThrowIfNull(context);

            _attach?.Invoke(
                definition,
                instance,
                context);
        }

        public void ReleaseInstance(
            TDefinition definition,
            TObject instance,
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentNullException.ThrowIfNull(context);

            _release?.Invoke(
                definition,
                instance,
                context);
        }
    }
}