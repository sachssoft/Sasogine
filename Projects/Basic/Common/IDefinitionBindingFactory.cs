namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines a factory that creates and releases engine objects from definitions.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition used to create engine objects.
/// </typeparam>
/// <typeparam name="TObject">
/// The type of engine object created by the factory.
/// </typeparam>
public interface IDefinitionBindingFactory<in TDefinition, TObject>
    where TDefinition : class, IDefinition
    where TObject : class, IEngineObject
{
    /// <summary>
    /// Creates an engine object for the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition from which the engine object is created.
    /// </param>
    /// <returns>
    /// The created engine object.
    /// </returns>
    TObject CreateInstance(TDefinition definition);

    /// <summary>
    /// Attaches an engine object to the specified definition binding.
    /// </summary>
    /// <param name="definition">
    /// The definition associated with the engine object.
    /// </param>
    /// <param name="instance">
    /// The engine object to attach.
    /// </param>
    void AttachInstance(TDefinition definition, TObject instance);

    /// <summary>
    /// Releases an engine object previously created for the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition associated with the engine object.
    /// </param>
    /// <param name="instance">
    /// The engine object to release.
    /// </param>
    void ReleaseInstance(TDefinition definition, TObject instance);
}