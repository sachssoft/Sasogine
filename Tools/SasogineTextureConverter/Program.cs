using Avalonia;

namespace Sachssoft.TextureConverter;

internal static class Program
{
    /// <summary>
    /// Starts the texture converter as a desktop application.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to Avalonia.</param>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Configures the cross-platform Avalonia application and its native platform backend.
    /// </summary>
    /// <returns>A configured Avalonia application builder.</returns>
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
}
