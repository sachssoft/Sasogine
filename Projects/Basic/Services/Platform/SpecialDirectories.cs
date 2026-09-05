namespace Sachssoft.Sasogine.Services.Platform;

/// <summary>
/// Specifies common platform-specific special directories.
/// </summary>
public enum SpecialDirectories
{
    /// <summary>
    /// Directory for cached application data.
    /// </summary>
    Cache,

    /// <summary>
    /// Directory for temporary application data.
    /// </summary>
    Temporary,

    /// <summary>
    /// Directory for application-specific data.
    /// </summary>
    Application,

    /// <summary>
    /// Directory associated with the current user.
    /// </summary>
    User
}