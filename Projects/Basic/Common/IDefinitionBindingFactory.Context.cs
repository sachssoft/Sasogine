namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines a context-aware factory for creating engine objects from
/// definitions and managing their binding lifecycle.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to create and bind engine objects.
/// </typeparam>
/// <typeparam name="TObject">
/// The type of engine object created and managed by the factory.
/// </typeparam>
/// <typeparam name="TContext">
/// The type of context supplied to creation, attachment, and release
/// operations.
/// </typeparam>
public interface IDefinitionBindingFactory<in TDefinition, TObject, in TContext>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
    where TContext : class
{
    /// <summary>
    /// Creates an engine object from the specified definition
    /// within the supplied context.
    /// </summary>
    /// <param name="definition">
    /// The definition used to create the engine object.
    /// </param>
    /// <param name="context">
    /// The context available during object creation.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    TObject CreateInstance(
        TDefinition definition,
        TContext context);

    /// <summary>
    /// Attaches an existing engine object to its definition
    /// within the supplied context.
    /// </summary>
    /// <param name="definition">
    /// The definition associated with the engine object.
    /// </param>
    /// <param name="instance">
    /// The engine object to attach.
    /// </param>
    /// <param name="context">
    /// The context available during attachment.
    /// </param>
    void AttachInstance(
        TDefinition definition,
        TObject instance,
        TContext context);

    /// <summary>
    /// Releases an engine object from its definition
    /// within the supplied context.
    /// </summary>
    /// <param name="definition">
    /// The definition associated with the engine object.
    /// </param>
    /// <param name="instance">
    /// The engine object to release.
    /// </param>
    /// <param name="context">
    /// The context available during release.
    /// </param>
    void ReleaseInstance(
        TDefinition definition,
        TObject instance,
        TContext context);
}