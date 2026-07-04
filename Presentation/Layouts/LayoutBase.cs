namespace Sachssoft.Sasogine.Presentation.Layouts
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
