using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    public class RenderOptions
    {
        // --- Default ---
        public static readonly RenderOptions Default = new RenderOptions
        {
            AlphaBlend = true,
            Depth = DepthMode.Disabled,
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            SamplerState = SamplerState.PointClamp,
            PremultipliedAlpha = true,
            ScissorRectangle = null
        };

        // --- Opaque ---
        public static readonly RenderOptions Opaque = new RenderOptions
        {
            AlphaBlend = false,
            Depth = DepthMode.Opaque,
            CullMode = CullMode.CullCounterClockwiseFace,
            FillMode = FillMode.Solid,
            SamplerState = SamplerState.LinearClamp,
            PremultipliedAlpha = true,
            ScissorRectangle = null
        };

        // --- Transparent ---
        public static readonly RenderOptions AlphaBlended = new RenderOptions
        {
            AlphaBlend = true,
            Depth = DepthMode.Transparent,
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            SamplerState = SamplerState.LinearClamp,
            PremultipliedAlpha = true,
            ScissorRectangle = null
        };

        // --- Overlay / UI / Parallax (Premultiplied) ---
        public static readonly RenderOptions ScenePremultiplied = new RenderOptions
        {
            AlphaBlend = true,
            Depth = DepthMode.Disabled,
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            SamplerState = SamplerState.PointClamp,
            PremultipliedAlpha = true,
            ScissorRectangle = null
        };

        // --- Straight Alpha ---
        public static readonly RenderOptions SceneStraightAlpha = new RenderOptions
        {
            AlphaBlend = true,
            Depth = DepthMode.Disabled,
            CullMode = CullMode.None,
            FillMode = FillMode.Solid,
            SamplerState = SamplerState.PointClamp,
            PremultipliedAlpha = false,
            ScissorRectangle = null
        };

        // --- Properties ---
        public DepthMode Depth { get; set; } = DepthMode.Disabled;
        public bool AlphaBlend { get; set; } = true;
        public bool PremultipliedAlpha { get; set; } = true;
        public CullMode CullMode { get; set; } = CullMode.None;
        public FillMode FillMode { get; set; } = FillMode.Solid;
        public SamplerState SamplerState { get; set; } = SamplerState.PointClamp;

        /// <summary>
        /// Optionales ScissorRectangle für das Rendern.
        /// Wird in RenderScope aktiviert, falls gesetzt.
        /// </summary>
        public Rectangle? ScissorRectangle { get; set; } = null;
    }
}
