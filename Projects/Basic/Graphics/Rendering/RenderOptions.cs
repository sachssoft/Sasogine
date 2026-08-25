using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Defines rendering state options used to configure the graphics pipeline.
    /// Includes blending, depth handling, culling, fill mode, texture sampling,
    /// and optional scissor clipping.
    /// </summary>
    public class RenderOptions
    {
        /// <summary>
        /// Default rendering options for general 2D rendering.
        /// </summary>
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


        /// <summary>
        /// Rendering options for opaque geometry.
        /// </summary>
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


        /// <summary>
        /// Rendering options for transparent objects using alpha blending.
        /// </summary>
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


        /// <summary>
        /// Rendering options for scene overlays, UI elements, and parallax layers
        /// using premultiplied alpha blending.
        /// </summary>
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


        /// <summary>
        /// Rendering options for scene elements using standard straight alpha blending.
        /// </summary>
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


        /// <summary>
        /// Gets or sets the depth buffer behavior.
        /// </summary>
        public DepthMode Depth { get; set; } = DepthMode.Disabled;


        /// <summary>
        /// Gets or sets whether alpha blending is enabled.
        /// </summary>
        public bool AlphaBlend { get; set; } = true;


        /// <summary>
        /// Gets or sets whether premultiplied alpha blending is used.
        /// </summary>
        public bool PremultipliedAlpha { get; set; } = true;


        /// <summary>
        /// Gets or sets the face culling mode.
        /// </summary>
        public CullMode CullMode { get; set; } = CullMode.None;


        /// <summary>
        /// Gets or sets the polygon fill mode.
        /// </summary>
        public FillMode FillMode { get; set; } = FillMode.Solid;


        /// <summary>
        /// Gets or sets the texture sampling state.
        /// </summary>
        public SamplerState SamplerState { get; set; } = SamplerState.PointClamp;


        /// <summary>
        /// Gets or sets the optional scissor rectangle used for clipping rendering output.
        /// </summary>
        public Rectangle? ScissorRectangle { get; set; }
    }
}