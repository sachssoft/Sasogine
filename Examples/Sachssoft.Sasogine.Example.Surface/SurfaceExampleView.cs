using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface.Controls;
using Sachssoft.Sasogine.Surface.Scenes;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Example
{
    public class SurfaceExampleView : SurfaceSceneBase
    {
        public SurfaceExampleView()
        {
            Container = new Grid()
            {
                Background = Color.DarkSalmon.ToBrush()
            };
        }

        protected override void OnDrawContent(PresentationContext context)
        {
            base.OnDrawContent(context);
        }


        //protected override ISurfaceElement CreateContainer()
        //{
        //    var grid = new Grid();

        //    grid.Background = Color.DarkSalmon.ToBrush();

        //    return grid;
        //}

        //protected override void OnLoad(GameBaseContext context)
        //{
        //    var camera = new Camera2D(context.GraphicsDevice);
        //    var effect = new BasicEffectAdapter(context.GraphicsDevice);

        //    Runtime = new ExampleRuntime(camera, effect);

        //    base.OnLoad(context);
        //}

        //protected override void OnUpdate(GameFrameContext context)
        //{
        //    base.OnUpdate(context);
        //}

        //protected override void OnDraw(GameFrameContext context)
        //{
        //    base.OnDraw(context);
        //}

    }
}
