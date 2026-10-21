namespace Sachssoft.Engine.Controls.Examples;
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Contains("--self-test", StringComparer.Ordinal))
            {
                InspectorChecks.Run();
                return 0;
            }

            if (args.Contains("--help", StringComparer.Ordinal))
            {
                Console.WriteLine("Sasogine Logo Inspector: --skin dark|light|modern|futuristic|medieval --font file.ttf --snapshot image.png --values Bounds3 --assets --dpi 144 --verify-resize --verify --self-test");
                return 0;
            }

            using var app = new LogoInspectorApplication(ExampleOptions.Parse(args));
            app.Run();
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }
}
