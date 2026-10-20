namespace Sachssoft.Engine.Controls.Examples;
internal sealed record ExampleOptions(string FontPath, string? SnapshotPath, string DocumentPath, bool Verify)
{
    internal static ExampleOptions Parse(string[] args)
    {
        string? font = null, snapshot = null;
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sachssoft", "EditorExample", "document.json");
        bool verify = false;
        for (int i = 0; i < args.Length; i++)
        {
            string option = args[i];
            if (option == "--verify")
            {
                verify = true;
                continue;
            }

            if (option is not ("--font" or "--snapshot" or "--document") || ++i == args.Length)
                throw new ArgumentException("Use --font, --snapshot, --document <path> or --verify.");
            string value = Path.GetFullPath(args[i]);
            if (option == "--font")
                font = value;
            else if (option == "--snapshot")
                snapshot = value;
            else
                path = value;
        }

        return new(ExampleFont.Resolve(font), snapshot, path, verify);
    }
}
