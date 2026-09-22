namespace Sachssoft.Engine;

/// <summary>
/// Defines the application context available to engine components.
/// </summary>
public interface IApplicationContext
{
    /// <summary>
    /// Gets the game application associated with this context.
    /// </summary>
    GameApplicationBase Application { get; }
}