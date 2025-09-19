using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using System;
using Microsoft.Xna.Framework;
using System.ComponentModel.DataAnnotations;

namespace Sachssoft.Sasogine;

public abstract class SplitScreenRuntime : RuntimeBase
{
    private readonly CameraBase[] _cameras;

    public int PlayerCount { get; }

    public CameraBase[] Cameras => _cameras;

    public SplitScreenRuntime(CameraBase[] cameras, IEffectAdapter? effect)
        : base(cameras[0], effect) // Hauptkamera egal – du renderst pro Kamera später
    {
        if (cameras.Length < 1 || cameras.Length > 4)
            throw new ArgumentException("SplitScreenRuntime supports 1 to 4 players.");

        _cameras = cameras;
        PlayerCount = cameras.Length;
    }

    protected override sealed void OnScreenDraw(GameFrameContext context)
    {
        var graphicsDevice = context.GraphicsDevice; //GetGraphicsDeviceSafely();

        var full = graphicsDevice.Viewport.Bounds;
        var viewports = GetSplitViewports(full, PlayerCount);

        for (int i = 0; i < PlayerCount; i++)
        {
            graphicsDevice.Viewport = new Viewport(viewports[i]);
            _cameras[i].Update(context);

            var cxt = new MultiScreenGameContext(context, _cameras[i], viewports[i], i);
            OnPlayerScreenDraw(cxt);
        }

        // Wichtig: Viewport zurücksetzen
        graphicsDevice.Viewport = new Viewport(full);
    }

    protected virtual void OnPlayerScreenDraw(MultiScreenGameContext context)
    {
        Components.ForEachDrawable(context);

        // Zeichne hier den Level oder Spielausschnitt für den jeweiligen Spieler
        // z.B. TileMap.Draw(camera), Entities.Draw(camera), etc.
        // Zugriff auf `Effect` oder `EffectOverrite(context)` möglich
    }

    private static Rectangle[] GetSplitViewports(Rectangle bounds, int count)
    {
        int w = bounds.Width;
        int h = bounds.Height;

        return count switch
        {
            1 => new[] { new Rectangle(0, 0, w, h) }, // ganzer Bildschirm
            2 => new[]
            {
            new Rectangle(0, 0, w, h / 2),
            new Rectangle(0, h / 2, w, h / 2)
        },
            3 => new[]
            {
            new Rectangle(0, 0, w / 2, h / 2),
            new Rectangle(w / 2, 0, w / 2, h / 2),
            new Rectangle(0, h / 2, w, h / 2)
        },
            4 => new[]
            {
            new Rectangle(0, 0, w / 2, h / 2),
            new Rectangle(w / 2, 0, w / 2, h / 2),
            new Rectangle(0, h / 2, w / 2, h / 2),
            new Rectangle(w / 2, h / 2, w / 2, h / 2)
        },
            _ => throw new NotSupportedException("Unsupported player count.")
        };
    }

}
