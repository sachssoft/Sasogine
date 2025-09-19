using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    public class RenderOptions
    {
        // --- Presets ---
        public static readonly RenderOptions Default = new RenderOptions();

        public static readonly RenderOptions LowQuality = new RenderOptions
        {
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            DepthEnabled = false,
            SamplerState = SamplerState.PointClamp,
            AlphaBlend = true
        };

        public static readonly RenderOptions HighQuality = new RenderOptions
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            FillMode = FillMode.Solid,
            DepthEnabled = true,
            SamplerState = SamplerState.AnisotropicClamp,
            AlphaBlend = true
        };

        public static readonly RenderOptions Wireframe = new RenderOptions
        {
            CullMode = CullMode.None,
            FillMode = FillMode.WireFrame,
            DepthEnabled = false,
            SamplerState = SamplerState.PointClamp,
            AlphaBlend = false
        };

        public static readonly RenderOptions Opaque = new RenderOptions
        {
            AlphaBlend = false,
            DepthEnabled = true,
            SamplerState = SamplerState.LinearClamp
        };

        public static readonly RenderOptions AlphaBlended = new RenderOptions
        {
            AlphaBlend = true,
            DepthEnabled = false,
            SamplerState = SamplerState.LinearClamp
        };

        public static readonly RenderOptions DepthTestOnly = new RenderOptions
        {
            AlphaBlend = false,
            DepthEnabled = true,
            CullMode = CullMode.CullCounterClockwiseFace,
            SamplerState = SamplerState.PointWrap
        };

        // --- Properties ---
        public CullMode CullMode { get; set; } = CullMode.None;
        public FillMode FillMode { get; set; } = FillMode.Solid;
        public bool DepthEnabled { get; set; } = false;
        public TextureAddressMode TextureWrap { get; set; } = TextureAddressMode.Wrap;
        public SamplerState SamplerState { get; set; } = SamplerState.PointClamp;
        public bool AlphaBlend { get; set; } = true;
    }
}
