using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Scenes
{
    public class SceneDrawContext : GameContext
    {
        public SceneDrawContext(
            IGameApplication application,
            IScene scene,
            ICamera viewCamera,
            IEffectAdapter effectAdapter,
            int viewIndex,
            int viewCount,
            float frameCounterSmoothing = 0.1f,
            float frameCounterFastWeight = 0.2f)
            : base(application, frameCounterSmoothing, frameCounterFastWeight)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            ViewCamera = viewCamera ?? throw new ArgumentNullException(nameof(viewCamera));
            EffectAdapter = effectAdapter ?? throw new ArgumentNullException(nameof(effectAdapter));
            ViewIndex = viewIndex;
            ViewCount = viewCount;
        }

        public IScene Scene { get; }

        public ICamera ViewCamera { get; }

        public int ViewIndex { get; }

        public int ViewCount { get; }

        public IEffectAdapter EffectAdapter { get; }

        public Rectangle GetViewport(Rectangle screenBounds)
        {
            int w = screenBounds.Width;
            int h = screenBounds.Height;

            switch (ViewCount)
            {
                case 1:
                    return new Rectangle(0, 0, w, h);
                case 2:
                    return ViewIndex switch
                    {
                        0 => new Rectangle(0, 0, w, h / 2),
                        1 => new Rectangle(0, h / 2, w, h / 2),
                        _ => throw new NotSupportedException()
                    };
                case 3:
                    return ViewIndex switch
                    {
                        0 => new Rectangle(0, 0, w / 2, h / 2),
                        1 => new Rectangle(w / 2, 0, w / 2, h / 2),
                        2 => new Rectangle(0, h / 2, w, h / 2),
                        _ => throw new NotSupportedException()
                    };
                case 4:
                    return ViewIndex switch
                    {
                        0 => new Rectangle(0, 0, w / 2, h / 2),
                        1 => new Rectangle(w / 2, 0, w / 2, h / 2),
                        2 => new Rectangle(0, h / 2, w / 2, h / 2),
                        3 => new Rectangle(w / 2, h / 2, w / 2, h / 2),
                        _ => throw new NotSupportedException()
                    };
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
