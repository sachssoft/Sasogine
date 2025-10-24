namespace Sachssoft.Sasogine.Presentation;



/// <summary>
/// Defines the switch mode when changing scenes.
/// </summary>
public enum SceneSwitchMode
{
    /// <summary>
    /// The old scene is unloaded and the new scene is loaded.
    /// </summary>
    Reload,

    /// <summary>
    /// The old scene remains loaded but is inactive.
    /// </summary>
    KeepAlive
}