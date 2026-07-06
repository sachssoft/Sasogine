using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Shared.Basic.Graphics
{
    public class ScreenshotRequestEventArgs : EventArgs
    {

        public ScreenshotRequestEventArgs(Texture2D capture)
        {
            Capture = capture ?? throw new ArgumentNullException(nameof(capture));
        }

        public Texture2D Capture { get; }
    }
}
