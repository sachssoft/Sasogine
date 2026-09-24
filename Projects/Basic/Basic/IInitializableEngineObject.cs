namespace Sachssoft.Engine;

/// <summary>
/// Defines an engine object that supports context-based initialization.
/// </summary>
public interface IInitializableEngineObject : IEngineObject
{
    /// <summary>
    /// Gets a value indicating whether the engine object is initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes the engine object using the specified context.
    /// </summary>
    /// <param name="context">
    /// The context used to initialize the engine object.
    /// </param>
    void Initialize(IEngineObjectContext context);

    /// <summary>
    /// Deinitializes the engine object.
    /// </summary>
    void Deinitialize();
}