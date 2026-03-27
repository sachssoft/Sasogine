using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Controls;

namespace Sachssoft.Sasogine.Surface.Scenes;

public abstract class SurfaceSceneBase : SceneBase
{
    public SurfaceSceneBase()
        : base()
    {
    }

    public new Desktop Host => (Desktop)base.Host!;
}
