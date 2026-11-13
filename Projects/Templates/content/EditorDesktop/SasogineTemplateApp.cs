using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using System;

namespace SASOGINE_ROOT_NAMESPACE;

/// <summary>Hosts the Sasogine desktop editor.</summary>
public sealed class SasogineTemplateApp : GameApplicationBase
{
    /// <summary>Creates the editor application.</summary>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="args">Command-line arguments.</param>
    public SasogineTemplateApp(GameConfiguration configuration, string[]? args = null)
        : base(ConfigureGame(configuration), args ?? [])
    {
    }

    /// <inheritdoc />
    protected override void ConfigureApplication(ApplicationConfiguration configuration)
    {
        configuration.StartScene = _ => new EditorScene();
    }

    /// <inheritdoc />
    protected override void Initialize()
    {
        base.Initialize();
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.Title = "SasogineTemplate Editor";
    }

    private static GameConfiguration ConfigureGame(GameConfiguration configuration)
    {
        configuration.Graphics = new GraphicsConfiguration
        {
            UseDisplayResolution = true,
            MultiSampling = true,
            Profile = GraphicsProfile.Reach,
            DepthStencilFormat = DepthFormat.Depth24Stencil8
        };
        return configuration;
    }
}
