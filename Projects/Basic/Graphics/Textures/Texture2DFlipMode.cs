using System;
using Sachssoft.Engine.Components.Definitions;

namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Specifies how a two-dimensional texture is flipped during rendering.
/// </summary>
[Flags]
public enum Texture2DFlipMode : int
{
    /// <summary>
    /// Specifies that the texture is not flipped.
    /// </summary>
    [DefinitionMember(
        Title = "None",
        Description = "Does not flip the texture.")]
    None = 0,

    /// <summary>
    /// Flips the texture horizontally.
    /// </summary>
    [DefinitionMember(
        Title = "Horizontal",
        Description = "Flips the texture horizontally.")]
    Horizontal = 1,

    /// <summary>
    /// Flips the texture vertically.
    /// </summary>
    [DefinitionMember(
        Title = "Vertical",
        Description = "Flips the texture vertically.")]
    Vertical = 2
}