#if IOS
using Foundation;
using UIKit;
using Sachssoft.Engine;

namespace SASOGINE_ROOT_NAMESPACE;

/// <summary>iOS application delegate for the Sasogine game.</summary>
[Register("AppDelegate")]
public sealed class AppDelegate : UIApplicationDelegate
{
    private SasogineTemplateApp? _game;

    /// <inheritdoc />
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        _game = new SasogineTemplateApp(new GameConfiguration());
        _game.Run();
        return true;
    }
}
#endif
