
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Localization;
using Sachssoft.Sasogine.Platform;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Resources;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine;

public interface IGameApplication
{

    [AllowNull]
    public static IGameApplication Current { get; internal set; }

    public static Game Game => (Game)Current;

    GraphicsDevice GraphicsDevice { get; }

    ContentManager Content { get; }

    SceneManager Scenes { get; }

    PlatformProfiles PlatformProfile { get; }

    LocalizationManager Localization {  get; }

    GameResourceManager Resources { get; }

    bool IsActive { get; }

    //TSettings GetSettings<TSettings>() where TSettings : GameSettings, new();

    GameServiceContainer Services { get; }

    void Exit();
}
