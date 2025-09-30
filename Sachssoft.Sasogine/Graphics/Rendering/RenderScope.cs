using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Surface;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    // Neues Konzept!!
    // Besser!

    /// <summary>
    /// A scope for applying rendering states temporarily.
    /// Restores previous states on Dispose().
    /// </summary>
    public sealed class RenderScope : IDisposable
    {
        private bool _disposed;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly RasterizerState _rasterizer;
        private readonly DepthStencilState _depthStencil;
        private readonly SamplerState _sampler;
        private readonly BlendState _blend;

        private readonly RasterizerState _defaultRasterizer;
        private readonly DepthStencilState _defaultDepthStencil;
        private readonly SamplerState _defaultSampler;
        private readonly BlendState _defaultBlend;

        public RenderScope(GameBaseContext context, RenderOptions? options = null)
            : this(context.GraphicsDevice, options) { }

        public RenderScope(GraphicsDevice graphicsDevice, RenderOptions? options = null)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            options ??= RenderOptions.Default;

            // Save default states
            _defaultRasterizer = _graphicsDevice.RasterizerState;
            _defaultDepthStencil = _graphicsDevice.DepthStencilState;
            _defaultSampler = _graphicsDevice.SamplerStates[0];
            _defaultBlend = _graphicsDevice.BlendState;

            // Create custom states
            _rasterizer = new RasterizerState
            {
                CullMode = options.CullMode,
                FillMode = options.FillMode
            };

            _depthStencil = CreateDepthStencilState(options.Depth);

            _sampler = options.SamplerState;
            _blend = options.AlphaBlend ? BlendState.AlphaBlend : BlendState.Opaque;

            // Apply immediately
            _graphicsDevice.RasterizerState = _rasterizer;
            _graphicsDevice.DepthStencilState = _depthStencil;
            _graphicsDevice.SamplerStates[0] = _sampler;
            _graphicsDevice.BlendState = _blend;
        }

        private DepthStencilState CreateDepthStencilState(DepthMode mode)
        {
            return mode switch
            {
                DepthMode.Disabled => DepthStencilState.None,
                DepthMode.Opaque => DepthStencilState.Default,
                DepthMode.Transparent => new DepthStencilState
                {
                    DepthBufferEnable = true,
                    DepthBufferWriteEnable = false
                },
                DepthMode.Overlay => new DepthStencilState
                {
                    DepthBufferEnable = false,
                    DepthBufferWriteEnable = false
                },
                DepthMode.DepthOnly => new DepthStencilState
                {
                    DepthBufferEnable = true,
                    DepthBufferWriteEnable = true,
                    // Optional: ColorWriteChannels = ColorWriteChannels.None
                },
                _ => DepthStencilState.None
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _graphicsDevice.RasterizerState = _defaultRasterizer;
            _graphicsDevice.DepthStencilState = _defaultDepthStencil;
            _graphicsDevice.SamplerStates[0] = _defaultSampler;
            _graphicsDevice.BlendState = _defaultBlend;

            _rasterizer?.Dispose();
            _sampler?.Dispose();
            _blend?.Dispose();

            if (_depthStencil != DepthStencilState.Default &&
                _depthStencil != DepthStencilState.None)
            {
                _depthStencil?.Dispose();
            }
        }
    }
}
