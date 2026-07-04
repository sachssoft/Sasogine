using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Presentation.Rendering
{
    public interface IBrush 
    {
        void Render(Bounds bounds, IRenderContext context);
    }
}
