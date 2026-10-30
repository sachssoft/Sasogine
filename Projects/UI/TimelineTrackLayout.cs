namespace Sachssoft.Engine.Controls;

/// <summary>Stores a visible row and its local header rectangle.</summary>
/// <param name="Track">The displayed row.</param>
/// <param name="Index">The logical row index.</param>
/// <param name="Bounds">The local row header rectangle.</param>
internal readonly record struct TimelineTrackLayout(TimelineTrack Track, int Index, Bounds2 Bounds);
