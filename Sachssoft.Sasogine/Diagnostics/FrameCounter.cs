using System;

namespace Sachssoft.Sasogine.Diagnostics;

/// <summary>
/// High-Performance Hybrid FrameCounter:
/// - Ringpuffer für schnelle Reaktion auf plötzliche FPS-Drops
/// - EMA für glatte Durchschnittswerte
/// - FastWeight für sofortige FPS-Reaktion
/// - GC-frei nach Initialisierung
/// </summary>
public class FrameCounter
{
    private readonly float[] _samples;
    private int _index;
    private int _count;
    private float _sum;

    private readonly float _smoothing;  // EMA-Faktor
    private readonly float _fastWeight; // Gewicht für aktuelle FPS

    private long _totalFrames;
    private float _totalSeconds;

    private float _currentFps;
    private float _averageFps;

    public const int MaximumSamples = 100;

    public FrameCounter(float smoothing = 0.1f, float fastWeight = 0.2f)
    {
        if (smoothing <= 0f || smoothing > 1f)
            throw new ArgumentOutOfRangeException(nameof(smoothing), "Smoothing must be in (0,1].");
        if (fastWeight < 0f || fastWeight > 1f)
            throw new ArgumentOutOfRangeException(nameof(fastWeight), "FastWeight must be in [0,1].");

        _samples = new float[MaximumSamples];
        _smoothing = smoothing;
        _fastWeight = fastWeight;

        Reset();
    }

    public long TotalFrames => _totalFrames;
    public float TotalSeconds => _totalSeconds;
    public float CurrentFramesPerSecond => _currentFps;
    public float AverageFramesPerSecond => _averageFps;

    public void Update(float deltaTime)
    {
        if (deltaTime <= 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTime), "deltaTime must be positive and non-zero.");

        _currentFps = 1f / deltaTime;

        // --- Ringpuffer für schnelle Durchschnitts-Reaktion ---
        if (_count < MaximumSamples)
        {
            _samples[_index] = _currentFps;
            _sum += _currentFps;
            _count++;
        }
        else
        {
            _sum -= _samples[_index];
            _samples[_index] = _currentFps;
            _sum += _currentFps;
        }

        _index = (_index + 1) % MaximumSamples;
        float ringAverage = _sum / _count;

        // --- EMA + FastWeight für stabilen, aber reaktiven Durchschnitt ---
        if (_totalFrames == 0)
        {
            _averageFps = ringAverage;
        }
        else
        {
            float ema = _averageFps + _smoothing * (ringAverage - _averageFps);
            _averageFps = (1f - _fastWeight) * ema + _fastWeight * _currentFps;
        }

        _totalFrames++;
        _totalSeconds += deltaTime;
    }

    public void Reset()
    {
        _index = 0;
        _count = 0;
        _sum = 0f;
        _totalFrames = 0;
        _totalSeconds = 0f;
        _currentFps = 0f;
        _averageFps = 0f;
        Array.Clear(_samples, 0, _samples.Length);
    }
}