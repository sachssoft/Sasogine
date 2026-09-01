using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Services
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
    public class ViewportCursorService : IComponentService
    {
        private Vector2 _viewportCursorPosition;
        private Vector2 _screenPosition;
        private Vector2 _worldPosition;
        private PixelBounds2 _viewport;
        private PixelSize2 _renderSize;

        public Vector2 ViewportPosition => _viewportCursorPosition;

        public PixelBounds2 Viewport => _viewport;

        public PixelSize2 RenderSize => _renderSize;

        public Vector2 ScreenPosition => _screenPosition;

        /// <summary>
        /// Maus befindet sich innerhalb des Render-Frames.
        /// </summary>
        public bool IsInside { get; set; }

        public void Update(
            GraphicsDevice graphicsDevice,
            PixelBounds2 viewport,
            Point viewportCursorPosition,
            PixelSize2 renderSize)
        {

            var screenScale = renderSize.ToVector2() / graphicsDevice.Viewport.Bounds.Size.ToVector2();
            var viewportScale = renderSize.ToVector2() / viewport.Size.ToVector2();

            var screenCursorPosition =
                new Vector2(
                    viewportCursorPosition.X / screenScale.X * viewportScale.X,
                    viewportCursorPosition.Y / screenScale.Y * viewportScale.Y);

            _screenPosition = screenCursorPosition;
            _viewport = viewport;
            _renderSize = renderSize;

            //Console.Clear();
            //Console.WriteLine("Render Size {0}", renderSize);
            //Console.WriteLine("Screen Size {0}", context.GraphicsDevice.Viewport.Bounds.Size);
            //Console.WriteLine("Screen Cursor Position: {0}", screenCursorPosition);
        }
    }
}