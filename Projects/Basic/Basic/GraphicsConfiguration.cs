using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine;

/// <summary>Provides initial graphics settings. Unspecified values use backend defaults.</summary>
public sealed class GraphicsConfiguration
{
    /// <summary>Gets the preferred back-buffer width in pixels.</summary>
    public int? Width { get; init; }
    /// <summary>Gets the preferred back-buffer height in pixels.</summary>
    public int? Height { get; init; }
    /// <summary>Gets whether unspecified dimensions use the current display resolution.</summary>
    public bool UseDisplayResolution { get; init; }
    /// <summary>Gets whether to begin in fullscreen mode.</summary>
    public bool? FullScreen { get; init; }
    /// <summary>Gets whether fullscreen may change the hardware display mode.</summary>
    public bool? HardwareModeSwitch { get; init; }
    /// <summary>Gets whether to request multisampling.</summary>
    public bool? MultiSampling { get; init; }
    /// <summary>Gets the required graphics profile.</summary>
    public GraphicsProfile? Profile { get; init; }
    /// <summary>Gets the depth and stencil buffer format.</summary>
    public DepthFormat? DepthStencilFormat { get; init; }
    /// <summary>Gets whether presentation synchronizes with the display refresh.</summary>
    public bool? SynchronizeWithVerticalRetrace { get; init; }
    /// <summary>Gets the supported display orientations.</summary>
    public DisplayOrientation? SupportedOrientations { get; init; }
    /// <summary>Gets whether to use XNA-compatible half-pixel positioning.</summary>
    public bool? PreferHalfPixelOffset { get; init; }
}
