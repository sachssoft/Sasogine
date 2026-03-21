using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Presentation;
using Sachssoft.Sasogine.Surface;
using Sachssoft.Sasogine.Surface.Controls;

namespace Sachssoft.Sasogine.Example
{
    public class SurfaceExampleApp : GameApplication<SurfaceExampleResources>
    {
        private CameraBase? _camera;

        protected override SurfaceExampleResources CreateResources()
        {
            return new SurfaceExampleResources(this);
        }

        protected override IPresentationHost? CreatePresensationHost()
        {
            return new Desktop();
        }

        protected override void InitializeViews(SceneManager view)
        {
            base.InitializeViews(view);

            Window.AllowUserResizing = true;

            view.Register<SurfaceExampleView>();
            view.SetDefault<SurfaceExampleView>();
        }

        protected override void OnUpdate(GameContext context)
        {
            base.OnUpdate(context);
        }

        //protected override void InitializeViews(ViewManager view)
        //{
        //    base.InitializeViews(view);

        //    Window.AllowUserResizing = true;

        //    view.Register<SurfaceExampleView>();
        //    view.SetDefault<SurfaceExampleView>();
        //}

        //protected override void OnUpdate(GameFrameContext context)
        //{
        //    base.OnUpdate(context);

        //    _camera?.Update(context);
        //}
    }
}
