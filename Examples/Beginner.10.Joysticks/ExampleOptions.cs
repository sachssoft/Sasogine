namespace Sachssoft.Engine.Controls.Examples;
internal sealed class ExampleOptions
{
    private ExampleOptions(string fontPath, string? snapshotPath)
    {
        FontPath = fontPath;
        SnapshotPath = snapshotPath;
    }

    internal string FontPath { get; }
    internal string? SnapshotPath { get; }

    internal static ExampleOptions Parse(string[] args)
    {
        string? font = null, snapshot = null;
        for (int i = 0; i < args.Length; i++)
        {
            string option = args[i];
            if (option is not ("--font" or "--snapshot"))
                throw new ArgumentException($"Unknown option '{option}'. Use --help.");
            if (++i == args.Length)
                throw new ArgumentException($"Missing path for {option}.");
            if (option == "--font")
                font = Path.GetFullPath(args[i]);
            else
                snapshot = Path.GetFullPath(args[i]);
        }

        return new ExampleOptions(ExampleFont.Resolve(font), snapshot);
    }
}
