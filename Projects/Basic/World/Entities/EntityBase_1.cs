using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Scenes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World;

/// <summary>
/// Provides a base implementation for entities that are defined by data
/// and composed of updateable and drawable components.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to configure the entity.
/// </typeparam>
public abstract class EntityBase<TDefinition> :
    EngineObject<TDefinition>,
    IEntity,
    IComponentProvider,
    IUpdatableComponent,
    IDrawableComponent
    where TDefinition : class, IEntityDefinition
{
    private readonly ComponentCollection _components = new();

    private EntityIntegrity _integrity = EntityIntegrity.Intact;
    private ActivityState _activityState = ActivityState.Idle;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityBase{TDefinition}"/> class.
    /// </summary>
    /// <param name="definition">
    /// The definition used to configure the entity.
    /// </param>
    protected EntityBase(TDefinition definition)
        : base(definition)
    {
    }

    /// <summary>
    /// Occurs after the entity has been successfully loaded.
    /// </summary>
    public event EventHandler? Loaded;

    /// <summary>
    /// Occurs after the entity has been unloaded.
    /// </summary>
    public event EventHandler? Unloaded;

    /// <summary>
    /// Occurs when the integrity state of the entity changes.
    /// </summary>
    public event EventHandler? StatusChanged;

    /// <summary>
    /// Occurs when the activity state of the entity changes.
    /// </summary>
    public event EventHandler? ActivityStateChanged;

    /// <summary>
    /// Gets or sets a value indicating whether the entity participates
    /// in update processing.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is rendered.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Gets the current integrity state of the entity.
    /// </summary>
    public EntityIntegrity Integrity
    {
        get => _integrity;
        protected set
        {
            if (Equals(_integrity, value))
                return;

            _integrity = value;
            OnStatusChanged();
        }
    }

    /// <summary>
    /// Gets the current activity state of the entity.
    /// </summary>
    public ActivityState ActivityState
    {
        get => _activityState;
        protected set
        {
            if (Equals(_activityState, value))
                return;

            _activityState = value;
            OnActivityStateChanged();
        }
    }

    /// <summary>
    /// Updates all updateable components attached to the entity.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current scene update.
    /// </param>
    public virtual void Update(SceneUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!IsEnabled)
            return;

        _components.UpdateForEach(context);
    }

    /// <summary>
    /// Draws all drawable components attached to the entity.
    /// </summary>
    /// <param name="context">
    /// Provides information about the current scene drawing cycle.
    /// </param>
    public virtual void Draw(SceneDrawContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!IsVisible)
            return;

        _components.DrawForEach(context);
    }

    /// <summary>
    /// Attempts to retrieve a component of the specified type.
    /// </summary>
    /// <typeparam name="T">
    /// The type of component to retrieve.
    /// </typeparam>
    /// <param name="component">
    /// When this method returns <see langword="true"/>, contains the matching
    /// component; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if a matching component was found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetComponent<T>(
        [MaybeNullWhen(false)] out T component)
        where T : class, IComponent
    {
        return _components.TryGet(out component);
    }

    /// <summary>
    /// Gets the collection of components attached to the entity.
    /// </summary>
    protected ComponentCollection Components => _components;

    /// <summary>
    /// Called when the entity is loaded.
    /// </summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        OnLoaded();
    }

    /// <summary>
    /// Called when the entity is loaded asynchronously.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous load operation.
    /// </returns>
    protected override async Task OnLoadAsync()
    {
        await base.OnLoadAsync().ConfigureAwait(false);

        OnLoaded();
    }

    /// <summary>
    /// Called when the entity is unloaded.
    /// </summary>
    protected override void OnUnload()
    {
        base.OnUnload();

        OnUnloaded();
    }

    /// <summary>
    /// Raises the <see cref="Loaded"/> event.
    /// </summary>
    protected virtual void OnLoaded()
    {
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="Unloaded"/> event.
    /// </summary>
    protected virtual void OnUnloaded()
    {
        Unloaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="StatusChanged"/> event.
    /// </summary>
    protected virtual void OnStatusChanged()
    {
        StatusChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the <see cref="ActivityStateChanged"/> event.
    /// </summary>
    protected virtual void OnActivityStateChanged()
    {
        ActivityStateChanged?.Invoke(this, EventArgs.Empty);
    }
}