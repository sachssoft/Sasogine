using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Surface;
using Sachssoft.Sasogine.Surface.Controls;

namespace Sachssoft.Sasogine.Example
{
    public class SurfaceExampleApp : MyGameApp<SurfaceExampleAssets>
    {
        private CameraBase? _camera;

        protected override SurfaceExampleAssets CreateAssetManager()
        {
            return new SurfaceExampleAssets(this);
        }

        protected override SurfaceHost? CreateSurfaceHost()
        {
            //return new Desktop();
            return new Workspace();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

        }

        protected override void InitializeViews(ViewManager view)
        {
            base.InitializeViews(view);

            Window.AllowUserResizing = true;

            view.Register<SurfaceExampleView>();
            view.SetDefault<SurfaceExampleView>();
        }

        protected override void OnUpdate(GameFrameContext context)
        {
            base.OnUpdate(context);

            _camera?.Update(context);
        }
    }
}
