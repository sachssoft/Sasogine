
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Basic;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine;

public interface IGameApplication
{
    public static IGameApplication Current { get; internal set; } = null!;

    GameConfiguration Configuration { get; }

    GraphicsDevice GraphicsDevice { get; }

    ContentManager Content { get; }

    ISceneManager Scenes { get; }

    GameResourceManager Resources { get; }

    bool IsActive { get; }

    IGameSettings Settings { get; }

    GameServiceContainer Services { get; }

    void Exit();
}
