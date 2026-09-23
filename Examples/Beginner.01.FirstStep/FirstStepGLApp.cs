using Sachssoft.Engine.Scenes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Engine.Examples;

class FirstStepGLApp : GameApplicationBase
{
    protected override ISceneManager CreateScenes(GameConfiguration configuration)
    {
        return new SceneManager(this, new PreviewScene());
    }
}
