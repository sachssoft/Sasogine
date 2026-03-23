using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Sachssoft.Sasogine.Surface.Visuals;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Surface.Scenes;

internal class FadeOverlay : Container
{
    private readonly List<Func<bool>> _frame_updates = new();

    public FadeOverlay(Color color, float initial_alpha)
    {
        Background = new SolidColorBrush(color * initial_alpha);
        HorizontalAlignment = Visuals.HorizontalAlignment.Stretch;
        VerticalAlignment = Visuals.VerticalAlignment.Stretch;
    }

    public override void InternalRender(RenderContext context, GameTime t)
    {
        for (int i = _frame_updates.Count - 1; i >= 0; i--)
        {
            var done = _frame_updates[i].Invoke();
            if (done)
                _frame_updates.RemoveAt(i);
        }

        base.InternalRender(context, t);
    }

    public Task FadeToAsync(float targetAlpha, int durationMs)
    {
        var tcs = new TaskCompletionSource<bool>();
        float start_alpha = ((SolidColorBrush)Background).Color.A / 255f;
        float end_Alpha = targetAlpha;
        int elapsed = 0;

        _frame_updates.Add(() =>
        {
            elapsed += 16; // simulierte Framerate
            float t = float.Clamp(elapsed / (float)durationMs, 0f, 1f);
            float alpha = float.Lerp(start_alpha, end_Alpha, t);
            Background = new SolidColorBrush(ColorUtils.ChangeAlphaChannel(((SolidColorBrush)Background).Color, alpha));

            if (elapsed >= durationMs)
            {
                Background = new SolidColorBrush(ColorUtils.ChangeAlphaChannel(((SolidColorBrush)Background).Color, end_Alpha));
                tcs.SetResult(true);
                return true; // fertig
            }

            return false; // nächste Frame
        });

        return tcs.Task;
    }
}

//internal class FadeOverlay : Container
//{
//    public FadeOverlay(Color color, float initial_alpha)
//    {
//        Background = new SolidBrush(color * initial_alpha);
//        HorizontalAlignment = HorizontalAlignment.Stretch;
//        VerticalAlignment = VerticalAlignment.Stretch;
//    }

//    public async Task FadeToAsync(float target_alpha, int duration_ms)
//    {
//        float start_alpha = ((SolidBrush)Background).Color.A / 255f;
//        float end_alpha = target_alpha;
//        int elapsed = 0;

//        while (elapsed < duration_ms)
//        {
//            float t = elapsed / (float)duration_ms;
//            float alpha = MathHelper.Lerp(start_alpha, end_alpha, t);
//            Background = new SolidBrush(((SolidBrush)Background).Color.ChangeAlphaChannel(alpha));
//            await Task.Delay(16); // ca. 60 FPS
//            elapsed += 16;
//        }

//        Background = new SolidBrush(((SolidBrush)Background).Color.ChangeAlphaChannel(end_alpha));
//    }
//}