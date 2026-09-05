using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides the base implementation for engine objects driven by a definition.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to configure the engine object.
/// </typeparam>
/// <remarks>
/// A definition contains the static configuration used to initialize the runtime
/// state of the object. Derived types can apply additional configuration by
/// overriding <see cref="ConfigureFromDefinition"/>.
/// </remarks>
public abstract class EngineObject<TDefinition> : EngineObjectBase, IEngineObject
    where TDefinition : class, IDefinition
{
    private TDefinition? _definition;

    /// <summary>
    /// Initializes a new instance of the <see cref="EngineObject{TDefinition}"/> class
    /// with the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition used to configure the engine object.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="definition"/> is <see langword="null"/>.
    /// </exception>
    protected EngineObject(TDefinition definition)
    {
        _definition = definition ??
            throw new ArgumentNullException(nameof(definition));
    }

    /// <summary>
    /// Occurs when the identifier of the engine object changes while applying
    /// its definition.
    /// </summary>
    public event EventHandler<EngineObjectChangedEventArgs>? IdChanged;

    /// <summary>
    /// Occurs when the class of the engine object changes while applying
    /// its definition.
    /// </summary>
    public event EventHandler<EngineObjectChangedEventArgs>? ClassChanged;

    /// <summary>
    /// Gets the definition used to configure this engine object.
    /// </summary>
    /// <remarks>
    /// The definition represents static configuration data and is typically
    /// used during loading, initialization, serialization, or editor operations.
    ///
    /// Runtime systems should generally copy frequently accessed values from
    /// the definition into runtime fields during initialization instead of
    /// repeatedly accessing the definition during performance-critical updates.
    ///
    /// If no definition is currently available,
    /// <see cref="EnsureDefinition"/> attempts to resolve one.
    /// </remarks>
    public TDefinition Definition
    {
        get
        {
            EnsureDefinition();
            return _definition;
        }
    }

    IDefinition IEngineObject.Definition => Definition;

    /// <summary>
    /// Loads the engine object and applies its definition.
    /// </summary>
    /// <remarks>
    /// Calling this method when the object is already loaded has no effect.
    /// </remarks>
    public sealed override void Load()
    {
        if (IsLoaded)
            return;

        EnsureDefinition();

        ConfigureFromDefinitionInternal();
        ConfigureFromDefinition();

        OnLoad();

        IsLoaded = true;
    }

    /// <summary>
    /// Asynchronously loads the engine object and applies its definition.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous load operation.
    /// </returns>
    /// <remarks>
    /// Calling this method when the object is already loaded has no effect.
    /// </remarks>
    public sealed override async Task LoadAsync()
    {
        if (IsLoaded)
            return;

        EnsureDefinition();

        ConfigureFromDefinitionInternal();
        ConfigureFromDefinition();

        await OnLoadAsync().ConfigureAwait(false);

        IsLoaded = true;
    }

    /// <summary>
    /// Unloads the engine object.
    /// </summary>
    /// <remarks>
    /// Calling this method when the object is not loaded has no effect.
    /// </remarks>
    public sealed override void Unload()
    {
        if (!IsLoaded)
            return;

        OnUnload();

        IsLoaded = false;
    }

    /// <summary>
    /// Ensures that a definition is available for this engine object.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="ResolveDefinition"/> does not provide a definition.
    /// </exception>
    [MemberNotNull(nameof(_definition))]
    public void EnsureDefinition()
    {
        if (_definition != null)
            return;

        _definition = ResolveDefinition()
            ?? throw new InvalidOperationException(
                $"ResolveDefinition returned null for {GetType().Name}.");
    }

    /// <summary>
    /// Resolves the definition used by this engine object when no definition
    /// is currently available.
    /// </summary>
    /// <returns>
    /// The resolved definition, or <see langword="null"/> if no definition
    /// could be resolved.
    /// </returns>
    /// <remarks>
    /// Derived classes can override this method to provide definitions from
    /// alternative sources such as asset stores, registries, or runtime providers.
    /// </remarks>
    protected virtual TDefinition? ResolveDefinition()
    {
        return _definition;
    }

    /// <summary>
    /// Applies definition-specific configuration to the engine object.
    /// </summary>
    /// <remarks>
    /// Override this method to copy configuration values from
    /// <see cref="Definition"/> into runtime fields.
    /// </remarks>
    protected virtual void ConfigureFromDefinition()
    {
    }

    /// <summary>
    /// Called when the engine object should load its runtime resources.
    /// </summary>
    protected virtual void OnLoad()
    {
    }

    /// <summary>
    /// Called when the engine object should asynchronously load its runtime resources.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous load operation.
    /// </returns>
    /// <remarks>
    /// The default implementation invokes <see cref="OnLoad"/>.
    /// </remarks>
    protected virtual Task OnLoadAsync()
    {
        OnLoad();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the engine object should unload its runtime resources.
    /// </summary>
    protected virtual void OnUnload()
    {
    }

    /// <summary>
    /// Applies common engine object properties from the definition and raises
    /// the corresponding change events when necessary.
    /// </summary>
    private void ConfigureFromDefinitionInternal()
    {
        if (Definition is not IEngineObjectDefinition engineObjectDefinition)
            return;

        if (!Equals(Id, engineObjectDefinition.Id))
        {
            var oldId = Id;

            Id = engineObjectDefinition.Id;

            IdChanged?.Invoke(
                this,
                new EngineObjectChangedEventArgs(
                    oldId,
                    Id,
                    Class,
                    Class));
        }

        if (!Equals(Class, engineObjectDefinition.Class))
        {
            var oldClass = Class;

            Class = engineObjectDefinition.Class;

            ClassChanged?.Invoke(
                this,
                new EngineObjectChangedEventArgs(
                    Id,
                    Id,
                    oldClass,
                    Class));
        }
    }
}