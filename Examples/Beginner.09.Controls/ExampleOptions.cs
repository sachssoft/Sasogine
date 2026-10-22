namespace Sachssoft.Engine.Controls.Examples;
internal sealed class ExampleOptions
{
    private ExampleOptions(string fontPath, string? snapshotPath, int pageIndex, string skinId)
    {
        FontPath = fontPath;
        SnapshotPath = snapshotPath;
        PageIndex = pageIndex;
        SkinId = skinId;
    }

    internal string FontPath { get; }
    internal string? SnapshotPath { get; }
    internal int PageIndex { get; }
    internal string SkinId { get; }

    internal static ExampleOptions Parse(string[] args)
    {
        string? font = null, snapshot = null;
        int page = 0;
        string skin = "dark";
        for (int i = 0; i < args.Length; i++)
        {
            string option = args[i];
            if (option is not ("--font" or "--snapshot" or "--page" or "--skin"))
                throw new ArgumentException($"Unknown option '{option}'. Use --help.");
            if (++i == args.Length)
                throw new ArgumentException($"Missing path for {option}.");
            if (option == "--font")
                font = Path.GetFullPath(args[i]);
            else if (option == "--snapshot")
                snapshot = Path.GetFullPath(args[i]);
            else if (option == "--skin")
                skin = args[i] is "dark" or "light" or "modern" or "futuristic" or "medieval" ? args[i] : throw new ArgumentException("Unknown skin ID.");
            else
                page = args[i] switch
                {
                    "controls" => 0,
                    "layout" => 1,
                    "advanced" => 2,
                    "curve" => 3,
                    "tools" => 4,
                    "bars" => 5,
                    "fonts" => 6,
                    _ => throw new ArgumentException("Page must be controls, layout, advanced, curve, tools, bars or fonts.")};
        }

        return new ExampleOptions(ExampleFont.Resolve(font), snapshot, page, skin);
    }
}
