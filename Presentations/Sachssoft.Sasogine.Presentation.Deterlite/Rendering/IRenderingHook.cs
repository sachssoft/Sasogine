namespace Sachssoft.Sasogine.Presentation.Deterlite.Rendering
{
    public interface IRenderingHook
    {
        IFrameHook? RenderHook { get; set; }

        IFrameHook? FrameHook { get; set; }
    }
}
