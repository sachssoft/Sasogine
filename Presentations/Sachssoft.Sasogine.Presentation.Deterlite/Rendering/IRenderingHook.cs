namespace Sachssoft.Sasogine.Presentation.Rendering
{
    public interface IRenderingHook
    {
        IFrameHook? RenderHook { get; set; } // Iteration

        IFrameHook? FrameHook { get; set; } // Nach Render 
    }
}
