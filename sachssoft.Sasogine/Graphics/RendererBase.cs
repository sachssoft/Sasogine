using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace sachssoft.Sasogine.Graphics;

public abstract class RendererBase : IDisposable
{
    private bool _disposed;
    private readonly GraphicsDevice _graphics_device;
    private RasterizerState _default_rasterizer;
    private RasterizerState _rasterizer;
    private DepthStencilState _depth_stencil_state;
    private DepthStencilState _default_depth_stencil_state;
    private SamplerState _default_sampler_state;
    private SamplerState _sampler_state;
    private BlendState _blend_state;
    private BlendState _default_blend_state;
    private readonly IEffect _effect;

    protected RendererBase(GraphicsDevice graphics_device, IEffect effect, params object[] args)
    {
        _graphics_device = graphics_device;
        _effect = effect;

        _default_rasterizer = _graphics_device.RasterizerState;
        _default_depth_stencil_state = _graphics_device.DepthStencilState;
        _default_sampler_state = _graphics_device.SamplerStates[0];
        _default_blend_state = _graphics_device.BlendState;

        _rasterizer = new RasterizerState
        {
            CullMode = CullMode.None,
            FillMode = FillMode.Solid
        };

        _depth_stencil_state = DepthStencilState.Default;

        OnInitialize(args);

        _graphics_device.RasterizerState = _rasterizer;
        _graphics_device.DepthStencilState = _depth_stencil_state;
        _graphics_device.SamplerStates[0] = _sampler_state ??= SamplerState.PointWrap;
        _graphics_device.BlendState = _blend_state ??= BlendState.AlphaBlend;
    }

    public virtual void Flush() { }

    protected virtual void OnInitialize(object[] args)
    {
    }

    protected virtual void OnRenderCompleted()
    {
    }

    protected virtual void OnUninitialize()
    {
    }

    protected IEffect Effect => _effect;

    protected BlendState BlendState
    {
        get => _blend_state;
        set
        {
            _blend_state = value;
            _graphics_device.BlendState = _blend_state;
        }
    }

    protected RasterizerState Rasterizer => _rasterizer;

    protected SamplerState SamplerState
    {
        get => _sampler_state;
        set
        {
            _sampler_state = value;
            _graphics_device.SamplerStates[0] = _sampler_state;
        }
    }

    protected DepthStencilState DepthStencil
    {
        get => _depth_stencil_state;
        set
        {
            _depth_stencil_state = value;
            _graphics_device.DepthStencilState = _depth_stencil_state;
        }
    }

    public IEnumerable<EffectPass> GetEffectPasses()
    {
        return _effect?.CurrentTechnique?.Passes ?? (IEnumerable<EffectPass>)Array.Empty<EffectPass>();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        OnRenderCompleted();

        _graphics_device.RasterizerState = _default_rasterizer;
        _graphics_device.DepthStencilState = _default_depth_stencil_state;
        _graphics_device.SamplerStates[0] = _default_sampler_state;
        _graphics_device.BlendState = _default_blend_state;

        OnUninitialize();

        _rasterizer?.Dispose();
        _depth_stencil_state?.Dispose();
        _sampler_state?.Dispose();
        _blend_state?.Dispose();
    }
}
