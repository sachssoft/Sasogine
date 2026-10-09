using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Sachssoft.TextureConverter;

/// <summary>
/// Initializes the Avalonia application and opens the texture converter window.
/// </summary>
public sealed partial class App : Application
{
    /// <summary>
    /// Loads the application styles and XAML resources.
    /// </summary>
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <summary>
    /// Creates the main window after the Avalonia framework has initialized.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}
