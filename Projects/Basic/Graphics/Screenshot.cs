using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Provides functionality for capturing screenshots from the graphics
/// device back buffer or a render target.
/// </summary>
/// <remarks>
/// Screenshots can either be captured immediately or requested for capture
/// after the current frame has been fully rendered.
/// </remarks>
public static class Screenshot
{
    private static bool _captureRequested;
    private static Texture2D? _lastCapture;
    private static Color[]? _buffer;

    #region Capture Request

    /// <summary>
    /// Occurs when a requested screenshot has been captured.
    /// </summary>
    public static event EventHandler<ScreenshotRequestEventArgs>? CaptureRequested;

    // Aufgrund des Komponentensystems wird der Screenshot nicht immer korrekt gerendert.
    // Diese Lösung stellt sicher, dass der Screenshot erst nach vollständigem Frame-Rendering erzeugt wird.

    /// <summary>
    /// Requests a screenshot to be captured when
    /// <see cref="CaptureIfRequested(GraphicsDevice)"/> is next called.
    /// </summary>
    public static void RequestCapture()
    {
        _captureRequested = true;
    }

    /// <summary>
    /// Captures the current back buffer if a screenshot has previously
    /// been requested using <see cref="RequestCapture"/>.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device whose back buffer is captured.
    /// </param>
    /// <remarks>
    /// If no capture is pending, this method performs no operation.
    /// After a successful capture, <see cref="CaptureRequested"/> is raised
    /// and the resulting texture can be obtained using
    /// <see cref="ConsumeLastCapture"/>.
    /// </remarks>
    public static void CaptureIfRequested(GraphicsDevice graphicsDevice)
    {
        if (!_captureRequested)
            return;

        _captureRequested = false;

        _lastCapture?.Dispose();

        int width = graphicsDevice.PresentationParameters.BackBufferWidth;
        int height = graphicsDevice.PresentationParameters.BackBufferHeight;

        var data = new Color[width * height];
        graphicsDevice.GetBackBufferData(data);

        _lastCapture = new Texture2D(graphicsDevice, width, height);
        _lastCapture.SetData(data);

        if (_lastCapture != null)
            CaptureRequested?.Invoke(null, new ScreenshotRequestEventArgs(_lastCapture));
    }

    /// <summary>
    /// Retrieves the most recently requested screenshot and removes it
    /// from the internal capture storage.
    /// </summary>
    /// <returns>
    /// The most recently captured screenshot texture, or
    /// <see langword="null"/> if no captured texture is available.
    /// </returns>
    /// <remarks>
    /// Ownership of the returned texture is transferred to the caller.
    /// The caller is responsible for disposing it when it is no longer needed.
    /// </remarks>
    public static Texture2D? ConsumeLastCapture()
    {
        var tex = _lastCapture;
        _lastCapture = null;
        return tex;
    }

    #endregion

    // Öffentliche Methoden für Backbuffer und RenderTarget

    /// <summary>
    /// Creates a texture containing the current contents of the graphics
    /// device back buffer.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device whose back buffer is captured.
    /// </param>
    /// <returns>
    /// A new texture containing the captured back buffer.
    /// </returns>
    /// <remarks>
    /// The caller is responsible for disposing the returned texture.
    /// </remarks>
    public static Texture2D Create(GraphicsDevice graphicsDevice)
    {
        int width = graphicsDevice.PresentationParameters.BackBufferWidth;
        int height = graphicsDevice.PresentationParameters.BackBufferHeight;

        return CreateTexture(
            graphicsDevice,
            width,
            height,
            data => graphicsDevice.GetBackBufferData(data));
    }

    /// <summary>
    /// Creates a texture containing the current contents of the specified
    /// render target.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the resulting texture.
    /// </param>
    /// <param name="target">
    /// The render target whose contents are captured.
    /// </param>
    /// <returns>
    /// A new texture containing the captured render target contents.
    /// </returns>
    /// <remarks>
    /// The caller is responsible for disposing the returned texture.
    /// </remarks>
    public static Texture2D FromRenderTarget(
        GraphicsDevice graphicsDevice,
        RenderTarget2D target)
    {
        return CreateTexture(
            graphicsDevice,
            target.Width,
            target.Height,
            data => target.GetData(data));
    }

    // Capture direkt in Stream, Backbuffer oder RenderTarget

    /// <summary>
    /// Captures the current graphics device back buffer and writes it
    /// to the specified stream.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device whose back buffer is captured.
    /// </param>
    /// <param name="stream">
    /// The stream to which the encoded screenshot is written.
    /// </param>
    /// <param name="format">
    /// The image format used to encode the screenshot.
    /// </param>
    public static void Capture(
        GraphicsDevice graphicsDevice,
        Stream stream,
        ScreenshotImageFormat format = ScreenshotImageFormat.PNG)
    {
        using var texture = Create(graphicsDevice);
        SaveTexture(texture, stream, format);
    }

    /// <summary>
    /// Captures the specified render target and writes it to the
    /// specified stream.
    /// </summary>
    /// <param name="target">
    /// The render target whose contents are captured.
    /// </param>
    /// <param name="graphicsDevice">
    /// The graphics device used to create the temporary capture texture.
    /// </param>
    /// <param name="stream">
    /// The stream to which the encoded screenshot is written.
    /// </param>
    /// <param name="format">
    /// The image format used to encode the screenshot.
    /// </param>
    public static void Capture(
        RenderTarget2D target,
        GraphicsDevice graphicsDevice,
        Stream stream,
        ScreenshotImageFormat format = ScreenshotImageFormat.PNG)
    {
        using var texture = FromRenderTarget(graphicsDevice, target);
        SaveTexture(texture, stream, format);
    }

    // Gemeinsame Methode für PNG/JPG

    private static void SaveTexture(
        Texture2D texture,
        Stream stream,
        ScreenshotImageFormat format)
    {
        switch (format)
        {
            case ScreenshotImageFormat.PNG:
                texture.SaveAsPng(stream, texture.Width, texture.Height);
                break;

            case ScreenshotImageFormat.JPG:
                texture.SaveAsJpeg(stream, texture.Width, texture.Height);
                break;

            default:
                texture.SaveAsPng(stream, texture.Width, texture.Height);
                break;
        }
    }

    private static Color[] GetBuffer(int size)
    {
        if (_buffer == null || _buffer.Length != size)
            _buffer = new Color[size];

        return _buffer;
    }

    // Interne generische Methode für Backbuffer oder RenderTarget

    private static Texture2D CreateTexture(
        GraphicsDevice graphicsDevice,
        int width,
        int height,
        Action<Color[]> getData)
    {
        var data = GetBuffer(width * height);
        getData(data);

        var texture = new Texture2D(graphicsDevice, width, height);
        texture.SetData(data);

        return texture;
    }
}