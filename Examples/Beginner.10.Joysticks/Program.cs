namespace Sachssoft.Engine.Controls.Examples;
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--help", StringComparer.Ordinal))
        {
            Console.WriteLine("Sasogine Virtual Joysticks example\n  --font <file.ttf>   Use your own font.\n  --snapshot <file.png>   Render one frame, save it, then exit.");
            return 0;
        }

        try
        {
            using var application = new JoystickExampleApp(ExampleOptions.Parse(args));
            application.Run();
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
