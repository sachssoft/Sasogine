using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Engine
{
    public class RuntimeContext : GameContext
    {
        public RuntimeContext(GameApplication application, RuntimeBase runtime)
            : base(application)
        {
            Runtime = runtime;
        }

        public RuntimeBase Runtime { get; }

        public ICamera PrimaryCamera => Runtime.PrimaryCamera;
        //public CameraBase PrimaryCamera => Runtime.PrimaryCamera;

        public IEffectAdapter PrimaryEffectAdapter => Runtime.PrimaryEffectAdapter;
    }
}
