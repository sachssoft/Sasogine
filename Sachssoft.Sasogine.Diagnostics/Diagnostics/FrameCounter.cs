/*
 * © 2024 Tobias Sachs
 * FrameCounter
 * 11.07.2024
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Diagnostics;

public class FrameCounter
{
    public long TotalFrames { get; private set; }
    public float TotalSeconds { get; private set; }
    public float AverageFramesPerSecond { get; private set; }
    public float CurrentFramesPerSecond { get; private set; }

    public const int MaximumSamples = 100;

    private readonly Queue<float> _sample_buffer = new();

    public void Update(float delta_time)
    {
        if (delta_time < 0f)
            throw new ArgumentOutOfRangeException(nameof(delta_time), "delta_time must be positive.");

        CurrentFramesPerSecond = 1.0f / delta_time;

        _sample_buffer.Enqueue(CurrentFramesPerSecond);

        if (_sample_buffer.Count > MaximumSamples)
        {
            _sample_buffer.Dequeue();
        }

        // Durchschnitt immer aus allen Samples berechnen
        AverageFramesPerSecond = _sample_buffer.Average();

        TotalFrames++;
        TotalSeconds += delta_time;
    }
}
