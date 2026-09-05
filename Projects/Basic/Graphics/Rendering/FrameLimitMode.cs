namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Specifies how the frame rate is limited.
/// </summary>
public enum FrameLimitMode
{
    /// <summary>
    /// Uses the default frame rate limit.
    /// </summary>
    Default,

    /// <summary>
    /// Disables the frame rate limit.
    /// </summary>
    Unlimited,

    /// <summary>
    /// Uses a custom frame rate limit.
    /// </summary>
    Custom
}