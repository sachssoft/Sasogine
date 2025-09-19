using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    public sealed class RenderScope : IDisposable
    {
        private bool _disposed;
        private readonly GraphicsDevice _graphicsDevice;

        // Eigene States
        private readonly RasterizerState _rasterizer;
        private readonly DepthStencilState _depthStencil;
        private readonly SamplerState _sampler;
        private readonly BlendState _blend;

        // Gesicherte States
        private readonly RasterizerState _defaultRasterizer;
        private readonly DepthStencilState _defaultDepthStencil;
        private readonly SamplerState _defaultSampler;
        private readonly BlendState _defaultBlend;

        public RenderScope(GraphicsDevice graphicsDevice, RenderOptions? options = null)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            options ??= RenderOptions.Default;

            // Standard-States merken
            _defaultRasterizer = _graphicsDevice.RasterizerState;
            _defaultDepthStencil = _graphicsDevice.DepthStencilState;
            _defaultSampler = _graphicsDevice.SamplerStates[0];
            _defaultBlend = _graphicsDevice.BlendState;

            // Eigene States erzeugen
            _rasterizer = new RasterizerState
            {
                CullMode = options.CullMode,
                FillMode = options.FillMode
            };

            _depthStencil = options.DepthEnabled ? DepthStencilState.Default : DepthStencilState.None;

            _sampler = options.SamplerState;

            _blend = options.AlphaBlend ? BlendState.AlphaBlend : BlendState.Opaque;

            // sofort aktivieren
            _graphicsDevice.RasterizerState = _rasterizer;
            _graphicsDevice.DepthStencilState = _depthStencil;
            _graphicsDevice.SamplerStates[0] = _sampler;
            _graphicsDevice.BlendState = _blend;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // Ursprungs-States wiederherstellen
            _graphicsDevice.RasterizerState = _defaultRasterizer;
            _graphicsDevice.DepthStencilState = _defaultDepthStencil;
            _graphicsDevice.SamplerStates[0] = _defaultSampler;
            _graphicsDevice.BlendState = _defaultBlend;

            // Eigene States disposen
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
