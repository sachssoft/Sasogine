namespace Sachssoft.Engine.Controls;

/// <summary>Identifies the time range operation performed by a timeline gesture.</summary>
public enum TimelineEditKind
{
    /// <summary>Moves the block while preserving its duration.</summary>
    Move,

    /// <summary>Moves the start edge while preserving the end time.</summary>
    ResizeStart,

    /// <summary>Moves the end edge while preserving the start time.</summary>
    ResizeEnd
}
