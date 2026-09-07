using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Defines an entity that contains executable script logic or actions.
    /// </summary>
    /// <remarks>
    /// Script entities can be used by both editor and runtime systems and support
    /// synchronous and asynchronous execution.
    /// </remarks>
    public interface IScriptEntity : IEntity
    {
        /// <summary>
        /// Occurs when the current script execution finishes or is interrupted.
        /// </summary>
        event EventHandler? ExecutionFinished;

        /// <summary>
        /// Gets a value indicating whether the script is currently executing.
        /// </summary>
        /// <value>
        /// <see langword="true"/> while the script is running; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool IsRunning { get; }

        /// <summary>
        /// Executes the script or associated logic synchronously.
        /// </summary>
        /// <param name="args">
        /// Optional arguments passed to the script.
        /// </param>
        void Execute(params object?[] args);

        /// <summary>
        /// Executes the script or associated logic asynchronously.
        /// </summary>
        /// <param name="args">
        /// Optional arguments passed to the script.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous execution operation.
        /// </returns>
        Task ExecuteAsync(params object?[] args);
    }
}