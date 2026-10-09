using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Graphics.Rendering
{
    /// <summary>
    /// Temporarily applies rendering states to a graphics device and restores
    /// the previous states when disposed.
    /// </summary>
    /// <remarks>
    /// Supports blending, depth handling, rasterization, texture sampling,
    /// and optional scissor clipping.
    /// </remarks>
    public sealed class RenderScope : IDisposable
    {
        private static readonly Dictionary<RenderOptions, RasterizerState> _rasterizerCache = new();
        private static readonly Dictionary<DepthMode, DepthStencilState> _depthCache = new();

        private readonly GraphicsDevice _graphicsDevice;

        private readonly RasterizerState _customRasterizer;
        private readonly DepthStencilState _customDepthStencil;
        private readonly SamplerState _customSampler;
        private readonly BlendState _customBlend;

        private readonly RasterizerState _prevRasterizer;
        private readonly DepthStencilState _prevDepthStencil;
        private readonly SamplerState _prevSampler;
        private readonly BlendState _prevBlend;

        private bool _disposed;

        /// <summary>
        /// Initializes a rendering scope using the specified game context.
        /// </summary>
        /// <param name="context">The game context containing the graphics device.</param>
        /// <param name="options">The rendering options to apply, or the default options.</param>
        public RenderScope(GameContext context, RenderOptions? options = null)
            : this(context.GraphicsDevice, options)
        {
        }

        /// <summary>
        /// Initializes a rendering scope using the specified graphics device.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to configure.</param>
        /// <param name="options">The rendering options to apply, or the default options.</param>
        public RenderScope(GraphicsDevice graphicsDevice, RenderOptions? options = null)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            options ??= RenderOptions.Default;

            // Save previous states
            _prevRasterizer = _graphicsDevice.RasterizerState;
            _prevDepthStencil = _graphicsDevice.DepthStencilState;
            _prevSampler = _graphicsDevice.SamplerStates[0];
            _prevBlend = _graphicsDevice.BlendState;

            // Rasterizer
            if (options.ScissorRectangle.HasValue)
            {
                _customRasterizer = new RasterizerState
                {
                    CullMode = options.CullMode,
                    FillMode = options.FillMode,
                    ScissorTestEnable = true
                };

                _graphicsDevice.ScissorRectangle = options.ScissorRectangle.Value;
            }
            else
            {
                _customRasterizer = GetRasterizer(options);
            }

            _graphicsDevice.RasterizerState = _customRasterizer;

            // Depth stencil
            _customDepthStencil = GetDepthStencil(options.Depth);
            _graphicsDevice.DepthStencilState = _customDepthStencil;

            // Texture sampling
            _customSampler = options.SamplerState;
            _graphicsDevice.SamplerStates[0] = _customSampler;

            // Alpha blending
            _customBlend = !options.AlphaBlend
                ? BlendState.Opaque
                : options.PremultipliedAlpha
                    ? BlendState.AlphaBlend
                    : BlendState.NonPremultiplied;

            _graphicsDevice.BlendState = _customBlend;
        }

        private static RasterizerState GetRasterizer(RenderOptions options)
        {
            if (!_rasterizerCache.TryGetValue(options, out var state)
                || state.IsDisposed)
            {
                state = new RasterizerState
                {
                    CullMode = options.CullMode,
                    FillMode = options.FillMode
                };

                _rasterizerCache[options] = state;
            }

            return state;
        }

        private static DepthStencilState GetDepthStencil(DepthMode mode)
        {
            if (!_depthCache.TryGetValue(mode, out var state)
                || state.IsDisposed)
            {
                state = mode switch
                {
                    DepthMode.Disabled => DepthStencilState.None,
                    DepthMode.Opaque => DepthStencilState.Default,
                    DepthMode.Transparent => new DepthStencilState
                    {
                        DepthBufferEnable = false,
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
                        DepthBufferWriteEnable = true
                    },
                    _ => DepthStencilState.None
                };

                _depthCache[mode] = state;
            }

            return state;
        }

        /// <summary>
        /// Restores the rendering states that were active before the scope was created.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _graphicsDevice.RasterizerState = _prevRasterizer;
            _graphicsDevice.DepthStencilState = _prevDepthStencil;
            _graphicsDevice.SamplerStates[0] = _prevSampler;
            _graphicsDevice.BlendState = _prevBlend;
        }
    }
}