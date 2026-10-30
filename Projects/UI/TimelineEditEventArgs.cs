using System;

namespace Sachssoft.Engine.Controls;

/// <summary>Captures a block edit for synchronization, commands, and caller-managed undo history.</summary>
public sealed class TimelineEditEventArgs : EventArgs
{
    /// <summary>Creates an immutable snapshot of the time range operation.</summary>
    /// <param name="track">The row containing the edited block.</param>
    /// <param name="item">The edited block.</param>
    /// <param name="kind">The operation performed.</param>
    /// <param name="originalStart">The start time before the gesture.</param>
    /// <param name="originalDuration">The duration before the gesture.</param>
    /// <param name="start">The start time at notification.</param>
    /// <param name="duration">The duration at notification.</param>
    internal TimelineEditEventArgs(TimelineTrack track, TimelineItem item, TimelineEditKind kind,
        double originalStart, double originalDuration, double start, double duration)
    {
        Track = track;
        Item = item;
        Kind = kind;
        OriginalStart = originalStart;
        OriginalDuration = originalDuration;
        Start = start;
        Duration = duration;
    }

    /// <summary>Gets the duration captured at notification.</summary>
    public double Duration { get; }

    /// <summary>Gets the edited block.</summary>
    public TimelineItem Item { get; }

    /// <summary>Gets the editing operation.</summary>
    public TimelineEditKind Kind { get; }

    /// <summary>Gets the duration before the gesture started.</summary>
    public double OriginalDuration { get; }

    /// <summary>Gets the start time before the gesture started.</summary>
    public double OriginalStart { get; }

    /// <summary>Gets the start time captured at notification.</summary>
    public double Start { get; }

    /// <summary>Gets the row containing the block at the start of the edit.</summary>
    public TimelineTrack Track { get; }
}
