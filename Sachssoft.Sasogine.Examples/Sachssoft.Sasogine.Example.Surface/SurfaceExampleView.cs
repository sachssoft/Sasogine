using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Surface;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Views;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Example
{
    public class SurfaceExampleView : SurfaceViewBase
    {
        public SurfaceExampleView()
        {
        }

        protected override ISurfaceElement CreateContainer()
        {
            var grid = new Grid();

            grid.Background = Color.DarkSalmon.ToBrush();

            return grid;
        }

        protected override void OnLoad(GameBaseContext context)
        {
            var camera = new Camera2D(context.GraphicsDevice);
            var effect = new BasicEffectAdapter(context.GraphicsDevice);

            Runtime = new ExampleRuntime(camera, effect);

            base.OnLoad(context);
        }

        protected override void OnUpdate(GameFrameContext context)
        {
            base.OnUpdate(context);
        }

        protected override void OnDraw(GameFrameContext context)
        {
            base.OnDraw(context);
        }

    }
}
