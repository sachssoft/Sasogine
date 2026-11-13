using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using System;

namespace SASOGINE_ROOT_NAMESPACE;

/// <summary>Shared Sasogine mobile game application.</summary>
public sealed class SasogineTemplateApp : GameApplicationBase
{
    /// <summary>Creates the mobile application.</summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="args">Command-line arguments.</param>
    public SasogineTemplateApp(GameConfiguration configuration, string[]? args = null)
        : base(ConfigureGame(configuration), args ?? [])
    {
    }

    /// <inheritdoc />
    protected override void ConfigureApplication(ApplicationConfiguration configuration)
    {
        configuration.StartScene = _ => new MainScene();
    }

    private static GameConfiguration ConfigureGame(GameConfiguration configuration)
    {
        configuration.Graphics = new GraphicsConfiguration
        {
            FullScreen = true,
            UseDisplayResolution = true,
            HardwareModeSwitch = false,
            Profile = GraphicsProfile.Reach,
            SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight,
            SynchronizeWithVerticalRetrace = true
        };
        return configuration;
    }
}
