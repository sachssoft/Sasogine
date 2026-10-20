using System.Globalization;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Parses bounded benchmark settings before creating the graphics host.</summary>
internal sealed class BenchmarkOptions
{
    /// <summary>Gets the installed font file used by both adapters.</summary>
    public string FontPath { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "segoeui.ttf");
    /// <summary>Gets the result directory for CSV, Markdown and screenshots.</summary>
    public string OutputPath { get; private set; } = Path.GetFullPath(Path.Combine("Results-FrameBatch", DateTime.Now.ToString("yyyyMMdd-HHmmss")));
    /// <summary>Gets the number of measurement batches per library.</summary>
    public int Rounds { get; private set; } = 6;
    /// <summary>Gets the number of measured frame or draw operations in each batch.</summary>
    public int Frames { get; private set; } = 60;
    /// <summary>Gets the number of unmeasured operations before measuring each tree.</summary>
    public int Warmup { get; private set; } = 20;
    /// <summary>Gets the button counts included in this run.</summary>
    public int[] Counts { get; private set; } = [100, 1000, 5000];
    /// <summary>Gets whether to close automatically after saving results.</summary>
    public bool ExitWhenFinished { get; private set; }

    /// <summary>Parses command-line options and validates their supported ranges.</summary>
    public static BenchmarkOptions Parse(string[] args)
    {
        var options = new BenchmarkOptions();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--exit")
            {
                options.ExitWhenFinished = true;
                continue;
            }

            if (args[i] == "--quick")
            {
                options.Rounds = 4;
                options.Frames = 20;
                options.Warmup = 10;
                options.Counts = [100];
                continue;
            }

            string option = args[i];
            if (++i == args.Length)
                throw new ArgumentException($"Missing value for {option}.");
            switch (option)
            {
                case "--font":
                    options.FontPath = Path.GetFullPath(args[i]);
                    break;
                case "--output":
                    options.OutputPath = Path.GetFullPath(args[i]);
                    break;
                case "--rounds":
                    options.Rounds = ParseCount(args[i], 3, 31);
                    break;
                case "--frames":
                    options.Frames = ParseCount(args[i], 2, 10000);
                    break;
                case "--warmup":
                    options.Warmup = ParseCount(args[i], 2, 10000);
                    break;
                default:
                    throw new ArgumentException($"Unknown option {option}.");
            }
        }

        if (!File.Exists(options.FontPath))
            throw new FileNotFoundException("Use --font with an installed TTF font.", options.FontPath);
        return options;
    }

    private static int ParseCount(string text, int minimum, int maximum)
    {
        if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value) || value < minimum || value > maximum)
            throw new ArgumentException($"Expected a count between {minimum} and {maximum}.");
        return value;
    }
}
