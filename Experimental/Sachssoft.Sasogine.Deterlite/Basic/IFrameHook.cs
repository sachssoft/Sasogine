namespace Sachssoft.Sasogine.Presentation
{
    public interface IFrameHook
    {
        void Call(FrameBase frame, FrameContext context);
    }
}
