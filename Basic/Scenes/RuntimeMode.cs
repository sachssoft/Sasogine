namespace Sachssoft.Sasogine.Scenes;

/// <summary>
/// Defines the execution mode of the runtime environment.
/// </summary>
public enum RuntimeMode
{
    /// <summary>
    /// Normal game execution mode.
    /// </summary>
    Game,

    /// <summary>
    /// Editor mode for creating and modifying content.
    /// </summary>
    Editor,

    /// <summary>
    /// Preview mode for viewing content without full game execution.
    /// </summary>
    Preview,

    /// <summary>
    /// Debug mode with additional diagnostic features.
    /// </summary>
    Debug,

    /// <summary>
    /// Simulation mode for testing systems such as physics or logic.
    /// </summary>
    Simulation,

    /// <summary>
    /// Replay mode for playing back recorded sessions.
    /// </summary>
    Replay
}