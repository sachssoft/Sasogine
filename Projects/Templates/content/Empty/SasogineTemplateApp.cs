using Sachssoft.Engine;
using Sachssoft.Engine.Scenes;
using System;

namespace SASOGINE_ROOT_NAMESPACE;

/// <summary>Minimal Sasogine application without a separate scene file.</summary>
public sealed class SasogineTemplateApp : GameApplicationBase
{
    /// <summary>Creates the minimal Sasogine application.</summary>
    /// <param name="configuration">The game configuration.</param>
    /// <param name="args">Optional command-line arguments.</param>
    public SasogineTemplateApp(GameConfiguration configuration, string[]? args = null)
        : base(configuration, args ?? [])
    {
    }

    /// <inheritdoc />
    protected override void ConfigureApplication(ApplicationConfiguration configuration)
    {
        configuration.StartScene = _ => new EmptyScene();
    }

    private sealed class EmptyScene : BasicSceneBase
    {
    }
}
