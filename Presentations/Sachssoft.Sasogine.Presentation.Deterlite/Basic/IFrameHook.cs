namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public interface IFrameHook
    {
        void Call(FrameBase frame, FrameContext context);
    }
}
