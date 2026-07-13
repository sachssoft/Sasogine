using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using System;
using System.Diagnostics;

namespace Sachssoft.Sasogine.Graphics
{
    public static class RenderTargetRequest
    {
        // Erklärung:
        //
        // Draw VOR Komponenten -> Start
        // -> Bedeutet: RenderTarget wird vor dem Zeichnen von Komponenten aktiviert
        // -> Alles danach (World, Entities, etc.) wird in das RenderTarget gerendert
        //
        // Draw NACH oder IN letzten Komponenten z.B. UI -> End
        // -> Bedeutet: RenderTarget wird wieder deaktiviert
        // -> Danach wird wieder direkt auf den Backbuffer (Bildschirm) gerendert
        //
        // Ensure -> Beliebig platzieren, auch im jetzigen Zeitpunkt trotz ohne Start problemlos möglich
        // -> Ensure erstellt oder resized nur das RenderTarget
        // -> Es führt KEIN Rendering aus und verändert keinen GraphicsDevice-State
        //
        // daher Current auch NULL
        // -> Wenn Ensure noch nicht ausgeführt wurde oder kein RenderTarget existiert,
        //    ist Current bewusst NULL (Lazy Initialization / optionaler Render-Pass)

        private static GraphicsDevice? _graphicsDevice;
        private static RenderTarget2D? _renderTarget;

        private static PixelSize _renderSize;
        private static float _resolutionScale;
        private static int _lastW;
        private static int _lastH;
        private static int _lastQuality;
        private static int _lastAntiAliasing;

        private static bool _isActive;

        public static RenderTarget2D? Current => _renderTarget;

        public static PixelSize Size => _renderSize; //_renderTarget == null ? PixelSize.Zero : new(_renderTarget.Bounds.Size);

        public static float ResolutionScale => _resolutionScale;

        public static void Ensure(
            int width,
            int height,
            int quality = 100,
            SurfaceFormat preferredFormat = SurfaceFormat.Color,
            DepthFormat preferredDepthFormat = DepthFormat.None,
            int antiAliasing = 0)
        {
            if (_graphicsDevice == null)
            {
                _renderSize = PixelSize.Zero;
                _resolutionScale = 1f;
                return;
            }

            if (width <= 0 || height <= 0)
            {
                _renderSize = PixelSize.Zero;
                _resolutionScale = 1f;
                return;
            }

            if (_renderTarget != null &&
                width == _lastW &&
                height == _lastH &&
                quality == _lastQuality &&
                antiAliasing == _lastAntiAliasing)
            {
                _renderSize = PixelSize.Zero;
                _resolutionScale = 1f;
                return;
            }


            // Qualität 1-100% (entspricht 25% bis 100% der Auflösung)
            float q = int.Clamp(quality, 1, 100) / 100f;
            float scale = 0.25f + (q * q) * 0.75f;

            int msaa = antiAliasing switch
            {
                <= 0 => 0,
                <= 2 => 2,
                <= 4 => 4,
                <= 8 => 8,
                _ => 8
            };

            _renderTarget?.Dispose();

            _renderTarget = new RenderTarget2D(
                _graphicsDevice,
                width: int.Clamp((int)(width * scale), 1, width),
                height: int.Clamp((int)(height * scale), 1, height),
                mipMap: false,
                preferredFormat: preferredFormat,
                preferredDepthFormat: preferredDepthFormat,
                preferredMultiSampleCount: msaa,
                usage: RenderTargetUsage.PlatformContents
            );

            _renderSize = new PixelSize(_renderTarget.Bounds.Size);

            _lastW = width;
            _lastH = height;
            _lastQuality = quality;
            _lastAntiAliasing = antiAliasing;

            _resolutionScale = scale;
        }

        public static void Begin(GraphicsDevice graphicsDevice)
        {
            if (_isActive)
                throw new InvalidOperationException("Begin cannot be called twice without End.");

            if (graphicsDevice == null)
                throw new ArgumentNullException(nameof(graphicsDevice));

            _graphicsDevice = graphicsDevice;

            if (_renderTarget != null)
            {
                _graphicsDevice.SetRenderTarget(_renderTarget);
                _graphicsDevice.Clear(Color.Transparent);
            }

            _isActive = true;
        }

        public static void End()
        {
            if (!_isActive)
                throw new InvalidOperationException("End called without Begin.");

            if (_graphicsDevice == null)
                throw new InvalidOperationException("GraphicsDevice is missing.");

            _graphicsDevice.SetRenderTarget(null);

            _isActive = false;
        }

        public static void Flush()
        {
            _renderTarget?.Dispose();
            _renderTarget = null;

            _renderSize = PixelSize.Zero;
            _resolutionScale = 1f;
        }
    }
}