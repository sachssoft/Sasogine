namespace Sachssoft.Sasogine.Presentation.Deterlite.Rendering
{
    public interface IRenderingHook
    {
        IFrameHook? RenderHook { get; set; } // Iteration

        IFrameHook? FrameHook { get; set; } // Nach Render 
    }
}
