using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Temporärer Rendering-Scope: wendet RenderStates an und stellt alte States wieder her.
    /// Unterstützt optional Scissor-Rectangle.
    /// Optimiert für 2D/Parallax-Szenen.
    /// </summary>
    public sealed class RenderScope : IDisposable
    {
        private bool _disposed;
        private readonly GraphicsDevice _graphicsDevice;

        private readonly RasterizerState _customRasterizer;
        private readonly DepthStencilState _customDepthStencil;
        private readonly SamplerState _customSampler;
        private readonly BlendState _customBlend;

        private readonly RasterizerState _prevRasterizer;
        private readonly DepthStencilState _prevDepthStencil;
        private readonly SamplerState _prevSampler;
        private readonly BlendState _prevBlend;

        private static readonly Dictionary<RenderOptions, RasterizerState> _rasterizerCache = new();
        private static readonly Dictionary<DepthMode, DepthStencilState> _depthCache = new();

        /// <summary>
        /// Erstellt einen RenderScope für GameBaseContext.
        /// </summary>
        public RenderScope(GameContext context, RenderOptions? options = null)
            : this(context.GraphicsDevice, options)
        {
        }

        /// <summary>
        /// Erstellt einen RenderScope für ein GraphicsDevice.
        /// </summary>
        public RenderScope(GraphicsDevice graphicsDevice, RenderOptions? options = null)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            options ??= RenderOptions.Default;

            // Alte States speichern
            _prevRasterizer = _graphicsDevice.RasterizerState;
            _prevDepthStencil = _graphicsDevice.DepthStencilState;
            _prevSampler = _graphicsDevice.SamplerStates[0];
            _prevBlend = _graphicsDevice.BlendState;

            // RasterizerState vorbereiten (mit Scissor optional)
            if (options.ScissorRectangle.HasValue)
            {
                // Neues RasterizerState erzeugen, basierend auf den Optionen
                _customRasterizer = new RasterizerState
                {
                    CullMode = options.CullMode,
                    FillMode = options.FillMode,
                    ScissorTestEnable = true
                };

                // Scissor-Rectangle setzen
                _graphicsDevice.ScissorRectangle = options.ScissorRectangle.Value;
            }
            else
            {
                _customRasterizer = GetRasterizer(options);
            }

            // RasterizerState anwenden
            _graphicsDevice.RasterizerState = _customRasterizer;

            // DepthStencil
            _customDepthStencil = GetDepthStencil(options.Depth);
            _graphicsDevice.DepthStencilState = _customDepthStencil;

            // Sampler
            _customSampler = options.SamplerState;
            _graphicsDevice.SamplerStates[0] = _customSampler;

            // Blend
            _customBlend = options.AlphaBlend
                ? new BlendState
                {
                    ColorSourceBlend = Blend.One,
                    ColorDestinationBlend = Blend.InverseSourceAlpha,
                    AlphaSourceBlend = Blend.One,
                    AlphaDestinationBlend = Blend.InverseSourceAlpha
                }
                : BlendState.Opaque;
            _graphicsDevice.BlendState = _customBlend;
        }

        private static RasterizerState GetRasterizer(RenderOptions options)
        {
            if (!_rasterizerCache.TryGetValue(options, out var state))
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
            if (!_depthCache.TryGetValue(mode, out var state))
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
        /// Setzt die alten GraphicsDevice States zurück.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _graphicsDevice.RasterizerState = _prevRasterizer;
            _graphicsDevice.DepthStencilState = _prevDepthStencil;
            _graphicsDevice.SamplerStates[0] = _prevSampler;
            _graphicsDevice.BlendState = _prevBlend;
        }
    }
}
