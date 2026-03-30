namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public interface IFrameChildHost
    {
        FrameCollection ChildFrames { get; }

        bool ShouldInvalidate { get; }

        void Invalidate();
    }
}
