namespace Sachssoft.Engine.Controls;

/// <summary>Stores the visible portion of a timeline block in local control coordinates.</summary>
/// <param name="Track">The owning row.</param>
/// <param name="Item">The displayed block.</param>
/// <param name="Bounds">The clipped local block rectangle.</param>
/// <param name="HasStart">Whether the actual start edge is visible.</param>
/// <param name="HasEnd">Whether the actual end edge is visible.</param>
internal readonly record struct TimelineItemLayout(TimelineTrack Track, TimelineItem Item, Bounds2 Bounds, bool HasStart, bool HasEnd);
