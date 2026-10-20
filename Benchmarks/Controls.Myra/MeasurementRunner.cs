using System.Diagnostics;

namespace Sachssoft.Engine.Controls.Benchmarks;
/// <summary>Warms prepared trees and measures only their draw entry points.</summary>
internal sealed class MeasurementRunner
{
    private readonly BenchmarkOptions _options;
    private readonly Action _drain;
    /// <summary>Creates a sequential renderer measurement runner.</summary>
    public MeasurementRunner(BenchmarkOptions options, Action drain) { _options = options; _drain = drain; }
    /// <summary>Gets raw batches in execution order.</summary>
    public List<Measurement> Results { get; } = [];
    /// <summary>Alternates AB and BA order and yields between batches for the progress display.</summary>
    public IEnumerable<string> Run(TreeSpec spec, Func<IBenchmarkTree> createSasogine, Func<IBenchmarkTree> createMyra, Action<IBenchmarkTree> snapshot)
    {
        yield return $"Preparing {spec.Count} buttons: layout and geometry validation";
        using var s = createSasogine();
        using var m = createMyra();
        s.Prepare(); m.Prepare(); s.Verify(); m.Verify();

        foreach (var tree in new[] { s, m })
        {
            yield return $"Warming {tree.Library}: {spec.Count} buttons";
            for (int i = 0; i < _options.Warmup; i++) { _drain(); tree.Draw(); _drain(); }
        }
        snapshot(s); snapshot(m);
        for (int round = 0; round < _options.Rounds; round++)
        {
            foreach (var tree in (round & 1) == 0 ? new[] { s, m } : new[] { m, s })
            {
                long ticks = 0;
                for (int frame = 0; frame < _options.Frames; frame++)
                {
                    _drain();
                    long start = Stopwatch.GetTimestamp();
                    tree.Draw();
                    ticks += Stopwatch.GetTimestamp() - start;
                    _drain();
                }
                var sample = new Measurement(tree.Library, spec.Count, round + 1, _options.Frames, ticks * 1000d / Stopwatch.Frequency / _options.Frames);
                Results.Add(sample);
                string status = $"{spec.Count} buttons | round {round + 1}/{_options.Rounds} | {tree.Library}: {sample.Milliseconds:F3} ms";
                Console.WriteLine(status);
                yield return status;
            }
        }
        s.Verify(); m.Verify();
    }
}

