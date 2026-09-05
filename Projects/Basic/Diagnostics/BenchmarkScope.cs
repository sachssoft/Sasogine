using System;
using System.Diagnostics;

namespace Sachssoft.Sasogine.Diagnostics;

/// <summary>
/// Measures the execution time of a scope and records the result
/// in a <see cref="Benchmark"/> instance.
/// </summary>
public sealed class BenchmarkScope : IDisposable
{
    private readonly Benchmark _benchmark;
    private readonly string _label;
    private readonly Stopwatch _stopwatch;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkScope"/> class
    /// and starts measuring the elapsed time.
    /// </summary>
    /// <param name="benchmark">
    /// The benchmark used to store the measurement.
    /// </param>
    /// <param name="label">
    /// The label used to identify the measurement.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="benchmark"/> or <paramref name="label"/>
    /// is <see langword="null"/>.
    /// </exception>
    public BenchmarkScope(Benchmark benchmark, string label)
    {
        ArgumentNullException.ThrowIfNull(benchmark);
        ArgumentNullException.ThrowIfNull(label);

        _benchmark = benchmark;
        _label = label;
        _stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Stops the measurement and records the elapsed time.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _stopwatch.Stop();
        _benchmark.AddScope(_label, _stopwatch.Elapsed);

        _disposed = true;
    }
}