namespace Sachssoft.Sasogine.Presentation.Deterlite.Basic
{
    public interface IFrameChildHost
    {
        FrameCollection ChildFrames { get; }

        bool ShouldInvalidate { get; }

        void Invalidate();
    }
}
