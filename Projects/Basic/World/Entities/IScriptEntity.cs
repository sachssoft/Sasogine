using Sachssoft.Engine.Execution;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Engine.World;

/// <summary>
/// Represents an entity that provides executable script logic.
/// </summary>
/// <remarks>
/// <para>
/// A script entity extends <see cref="IEntity"/> with executable logic that
/// can be invoked synchronously or asynchronously.
/// </para>
/// <para>
/// Execution-specific information is supplied through an
/// <see cref="IExecutionContext"/>, allowing the script implementation to
/// remain independent of a specific scripting language or backend.
/// </para>
/// </remarks>
public interface IScriptEntity : IEntity
{
    /// <summary>
    /// Gets a value indicating whether the script is currently executing.
    /// </summary>
    /// <value>
    /// <see langword="true"/> while an execution is in progress; otherwise,
    /// <see langword="false"/>.
    /// </value>
    bool IsRunning { get; }

    /// <summary>
    /// Occurs when the current script execution has finished.
    /// </summary>
    /// <remarks>
    /// The event is raised when an execution is no longer running, including
    /// executions that were cancelled or otherwise interrupted.
    /// </remarks>
    event EventHandler? ExecutionFinished;

    /// <summary>
    /// Executes the script synchronously using the specified execution context.
    /// </summary>
    /// <param name="context">
    /// The context containing the target and optional additional data associated
    /// with the execution.
    /// </param>
    void Execute(IExecutionContext context);

    /// <summary>
    /// Executes the script asynchronously using the specified execution context.
    /// </summary>
    /// <param name="context">
    /// The context containing the target and optional additional data associated
    /// with the execution.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the asynchronous execution.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous script execution.
    /// </returns>
    Task ExecuteAsync(
        IExecutionContext context,
        CancellationToken cancellationToken = default);
}