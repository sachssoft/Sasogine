#if ANDROID
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using Sachssoft.Engine;

namespace SASOGINE_ROOT_NAMESPACE;

/// <summary>Android entry activity for the Sasogine game.</summary>
[Activity(Label = "SasogineTemplate", MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden)]
public sealed class MainActivity : AndroidGameActivity
{
    private SasogineTemplateApp? _game;

    /// <inheritdoc />
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        _game = new SasogineTemplateApp(new GameConfiguration());
        SetContentView((View)_game.Services.GetService(typeof(View))!);
        _game.Run();
    }
}
#endif
