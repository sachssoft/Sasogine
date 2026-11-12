using Sachssoft.Engine.Diagnostics;
using Sachssoft.Engine.Resources.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine;

/// <summary>
/// Provides configuration for a game application.
/// </summary>
public class GameConfiguration
{
    /// <summary>Gets or sets the graphics settings applied before the device is created.</summary>
    public GraphicsConfiguration Graphics { get; set; } = new()
    {
        UseDisplayResolution = true,
        HardwareModeSwitch = true,
        MultiSampling = true,
        Profile = GraphicsProfile.HiDef,
        DepthStencilFormat = DepthFormat.Depth24Stencil8,
        SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
    };

    /// <summary>Gets or sets the initial root directory for the MonoGame content manager.</summary>
    public string ContentRootDirectory { get; set; } = "Content";

    /// <summary>
    /// Gets the service manager used to configure application services.
    /// </summary>
    public GameServiceManager Services { get; } =
        new GameServiceManager();

    /// <summary>
    /// Gets or sets the diagnostic output used by the application.
    /// </summary>
    public IApplicationDebug? Debug { get; set; }

    /// <summary>
    /// Gets or sets the initial language used by the application.
    /// </summary>
    /// <remarks>
    /// A value of <see langword="null"/> indicates that no initial
    /// language is explicitly configured.
    /// </remarks>
    public Language? Language { get; set; }
}
