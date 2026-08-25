using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sachssoft.Sasogine.Diagnostics;

public class Benchmark
{
    private readonly Dictionary<string, TimeSpan> _scoped_measurements = new();
    private readonly Stopwatch _stopwatch;
    private readonly Dictionary<string, TimeSpan> _measurements;
    private string? _current_label;

    public Benchmark()
    {
        _stopwatch = new Stopwatch();
        _measurements = new Dictionary<string, TimeSpan>();
    }

    public void Start(string label)
    {
        if (_stopwatch.IsRunning)
            throw new InvalidOperationException("Benchmark already running. Call Stop() first.");

        _current_label = label;
        _stopwatch.Restart();
    }

    public void Stop()
    {
        if (!_stopwatch.IsRunning)
            throw new InvalidOperationException("Benchmark not running. Call Start(label) first.");

        _stopwatch.Stop();
        _measurements[_current_label!] = _stopwatch.Elapsed;
    }

    public void Clear()
    {
        _measurements.Clear();
    }

    public TimeSpan GetTime(string label)
    {
        return _measurements.TryGetValue(label, out var time) ? time : TimeSpan.Zero;
    }

    public string GetSummary()
    {
        var sb = new StringBuilder();
        foreach (var entry in _measurements)
            sb.AppendLine($"{entry.Key}: {entry.Value.TotalMilliseconds:F3} ms");
        return sb.ToString();
    }

    internal void AddScope(string label, TimeSpan duration)
    {
        _scoped_measurements[label] = duration;
    }

    public TimeSpan GetScopedMeasurement(string label)
    {
        if (_scoped_measurements.TryGetValue(label, out var result))
            return result;

        return TimeSpan.Zero;
    }

    public IEnumerable<(string Label, TimeSpan Duration)> GetScopedMeasurements()
        => _scoped_measurements.Select(x => (Label: x.Key, Duration: x.Value));
}