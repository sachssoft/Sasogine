using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Rendering options that define how an object should be drawn.
    /// </summary>
    public class RenderOptions
    {
        // --- Presets ---
        public static readonly RenderOptions Default = new RenderOptions();

        public static readonly RenderOptions Opaque = new RenderOptions
        {
            AlphaBlend = false,
            Depth = DepthMode.Opaque,
            CullMode = CullMode.CullCounterClockwiseFace,
            FillMode = FillMode.Solid,
            SamplerState = SamplerState.LinearClamp
        };

        public static readonly RenderOptions AlphaBlended = new RenderOptions
        {
            AlphaBlend = true,
            Depth = DepthMode.Transparent,
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            SamplerState = SamplerState.LinearClamp
        };

        public static readonly RenderOptions Overlay = new RenderOptions
        {
            AlphaBlend = true,
            Depth = DepthMode.Overlay,
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            SamplerState = SamplerState.PointClamp
        };

        public static readonly RenderOptions DepthOnly = new RenderOptions
        {
            AlphaBlend = false,
            Depth = DepthMode.DepthOnly,
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            SamplerState = SamplerState.PointClamp
        };

        // --- Properties ---
        /// <summary>
        /// Depth mode controlling test and write behavior.
        /// </summary>
        public DepthMode Depth { get; set; } = DepthMode.Disabled;

        /// <summary>
        /// Whether to enable alpha blending.
        /// </summary>
        public bool AlphaBlend { get; set; } = true;

        /// <summary>
        /// Cull mode for rasterization.
        /// </summary>
        public CullMode CullMode { get; set; } = CullMode.None;

        /// <summary>
        /// Fill mode for rasterization (Solid or Wireframe).
        /// </summary>
        public FillMode FillMode { get; set; } = FillMode.Solid;

        /// <summary>
        /// Sampler state for texture sampling.
        /// </summary>
        public SamplerState SamplerState { get; set; } = SamplerState.PointClamp;

        /// <summary>
        /// Texture wrap mode (optional, can be used in shader).
        /// </summary>
        public TextureAddressMode TextureWrap { get; set; } = TextureAddressMode.Wrap;
    }
}
