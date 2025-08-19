/* 
 * © 2024 Tobias Sachs
 * IMyGameApp
 * 08.07.2024 
*/

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Surface;
using Microsoft.Xna.Framework;
using System.ComponentModel.Design;

namespace Sachssoft.Sasogine;

public interface IMyGameApp
{

    [AllowNull]
    public static IMyGameApp Current { get; internal set; }

    public static Game Game => (Game)Current;

    GraphicsDevice GraphicsDevice { get; }

    ContentManager Content { get; }

    ViewManager View { get; }

    IEnumerable<Region> Regions { get; }

    Region CurrentRegion { get; }

    PlatformProfiles PlatformProfile { get; }

    GameAssetManager Assets { get; }

    bool IsActive { get; }

    GameSettings GetSettings(int index);

    GameServiceContainer Services { get; }

    //ViewBase? GetCurrentView(); 
    
    //void OpenView<TView>(Action<TView>? init = null) where TView : ViewBase;

    //void CloseView<TView>(TView view) where TView : ViewBase;

    //void SwitchView<TView>(Action<TView>? init = null) where TView : ViewBase;

    //void GoBackView(Action<ViewBase>? init = null);

    void Exit();
}
