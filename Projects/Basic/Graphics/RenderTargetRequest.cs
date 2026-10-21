using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using System;

namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Provides shared management of a render target used for an optional
/// off-screen rendering pass.
/// </summary>
/// <remarks>
/// <para>
/// The render target is created lazily by <see cref="Ensure(int, int, int, SurfaceFormat, DepthFormat, int)"/> once a
/// <see cref="GraphicsDevice"/> has been provided through <see cref="Begin"/>.
/// </para>
/// <para>
/// <see cref="Begin"/> activates the render target before rendering components,
/// while <see cref="End"/> restores rendering to the back buffer.
/// <see cref="Ensure(int, int, int, SurfaceFormat, DepthFormat, int)"/> only creates or resizes the render target and does not
/// change the current graphics device render state.
/// </para>
/// </remarks>
public static class RenderTargetRequest
{
    private static GraphicsDevice? _graphicsDevice;
    private static RenderTarget2D? _renderTarget;

    private static PixelSize2 _renderSize;
    private static float _resolutionScale = 1f;

    private static int _lastW;
    private static int _lastH;
    private static int _lastQuality;
    private static int _lastAntiAliasing;

    private static SurfaceFormat _lastFormat;
    private static DepthFormat _lastDepthFormat;

    private static bool _isActive;

    /// <summary>
    /// Gets the current render target, or <see langword="null"/> if no render
    /// target has been created.
    /// </summary>
    public static RenderTarget2D? Current => _renderTarget;

    /// <summary>
    /// Gets the actual size of the current render target.
    /// </summary>
    public static PixelSize2 Size => _renderSize;

    /// <summary>
    /// Gets the resolution scale applied to the requested render size.
    /// </summary>
    public static float ResolutionScale => _resolutionScale;

    /// <summary>Ensures an off-screen target using an explicitly supplied device, before the first Begin.</summary>
    /// <param name="graphicsDevice">The graphics device owning the target.</param>
    /// <param name="width">The requested displayed width.</param>
    /// <param name="height">The requested displayed height.</param>
    /// <param name="quality">The existing render-quality percentage.</param>
    /// <param name="antiAliasing">The existing multisample setting.</param>
    public static void Ensure(GraphicsDevice graphicsDevice, int width, int height, int quality = 100, int antiAliasing = 0)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        if (_isActive) throw new InvalidOperationException("Cannot resize an active render target.");
        if (!ReferenceEquals(_graphicsDevice, graphicsDevice)) DisposeRenderTarget();
        _graphicsDevice = graphicsDevice;
        Ensure(width, height, quality: quality, antiAliasing: antiAliasing);
    }

    /// <summary>
    /// Ensures that a render target exists with the requested dimensions,
    /// quality, format, depth format, and anti-aliasing settings.
    /// </summary>
    /// <param name="width">The requested render width in pixels.</param>
    /// <param name="height">The requested render height in pixels.</param>
    /// <param name="quality">
    /// The render quality from 1 to 100 used to determine the resolution scale.
    /// </param>
    /// <param name="preferredFormat">
    /// The preferred surface format of the render target.
    /// </param>
    /// <param name="preferredDepthFormat">
    /// The preferred depth format of the render target.
    /// </param>
    /// <param name="antiAliasing">
    /// The requested multisample anti-aliasing level.
    /// </param>
    /// <remarks>
    /// This method does not activate the render target or otherwise change the
    /// current graphics device render state.
    /// </remarks>
    public static void Ensure(
        int width,
        int height,
        int quality = 100,
        SurfaceFormat preferredFormat = SurfaceFormat.Color,
        DepthFormat preferredDepthFormat = DepthFormat.Depth24,
        int antiAliasing = 0)
    {
        if (_graphicsDevice == null)
        {
            _renderSize = PixelSize2.Zero;
            _resolutionScale = 1f;
            return;
        }

        if (width <= 0 || height <= 0)
        {
            DisposeRenderTarget();
            return;
        }

        quality = int.Clamp(quality, 1, 100);

        float q = quality / 100f;
        float scale = 0.25f + q * q * 0.75f;

        int renderWidth = int.Clamp((int)(width * scale), 1, width);
        int renderHeight = int.Clamp((int)(height * scale), 1, height);

        int msaa = antiAliasing switch
        {
            <= 0 => 0,
            <= 2 => 2,
            <= 4 => 4,
            <= 8 => 8,
            _ => 8
        };

        if (_renderTarget != null &&
            !_renderTarget.IsDisposed &&
            width == _lastW &&
            height == _lastH &&
            quality == _lastQuality &&
            preferredFormat == _lastFormat &&
            preferredDepthFormat == _lastDepthFormat &&
            antiAliasing == _lastAntiAliasing)
        {
            _renderSize = new PixelSize2(_renderTarget.Width, _renderTarget.Height);
            _resolutionScale = scale;
            return;
        }

        _renderTarget?.Dispose();

        _renderTarget = new RenderTarget2D(
            _graphicsDevice,
            width: renderWidth,
            height: renderHeight,
            mipMap: false,
            preferredFormat: preferredFormat,
            preferredDepthFormat: preferredDepthFormat,
            preferredMultiSampleCount: msaa,
            usage: RenderTargetUsage.PlatformContents);

        _renderSize = new PixelSize2(_renderTarget.Width, _renderTarget.Height);
        _resolutionScale = scale;

        _lastW = width;
        _lastH = height;
        _lastQuality = quality;
        _lastFormat = preferredFormat;
        _lastDepthFormat = preferredDepthFormat;
        _lastAntiAliasing = antiAliasing;
    }

    /// <summary>
    /// Begins the render target pass using the specified graphics device.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used for rendering.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// A render target pass is already active.
    /// </exception>
    public static void Begin(GraphicsDevice graphicsDevice)
    {
        if (_isActive)
            throw new InvalidOperationException("Begin cannot be called twice without End.");

        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));

        if (_renderTarget != null && !_renderTarget.IsDisposed)
        {
            _graphicsDevice.SetRenderTarget(_renderTarget);

            var clearOptions = ClearOptions.Target;

            if (_renderTarget.DepthStencilFormat != DepthFormat.None)
                clearOptions |= ClearOptions.DepthBuffer;

            if (_renderTarget.DepthStencilFormat == DepthFormat.Depth24Stencil8)
                clearOptions |= ClearOptions.Stencil;

            _graphicsDevice.Clear(
                clearOptions,
                Color.Transparent,
                depth: 1f,
                stencil: 0);
        }

        _isActive = true;
    }

    /// <summary>
    /// Ends the current render target pass and restores rendering to the
    /// back buffer.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// No render target pass is active or the graphics device is unavailable.
    /// </exception>
    public static void End()
    {
        if (!_isActive)
            throw new InvalidOperationException("End called without Begin.");

        if (_graphicsDevice == null)
            throw new InvalidOperationException("GraphicsDevice is missing.");

        _graphicsDevice.SetRenderTarget(null);
        _isActive = false;
    }

    /// <summary>
    /// Releases the current render target and resets its state.
    /// </summary>
    public static void Flush()
    {
        if (_isActive)
            throw new InvalidOperationException("Flush cannot be called while a render target pass is active.");

        DisposeRenderTarget();
    }

    private static void DisposeRenderTarget()
    {
        _renderTarget?.Dispose();
        _renderTarget = null;

        _renderSize = PixelSize2.Zero;
        _resolutionScale = 1f;

        _lastW = 0;
        _lastH = 0;
        _lastQuality = 0;
        _lastAntiAliasing = 0;

        _lastFormat = default;
        _lastDepthFormat = default;
    }
}