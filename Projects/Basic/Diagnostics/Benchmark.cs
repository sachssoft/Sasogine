using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sachssoft.Sasogine.Diagnostics;

/// <summary>
/// Provides functionality for measuring and tracking execution times
/// of named operations and scopes.
/// </summary>
public class Benchmark
{
    private readonly Dictionary<string, TimeSpan> _scopedMeasurements = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly Dictionary<string, TimeSpan> _measurements = new();

    private string? _currentLabel;

    /// <summary>
    /// Starts measuring the execution time for the specified label.
    /// </summary>
    /// <param name="label">The label used to identify the measurement.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="label"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a measurement is already running.
    /// </exception>
    public void Start(string label)
    {
        ArgumentNullException.ThrowIfNull(label);

        if (_stopwatch.IsRunning)
            throw new InvalidOperationException(
                "Benchmark already running. Call Stop() first.");

        _currentLabel = label;
        _stopwatch.Restart();
    }

    /// <summary>
    /// Stops the current measurement and stores its elapsed time.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no measurement is currently running.
    /// </exception>
    public void Stop()
    {
        if (!_stopwatch.IsRunning)
            throw new InvalidOperationException(
                "Benchmark not running. Call Start(label) first.");

        _stopwatch.Stop();

        _measurements[_currentLabel!] = _stopwatch.Elapsed;
        _currentLabel = null;
    }

    /// <summary>
    /// Clears all stored measurements.
    /// </summary>
    public void Clear()
    {
        _measurements.Clear();
        _scopedMeasurements.Clear();
    }

    /// <summary>
    /// Gets the stored execution time for the specified label.
    /// </summary>
    /// <param name="label">The label of the measurement.</param>
    /// <returns>
    /// The measured duration, or <see cref="TimeSpan.Zero"/> if no measurement
    /// exists for the specified label.
    /// </returns>
    public TimeSpan GetTime(string label)
    {
        return _measurements.TryGetValue(label, out var time)
            ? time
            : TimeSpan.Zero;
    }

    /// <summary>
    /// Creates a textual summary of all stored measurements.
    /// </summary>
    /// <returns>
    /// A formatted summary containing each measurement and its duration
    /// in milliseconds.
    /// </returns>
    public string GetSummary()
    {
        var builder = new StringBuilder();

        foreach (var entry in _measurements)
        {
            builder.AppendLine(
                $"{entry.Key}: {entry.Value.TotalMilliseconds:F3} ms");
        }

        return builder.ToString();
    }

    /// <summary>
    /// Adds or replaces a scoped measurement.
    /// </summary>
    /// <param name="label">The label used to identify the scoped measurement.</param>
    /// <param name="duration">The measured duration.</param>
    internal void AddScope(string label, TimeSpan duration)
    {
        _scopedMeasurements[label] = duration;
    }

    /// <summary>
    /// Gets the stored duration of the specified scoped measurement.
    /// </summary>
    /// <param name="label">The label of the scoped measurement.</param>
    /// <returns>
    /// The measured duration, or <see cref="TimeSpan.Zero"/> if no scoped
    /// measurement exists for the specified label.
    /// </returns>
    public TimeSpan GetScopedMeasurement(string label)
    {
        return _scopedMeasurements.TryGetValue(label, out var result)
            ? result
            : TimeSpan.Zero;
    }

    /// <summary>
    /// Gets all stored scoped measurements.
    /// </summary>
    /// <returns>
    /// A sequence containing the label and duration of each scoped measurement.
    /// </returns>
    public IEnumerable<(string Label, TimeSpan Duration)> GetScopedMeasurements()
    {
        return _scopedMeasurements.Select(
            entry => (entry.Key, entry.Value));
    }
}