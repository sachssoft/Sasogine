namespace Sachssoft.Engine.Controls.Examples;
internal sealed record ExampleOptions(string FontPath, string? SnapshotPath, string SkinId, bool Verify, string? ValueType = null, bool Assets = false)
{
    internal static ExampleOptions Parse(string[] args)
    {
        string? font = null, snapshot = null, valueType = null;
        string skin = "Modern";
        bool verify = false, assets = false;
        for (int i = 0; i < args.Length; i++)
        {
            string option = args[i];
            if (option == "--assets") { assets = true; continue; }
            if (option == "--verify")
            {
                verify = true;
                continue;
            }

            if (option is not ("--font" or "--snapshot" or "--skin" or "--values") || ++i == args.Length)
                throw new ArgumentException("Use --font, --snapshot, --skin, --values or --verify.");
            if (option == "--font")
                font = Path.GetFullPath(args[i]);
            else if (option == "--snapshot")
                snapshot = Path.GetFullPath(args[i]);
            else if (option == "--values")
                valueType = args[i];
            else
                skin = args[i].ToLowerInvariant() switch
                {
                    "dark" => "Dark",
                    "light" => "Light",
                    "modern" => "Modern",
                    "futuristic" => "Futuristic",
                    "medieval" => "Medieval",
                    _ => throw new ArgumentException("Unknown skin.")};
        }

        return new(ExampleFont.Resolve(font), snapshot, skin, verify, valueType, assets);
    }
}
