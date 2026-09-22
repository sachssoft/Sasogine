namespace Sachssoft.Engine.Common.Execution
{
    /// <summary>
    /// Represents contextual information associated with a runtime execution.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An execution context provides a common mechanism for passing contextual
    /// information to dynamically executed logic without coupling that logic
    /// to a specific execution system or backend.
    /// </para>
    /// <para>
    /// The context distinguishes between the primary object associated with
    /// the execution through <see cref="Target"/> and optional supplementary
    /// information through <see cref="Data"/>.
    /// </para>
    /// <para>
    /// Implementations may be used by scripting backends, plugins, extensions,
    /// runtime components, or other systems that require contextual information
    /// during execution.
    /// </para>
    /// </remarks>
    public interface IExecutionContext
    {
        /// <summary>
        /// Gets the primary object associated with the execution.
        /// </summary>
        /// <value>
        /// The primary target of the execution, or <see langword="null"/> when
        /// the execution is not associated with a specific object.
        /// </value>
        /// <remarks>
        /// <para>
        /// The target identifies the object on which, for which, or in relation
        /// to which the current operation is executed.
        /// </para>
        /// <para>
        /// Depending on the execution system, the target may represent an
        /// entity, component, asset, tool, plugin host, or another engine or
        /// application object.
        /// </para>
        /// <para>
        /// Execution backends may expose or translate the target into their
        /// own runtime representation.
        /// </para>
        /// </remarks>
        object? Target { get; }

        /// <summary>
        /// Gets optional supplementary data associated with the execution.
        /// </summary>
        /// <value>
        /// Additional execution-specific data, or <see langword="null"/> when
        /// no supplementary data is required.
        /// </value>
        /// <remarks>
        /// <para>
        /// Unlike <see cref="Target"/>, this value does not identify the primary
        /// object associated with the execution. It provides additional
        /// information required or made available for the current invocation.
        /// </para>
        /// <para>
        /// Examples include event information, interaction data, invocation
        /// arguments, collision information, plugin-specific data, or values
        /// supplied to a script execution.
        /// </para>
        /// <para>
        /// The concrete type and interpretation of this value are determined
        /// by the caller and the system processing the execution context.
        /// </para>
        /// </remarks>
        object? Data { get; }
    }
}