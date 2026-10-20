namespace Sachssoft.Engine.Controls.Examples;
internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Contains("--help", StringComparer.Ordinal))
        {
            Console.WriteLine("Sasogine simple editor\n--font <file.ttf>\n--document <file.json>  Open/save this file.\n--snapshot <file.png>  Render once and exit.\n--verify  Check real canvas input.\n--self-test  Check document editing and JSON without opening a window.");
            return 0;
        }

        try
        {
            if (args.Contains("--self-test", StringComparer.Ordinal))
            {
                EditorChecks.Run();
                return 0;
            }

            using var app = new EditorApplication(ExampleOptions.Parse(args));
            app.Run();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
