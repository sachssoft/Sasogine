namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Starts the benchmark without requiring an installed system-wide profiler.</summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Contains("--help"))
        {
            Console.WriteLine("Controls.Myra [--quick] [--exit] [--font file.ttf] [--output directory] [--rounds 3..31] [--frames 2..10000] [--warmup 2..10000]");
            return 0;
        }

        try
        {
            using var application = new BenchmarkApplication(BenchmarkOptions.Parse(args));
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
