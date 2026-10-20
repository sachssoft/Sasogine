using Microsoft.Xna.Framework;

/// <summary>Provides mutable and read-only values for the inspector contract checks.</summary>
internal sealed class InspectorViewContractDefinition
{
    internal string Title { get; set; } = "Original";
    internal int Count { get; set; } = 3;
    internal int ReadOnlyCount { get; set; } = 7;
    internal Color Color { get; set; } = Color.CornflowerBlue;
    internal Vector2 Position { get; set; } = new(2, 4);
}
