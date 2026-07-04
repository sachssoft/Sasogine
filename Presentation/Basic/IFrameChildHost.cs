namespace Sachssoft.Sasogine.Presentation
{
    public interface IFrameChildHost
    {
        FrameCollection ChildFrames { get; }

        bool ShouldInvalidate { get; }

        void Invalidate();
    }
}
