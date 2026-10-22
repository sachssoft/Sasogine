namespace Sachssoft.Engine.Controls.Examples;
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--help", StringComparer.Ordinal))
        {
            Console.WriteLine("Sasogine Controls example\n  --font <file.ttf>   Use your own font.\n  --snapshot <file.png>   Render one frame, save it, then exit.\n  --page controls|layout|advanced|curve|tools|bars|fonts   Select a gallery page.\n  --skin dark|light|modern|futuristic|medieval   Select a skin.");
            return 0;
        }

        try
        {
            using var application = new ControlsExampleApp(ExampleOptions.Parse(args));
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
