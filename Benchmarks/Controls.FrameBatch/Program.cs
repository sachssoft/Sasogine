namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Starts the real game-loop comparison.</summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Contains("--help"))
            {
                Console.WriteLine("GameLoop [--quick] [--exit] [--output directory] [--font file.ttf] [--rounds 3..31] [--frames 2..10000] [--warmup 2..10000]");
                return 0;
            }

            using var app = new BenchmarkApplication(BenchmarkOptions.Parse(args));
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
