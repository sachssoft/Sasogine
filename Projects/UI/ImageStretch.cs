using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>Image scaling modes.</summary>
public enum ImageStretch
{
    /// <summary>Preserves pixel size.</summary>
    None,
    /// <summary>Fills both axes independently.</summary>
    Fill,
    /// <summary>Preserves aspect ratio and fits inside the destination.</summary>
    Uniform,
    /// <summary>Preserves aspect ratio and fills the clipped destination.</summary>
    UniformToFill
}
