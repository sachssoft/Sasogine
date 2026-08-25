
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Localization;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine;

public interface IGameApplication
{
    public static IGameApplication Current { get; internal set; } = null!;

    GameConfiguration Configuration { get; }

    GraphicsDevice GraphicsDevice { get; }

    ContentManager Content { get; }

    LocalizationManager Localization { get; }

    ISceneManager Scenes { get; }

    AssetStore Assets { get; }

    GameRegistry Registry { get; }

    bool IsActive { get; }

    IGameSettings Settings { get; }

    GameServiceContainer Services { get; }

    void Exit();
}
