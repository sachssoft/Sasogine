using Sachssoft.Sasogine.Graphics;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;
using Sachssoft.Sasogine.Presentation.Deterlite.Styling;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Rendering
{
    public interface ITextureRegion : IStylePart
    {
        void Render(Bounds bounds, IRenderContext context);
    }
}
