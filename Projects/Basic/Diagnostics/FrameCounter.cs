using System;

namespace Sachssoft.Sasogine.Diagnostics;

/// <summary>
/// Tracks frame rate statistics using a fixed-size rolling frame-time window
/// combined with exponential smoothing.
/// </summary>
public sealed class FrameCounter
{
    /// <summary>
    /// Specifies the maximum number of frame samples used for the rolling
    /// frame rate calculation.
    /// </summary>
    public const int MaximumSamples = 100;

    private readonly float[] _samples;
    private readonly float _smoothing;

    private int _sampleIndex;
    private int _sampleCount;

    private float _sampleTime;
    private float _currentFramesPerSecond;
    private float _smoothedFramesPerSecond;

    private long _totalFrames;
    private double _totalSeconds;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameCounter"/> class.
    /// </summary>
    /// <param name="smoothing">
    /// The exponential smoothing factor used for the smoothed frame rate.
    /// A higher value reacts faster to frame rate changes.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="smoothing"/> is not greater than zero
    /// and less than or equal to one.
    /// </exception>
    public FrameCounter(float smoothing = 0.1f)
    {
        if (smoothing <= 0f || smoothing > 1f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(smoothing),
                "Smoothing must be greater than zero and less than or equal to one.");
        }

        _samples = new float[MaximumSamples];
        _smoothing = smoothing;
    }

    /// <summary>
    /// Gets the total number of frames recorded since the last reset.
    /// </summary>
    public long TotalFrames => _totalFrames;

    /// <summary>
    /// Gets the total elapsed time in seconds since the last reset.
    /// </summary>
    public double TotalSeconds => _totalSeconds;

    /// <summary>
    /// Gets the instantaneous frame rate of the most recently recorded frame.
    /// </summary>
    public float CurrentFramesPerSecond => _currentFramesPerSecond;

    /// <summary>
    /// Gets the rolling average frame rate calculated from recent frame times.
    /// </summary>
    public float AverageFramesPerSecond =>
        _sampleTime > 0f
            ? _sampleCount / _sampleTime
            : 0f;

    /// <summary>
    /// Gets the exponentially smoothed frame rate.
    /// </summary>
    public float SmoothedFramesPerSecond => _smoothedFramesPerSecond;

    /// <summary>
    /// Gets the average frame rate over the entire measurement period.
    /// </summary>
    public double OverallFramesPerSecond =>
        _totalSeconds > 0.0
            ? _totalFrames / _totalSeconds
            : 0.0;

    /// <summary>
    /// Updates the frame rate statistics using the specified frame duration.
    /// </summary>
    /// <param name="deltaTime">
    /// The elapsed frame time, in seconds.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="deltaTime"/> is negative.
    /// </exception>
    public void Update(float deltaTime)
    {
        if (deltaTime < 0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(deltaTime),
                "Delta time cannot be negative.");
        }

        if (deltaTime == 0f)
            return;

        _currentFramesPerSecond = 1f / deltaTime;

        if (_sampleCount == MaximumSamples)
        {
            _sampleTime -= _samples[_sampleIndex];
        }
        else
        {
            _sampleCount++;
        }

        _samples[_sampleIndex] = deltaTime;
        _sampleTime += deltaTime;

        _sampleIndex++;

        if (_sampleIndex == MaximumSamples)
            _sampleIndex = 0;

        var averageFramesPerSecond = _sampleCount / _sampleTime;

        if (_totalFrames == 0)
        {
            _smoothedFramesPerSecond = averageFramesPerSecond;
        }
        else
        {
            _smoothedFramesPerSecond +=
                (averageFramesPerSecond - _smoothedFramesPerSecond) *
                _smoothing;
        }

        _totalFrames++;
        _totalSeconds += deltaTime;
    }

    /// <summary>
    /// Resets all frame rate statistics and recorded samples.
    /// </summary>
    public void Reset()
    {
        _sampleIndex = 0;
        _sampleCount = 0;
        _sampleTime = 0f;

        _currentFramesPerSecond = 0f;
        _smoothedFramesPerSecond = 0f;

        _totalFrames = 0;
        _totalSeconds = 0.0;
    }
}