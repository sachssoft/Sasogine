using System;

namespace Sachssoft.Engine.Common;

/// <summary>
/// Provides a fluent builder for creating an
/// <see cref="IDefinitionBindingFactory{TDefinition, TObject}"/>.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to create objects.
/// </typeparam>
/// <typeparam name="TObject">
/// The type of object created from a definition.
/// </typeparam>
public sealed class DefinitionBindingFactoryBuilder<TDefinition, TObject>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    private Func<TDefinition, TObject>? _create;
    private Action<TDefinition, TObject>? _attach;
    private Action<TDefinition, TObject>? _release;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DefinitionBindingFactoryBuilder{TDefinition, TObject}"/>
    /// class.
    /// </summary>
    private DefinitionBindingFactoryBuilder()
    {
    }

    /// <summary>
    /// Creates a new definition binding factory builder.
    /// </summary>
    /// <returns>
    /// A new builder instance.
    /// </returns>
    public static DefinitionBindingFactoryBuilder<TDefinition, TObject> Create()
    {
        return new DefinitionBindingFactoryBuilder<TDefinition, TObject>();
    }

    /// <summary>
    /// Configures the function used to create an object from a definition.
    /// </summary>
    /// <param name="create">
    /// The function used to create the object.
    /// </param>
    /// <returns>
    /// The current builder instance.
    /// </returns>
    public DefinitionBindingFactoryBuilder<TDefinition, TObject> WithCreateInstance(
        Func<TDefinition, TObject> create)
    {
        ArgumentNullException.ThrowIfNull(create);

        _create = create;
        return this;
    }

    /// <summary>
    /// Configures the action invoked when an object is attached to a binding.
    /// </summary>
    /// <param name="attach">
    /// The action invoked with the definition and its associated object.
    /// </param>
    /// <returns>
    /// The current builder instance.
    /// </returns>
    public DefinitionBindingFactoryBuilder<TDefinition, TObject> WithAttachInstance(
        Action<TDefinition, TObject> attach)
    {
        ArgumentNullException.ThrowIfNull(attach);

        _attach = attach;
        return this;
    }

    /// <summary>
    /// Configures the action invoked when a bound object is released.
    /// </summary>
    /// <param name="release">
    /// The action invoked with the definition and its associated object.
    /// </param>
    /// <returns>
    /// The current builder instance.
    /// </returns>
    public DefinitionBindingFactoryBuilder<TDefinition, TObject> WithReleaseInstance(
        Action<TDefinition, TObject> release)
    {
        ArgumentNullException.ThrowIfNull(release);

        _release = release;
        return this;
    }

    /// <summary>
    /// Builds the configured definition binding factory.
    /// </summary>
    /// <returns>
    /// A factory using the configured creation and destruction operations.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// No creation function has been configured.
    /// </exception>
    public IDefinitionBindingFactory<TDefinition, TObject> Build()
    {
        if (_create is null)
        {
            throw new InvalidOperationException(
                "A create delegate must be configured before building the factory.");
        }

        return Factory.Create(
            _create,
            _attach,
            _release);
    }

    /// <summary>
    /// Implements a definition binding factory using delegates configured
    /// by the builder.
    /// </summary>
    private sealed class Factory :
        IDefinitionBindingFactory<TDefinition, TObject>
    {
        private readonly Func<TDefinition, TObject> _create;
        private readonly Action<TDefinition, TObject>? _attach;
        private readonly Action<TDefinition, TObject>? _release;

        /// <summary>
        /// Initializes a new factory.
        /// </summary>
        /// <param name="create">
        /// The function used to create objects.
        /// </param>
        /// <param name="attach">
        /// The optional action used to attach objects.
        /// </param>
        /// <param name="release">
        /// The optional action used to release objects.
        /// </param>
        private Factory(
            Func<TDefinition, TObject> create,
            Action<TDefinition, TObject>? attach,
            Action<TDefinition, TObject>? release)
        {
            _create = create;
            _attach = attach;
            _release = release;
        }

        /// <summary>
        /// Creates a new factory using the specified delegates.
        /// </summary>
        /// <param name="create">
        /// The function used to create objects.
        /// </param>
        /// <param name="attach">
        /// The optional action used to attach objects.
        /// </param>
        /// <param name="release">
        /// The optional action used to release objects.
        /// </param>
        /// <returns>
        /// A new factory instance.
        /// </returns>
        public static Factory Create(
            Func<TDefinition, TObject> create,
            Action<TDefinition, TObject>? attach,
            Action<TDefinition, TObject>? release)
        {
            ArgumentNullException.ThrowIfNull(create);

            return new Factory(
                create,
                attach,
                release);
        }

        /// <inheritdoc/>
        public TObject CreateInstance(TDefinition definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            TObject instance = _create(definition);

            if (instance is null)
            {
                throw new InvalidOperationException(
                    "The configured create delegate returned null.");
            }

            return instance;
        }

        /// <inheritdoc/>
        public void AttachInstance(
            TDefinition definition,
            TObject instance)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNull(instance);

            _attach?.Invoke(
                definition,
                instance);
        }

        /// <inheritdoc/>
        public void ReleaseInstance(
            TDefinition definition,
            TObject instance)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNull(instance);

            _release?.Invoke(
                definition,
                instance);
        }
    }
}