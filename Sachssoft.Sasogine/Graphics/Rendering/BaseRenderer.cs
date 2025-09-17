using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    public abstract class BaseRenderer : IDisposable
    {
        private bool _disposed;
        protected readonly GraphicsDevice GraphicsDevice;

        // Eigene, sofort nutzbare States
        protected readonly RasterizerState RasterizerState;
        protected readonly DepthStencilState DepthStencilState;
        protected readonly SamplerState SamplerState;
        protected readonly BlendState BlendState;

        // Ursprungszustände zum Wiederherstellen
        private readonly RasterizerState _defaultRasterizer;
        private readonly DepthStencilState _defaultDepthStencil;
        private readonly SamplerState _defaultSampler;
        private readonly BlendState _defaultBlend;

        protected BaseRenderer(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

            // Standard-States merken
            _defaultRasterizer = GraphicsDevice.RasterizerState;
            _defaultDepthStencil = GraphicsDevice.DepthStencilState;
            _defaultSampler = GraphicsDevice.SamplerStates[0];
            _defaultBlend = GraphicsDevice.BlendState;

            // Eigene States erzeugen
            RasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid
            };
            DepthStencilState = DepthStencilState.Default;
            SamplerState = SamplerState.PointWrap;
            BlendState = BlendState.AlphaBlend;

            ApplyStates();
            OnInitialize();
        }

        private void ApplyStates()
        {
            GraphicsDevice.RasterizerState = RasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState;
            GraphicsDevice.SamplerStates[0] = SamplerState;
            GraphicsDevice.BlendState = BlendState;
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnRenderCompleted() { }
        protected virtual void OnUninitialize() { }

        public virtual void Flush() { }

        /// <summary>
        /// Draw-Methode, die den Effekt aus dem GameContext bezieht
        /// </summary>
        public void Draw(GameContext context, Matrix? transform = null, CameraBase? customCamera = null, IEffectAdapter? customEffect = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            DrawInternal(context, transform, customCamera, customEffect);
        }

        /// <summary>
        /// Abgeleitete Klassen implementieren hier das eigentliche Zeichnen
        /// </summary>
        protected abstract void DrawInternal(GameContext context, Matrix? transform = null, CameraBase? customCamera = null, IEffectAdapter? customEffect = null);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            OnRenderCompleted();

            // Standard-States wiederherstellen
            GraphicsDevice.RasterizerState = _defaultRasterizer;
            GraphicsDevice.DepthStencilState = _defaultDepthStencil;
            GraphicsDevice.SamplerStates[0] = _defaultSampler;
            GraphicsDevice.BlendState = _defaultBlend;

            OnUninitialize();

            // Eigene States disposen
            RasterizerState?.Dispose();
            SamplerState?.Dispose();
            BlendState?.Dispose();

            if (DepthStencilState != DepthStencilState.Default)
                DepthStencilState?.Dispose();
        }
    }
}
