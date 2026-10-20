using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Verifies real render-target readback, logical coordinates and premultiplied alpha conversion.</summary>
internal static class AssetGraphicsChecks
{
    internal static void Verify(GraphicsDevice graphics)
    {
        using var target = new RenderTarget2D(graphics, 2, 2, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        using var white = new Texture2D(graphics, 1, 1);
        using var batch = new SpriteBatch(graphics);
        white.SetData(new[] { Color.White });
        RenderTargetBinding[] previous = graphics.GetRenderTargets();
        var source = new RenderTargetColorSource(target, new Bounds2(10, 20, 200, 100));
        try
        {
            graphics.SetRenderTarget(target); graphics.Clear(Color.Black);
            if (source.TryPick(new Vector2(10, 20), out _)) throw new InvalidOperationException("Bound targets must not be read.");
            batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp);
            batch.Draw(white, new Rectangle(0, 0, 1, 1), new Color(64, 32, 16, 128));
            batch.Draw(white, new Rectangle(1, 0, 1, 1), Color.Blue);
            batch.Draw(white, new Rectangle(0, 1, 1, 1), Color.Green);
            batch.Draw(white, new Rectangle(1, 1, 1, 1), Color.White);
            batch.End();
            graphics.SetRenderTargets(previous);
            if (!source.TryPick(new Vector2(10, 20), out Color sampled) || sampled != new Color(128, 64, 32, 128))
                throw new InvalidOperationException("Render-target alpha conversion or logical coordinate mapping failed.");
            if (!source.TryPick(new Vector2(209, 119), out sampled) || sampled != Color.White || source.LastPixel != new Point(1, 1))
                throw new InvalidOperationException("Last target pixel mapping failed.");
            if (source.TryPick(new Vector2(210, 120), out _)) throw new InvalidOperationException("Outside target samples must be rejected.");
            source.Source = new Rectangle(1, 0, 1, 2);
            if (!source.TryPick(new Vector2(10, 20), out sampled) || sampled != Color.Blue)
                throw new InvalidOperationException("Atlas region picking failed.");
            Console.WriteLine("PASS: Render-target picking: bound-target rejection, scaled coordinates, source regions, RGBA and magnifier readback.");
        }
        finally { graphics.SetRenderTargets(previous); }
    }
}
