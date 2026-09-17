using System;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Builds a context-aware definition binding factory.
/// </summary>
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

    public static DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext> Create()
    {
        return new DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext>();
    }

    public DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext> WithCreateInstance(
        Func<TDefinition, TContext, TObject> create)
    {
        ArgumentNullException.ThrowIfNull(create);
        _create = create;
        return this;
    }

    public DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext> WithAttachInstance(
        Action<TDefinition, TObject, TContext> attach)
    {
        ArgumentNullException.ThrowIfNull(attach);
        _attach = attach;
        return this;
    }

    public DefinitionBindingFactoryBuilder<TDefinition, TObject, TContext> WithReleaseInstance(
        Action<TDefinition, TObject, TContext> release)
    {
        ArgumentNullException.ThrowIfNull(release);
        _release = release;
        return this;
    }

    public IDefinitionBindingFactory<TDefinition, TObject, TContext> Build()
    {
        if (_create is null)
            throw new InvalidOperationException("A create delegate must be configured.");

        return new Factory(_create, _attach, _release);
    }

    private sealed class Factory : IDefinitionBindingFactory<TDefinition, TObject, TContext>
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

        public TObject CreateInstance(TDefinition definition, TContext context)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNull(context);

            var instance = _create(definition, context);

            if (instance is null)
                throw new InvalidOperationException("The configured create delegate returned null.");

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

            _attach?.Invoke(definition, instance, context);
        }

        public void ReleaseInstance(
            TDefinition definition,
            TObject instance,
            TContext context)
        {
            ArgumentNullException.ThrowIfNull(definition);
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentNullException.ThrowIfNull(context);

            _release?.Invoke(definition, instance, context);
        }
    }
}
