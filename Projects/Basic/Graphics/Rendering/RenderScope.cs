using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Sachssoft.Engine.Graphics.Cameras;
using Sachssoft.Engine.Graphics.Meshes;
using Sachssoft.Engine.Scenes;

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
        private static readonly ConditionalWeakTable<GraphicsDevice, Dictionary<(CullMode CullMode, FillMode FillMode, bool Scissor), RasterizerState>> _rasterizerCache = new();
        private static readonly Dictionary<DepthMode, DepthStencilState> _depthCache = new();

        private readonly GraphicsDevice _graphicsDevice;
        private readonly SceneDrawContext? _sceneContext;

        private readonly RasterizerState _prevRasterizer;
        private readonly DepthStencilState _prevDepthStencil;
        private readonly SamplerState _prevSampler;
        private readonly BlendState _prevBlend;
        private readonly Rectangle? _prevScissor;

        private bool _disposed;

        /// <summary>
        /// Initializes a rendering scope using the specified game context.
        /// </summary>
        /// <param name="context">The game context containing the graphics device.</param>
        /// <param name="options">The rendering options to apply, or the default options.</param>
        public RenderScope(GameContext context, RenderOptions? options = null)
            : this((context ?? throw new ArgumentNullException(nameof(context))).GraphicsDevice, options)
        {
            _sceneContext = context as SceneDrawContext;
        }

        /// <summary>Creates a scope that can begin with the device's current rendering states.</summary>
        /// <param name="context">The game context containing the graphics device.</param>
        /// <param name="preserveCurrentState">Whether to keep the active states on entry. Otherwise, the default options apply.</param>
        public RenderScope(GameContext context, bool preserveCurrentState)
            : this((context ?? throw new ArgumentNullException(nameof(context))).GraphicsDevice, null, preserveCurrentState)
        {
            _sceneContext = context as SceneDrawContext;
        }

        /// <summary>
        /// Initializes a rendering scope using the specified graphics device.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to configure.</param>
        /// <param name="options">The rendering options to apply, or the default options.</param>
        public RenderScope(GraphicsDevice graphicsDevice, RenderOptions? options = null)
            : this(graphicsDevice, options, preserveCurrentState: false)
        {
        }

        private RenderScope(GraphicsDevice graphicsDevice, RenderOptions? options, bool preserveCurrentState)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _prevRasterizer = _graphicsDevice.RasterizerState;
            _prevDepthStencil = _graphicsDevice.DepthStencilState;
            _prevSampler = _graphicsDevice.SamplerStates[0];
            _prevBlend = _graphicsDevice.BlendState;

            if (preserveCurrentState)
            {
                _prevScissor = _graphicsDevice.ScissorRectangle;
                return;
            }

            options ??= RenderOptions.Default;
            if (options.ScissorRectangle is { } scissor)
            {
                _prevScissor = _graphicsDevice.ScissorRectangle;
                _graphicsDevice.ScissorRectangle = scissor;
            }
            _graphicsDevice.RasterizerState = GetRasterizer(_graphicsDevice, options);
            _graphicsDevice.DepthStencilState = GetDepthStencil(options.Depth);
            _graphicsDevice.SamplerStates[0] = options.SamplerState;
            _graphicsDevice.BlendState = !options.AlphaBlend
                ? BlendState.Opaque
                : options.PremultipliedAlpha
                    ? BlendState.AlphaBlend
                    : BlendState.NonPremultiplied;
        }

        /// <summary>Draws a mesh using the graphics state active in this scope.</summary>
        /// <param name="mesh">The indexed mesh to draw.</param>
        /// <param name="shader">The shader, or the scene's default material shader if available.</param>
        /// <param name="transform">The mesh transform. Defaults to the identity matrix.</param>
        /// <param name="camera">The camera override. Defaults to the scene view camera if available.</param>
        /// <param name="primitiveType">The topology used to interpret the mesh indices.</param>
        /// <exception cref="ObjectDisposedException">The scope has already been disposed.</exception>
        /// <exception cref="InvalidOperationException">No shader was supplied and no scene material is available.</exception>
        public void DrawMesh(
            IMesh mesh,
            IShader? shader = null,
            Matrix? transform = null,
            ICameraTransform? camera = null,
            PrimitiveType primitiveType = PrimitiveType.TriangleList)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_sceneContext is { } context)
            {
                shader ??= context.DefaultMaterial.Shader;
                camera ??= context.HasCamera ? context.ViewCamera : null;
            }
            MeshRenderer.Draw(_graphicsDevice, mesh,
                shader ?? throw new InvalidOperationException("A render scope without a scene context requires an explicit shader."),
                camera, transform, primitiveType);
        }

        private static RasterizerState GetRasterizer(GraphicsDevice device, RenderOptions options)
        {
            var cache = _rasterizerCache.GetValue(device, static _ => new());
            var key = (options.CullMode, options.FillMode, options.ScissorRectangle.HasValue);
            if (!cache.TryGetValue(key, out var state) || state.IsDisposed)
            {
                state = new RasterizerState
                {
                    CullMode = options.CullMode,
                    FillMode = options.FillMode,
                    ScissorTestEnable = options.ScissorRectangle.HasValue
                };

                cache[key] = state;
            }

            return state;
        }

        private static DepthStencilState GetDepthStencil(DepthMode mode)
        {
            if (!_depthCache.TryGetValue(mode, out var state) || state.IsDisposed)
            {
                state = mode switch
                {
                    DepthMode.Disabled => DepthStencilState.None,

                    DepthMode.Opaque => DepthStencilState.Default,

                    DepthMode.Transparent => new DepthStencilState
                    {
                        DepthBufferEnable = true,
                        DepthBufferWriteEnable = false,
                        DepthBufferFunction = CompareFunction.LessEqual
                    },

                    DepthMode.Overlay => DepthStencilState.None,

                    DepthMode.DepthOnly => new DepthStencilState
                    {
                        DepthBufferEnable = true,
                        DepthBufferWriteEnable = true,
                        DepthBufferFunction = CompareFunction.LessEqual
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
            if (_prevScissor is { } scissor)
                _graphicsDevice.ScissorRectangle = scissor;
        }
    }
}
