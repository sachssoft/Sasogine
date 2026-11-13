using Sachssoft.Engine;
using Sachssoft.Engine.Scenes;
using System;

namespace SASOGINE_ROOT_NAMESPACE;

/// <summary>Configures and runs the Sasogine application.</summary>
public sealed class SasogineTemplateApp : GameApplicationBase
{
    /// <summary>Creates a Sasogine application.</summary>
    /// <param name="configuration">The game configuration.</param>
    /// <param name="args">Optional command-line arguments.</param>
    public SasogineTemplateApp(GameConfiguration configuration, string[]? args = null)
        : base(configuration, args ?? [])
    {
    }

    /// <inheritdoc />
    protected override void ConfigureApplication(ApplicationConfiguration configuration)
    {
        configuration.StartScene = _ => new MainScene();
    }
}
