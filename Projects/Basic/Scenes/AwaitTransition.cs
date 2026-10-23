using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Scenes;

/// <summary>
/// Provides a drawable transition that remains active while an asynchronous operation runs.
/// </summary>
public abstract class AwaitTransition : ITransition
{
    /// <summary>
    /// Initializes an asynchronous transition with the operation to await.
    /// </summary>
    /// <param name="operation">The operation determining transition completion.</param>
    /// <exception cref="ArgumentNullException">The operation is null.</exception>
    protected AwaitTransition(Task operation)
    {
        Completion = operation ?? throw new ArgumentNullException(nameof(operation));
    }

    /// <inheritdoc/>
    public Task Completion { get; }

    /// <inheritdoc/>
    public bool IsCompleted => Completion.IsCompletedSuccessfully;

    /// <inheritdoc/>
    public virtual void Update(GameTime gameTime)
    {
        ArgumentNullException.ThrowIfNull(gameTime);
    }

    /// <inheritdoc/>
    public abstract void Draw(SceneDrawContext context);
}
