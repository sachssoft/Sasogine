using Sachssoft.Sasogine.Presentation.Deterlite.Basic;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Layouts
{
    public abstract class LayoutBase : FrameBase
    {
        public LayoutBase()
        {
            Layer = FrameLayer.Background;
        }

        public FrameCollection Children => ChildFrames;
    }
}
