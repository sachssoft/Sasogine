using System;
using System.Diagnostics;

namespace Sachssoft.Sasogine.Diagnostics;

public class BenchmarkScope : IDisposable
{
    private readonly Benchmark _benchmark;
    private readonly string _label;
    private readonly Stopwatch _stopwatch;

    public BenchmarkScope(Benchmark benchmark, string label)
    {
        _benchmark = benchmark;
        _label = label;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        _benchmark.AddScope(_label, _stopwatch.Elapsed);
    }
}