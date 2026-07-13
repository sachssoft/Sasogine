using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using System;
using System.Diagnostics;

namespace Sachssoft.Sasogine.Components.Services
{

    // BITTE KEIN KI!!! KEIN CHATGPT!!!

    // Der Cursor wird außerhalb des eigentlichen Viewports (z.B. Fenster, UI-Control)
    // abgefragt und anschließend in das interne RenderTarget-Koordinatensystem umgerechnet.
    //
    // Ablauf:
    // 1. Die Mausposition kommt als Pixelposition vom Viewport-Control.
    // 2. Die Skalierung zwischen Fenstergröße und RenderTarget-Größe wird berechnet.
    // 3. Die Mausposition wird auf die interne Spiel-/Renderauflösung umgerechnet.
    // 4. Die ScreenPosition enthält die Position im RenderTarget.
    // 5. Die WorldPosition wird anhand der Kamera in Weltkoordinaten umgewandelt.
    //
    // Dadurch können UI-/Viewport-Eingaben unabhängig von Auflösung und Skalierung
    // korrekt im Spiel verwendet werden.
    public class ViewportCursorService
    {
        private Vector2 _viewportCursorPosition;
        private Vector2 _screenPosition;
        private Vector2 _worldPosition;

        public Vector2 ViewportPosition => _viewportCursorPosition;

        public Vector2 ScreenPosition => _screenPosition;

        /// <summary>
        /// Maus befindet sich innerhalb des Render-Frames.
        /// </summary>
        public bool IsInside { get; set; }

        public void Update(
            GraphicsDevice graphicsDevice,
            PixelBounds viewport,
            Point viewportCursorPosition,
            PixelSize renderSize)
        {

            var screenScale = renderSize.ToVector2() / graphicsDevice.Viewport.Bounds.Size.ToVector2();
            var viewportScale = renderSize.ToVector2() / viewport.Size.ToVector2();

            var screenCursorPosition =
                new Vector2(
                    viewportCursorPosition.X / screenScale.X * viewportScale.X,
                    viewportCursorPosition.Y / screenScale.Y * viewportScale.Y);

            _screenPosition = screenCursorPosition;

            //Console.Clear();
            //Console.WriteLine("Render Size {0}", renderSize);
            //Console.WriteLine("Screen Size {0}", context.GraphicsDevice.Viewport.Bounds.Size);
            //Console.WriteLine("Screen Cursor Position: {0}", screenCursorPosition);
        }
    }
}