namespace Sachssoft.Engine.Controls;

/// <summary>Stores a cached ruler label and its local horizontal position.</summary>
/// <param name="X">The local horizontal position.</param>
/// <param name="Label">The prepared invariant-culture time label.</param>
internal readonly record struct TimelineTick(float X, string Label);
