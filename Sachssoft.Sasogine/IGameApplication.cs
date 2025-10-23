/* 
 * © 2024 Tobias Sachs
 * IMyGameApp
 * 08.07.2024 
*/

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sachssoft.Sasogine.Graphics;
using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation;

namespace Sachssoft.Sasogine;

public interface IGameApplication
{

    [AllowNull]
    public static IGameApplication Current { get; internal set; }

    public static Game Game => (Game)Current;

    GraphicsDevice GraphicsDevice { get; }

    ContentManager Content { get; }

    SceneManager View { get; }

    IEnumerable<Region> Regions { get; }

    Region CurrentRegion { get; }

    PlatformProfiles PlatformProfile { get; }

    GameAssetManager Assets { get; }

    bool IsActive { get; }

    TSettings GetSettings<TSettings>() where TSettings : GameSettings, new();

    GameServiceContainer Services { get; }

    void Exit();
}
