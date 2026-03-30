using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Rendering
{
    public interface IBrush
    {
        void Render(Bounds bounds, IRenderContext context);
    }
}
