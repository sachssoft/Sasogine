using Sachssoft.Sasogine.Presentation.Rendering;
using Sachssoft.Sasogine.Presentation.Styling.Factories;
using Sachssoft.Sasogine.Resources;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public sealed class StyleRegistry : ResourceRegistry
    {
        protected override void RegisterDefaults()
        {
            base.RegisterDefaults();

            Register<FontFace, FontFaceFactory>();
            Register<FontFaceSet, FontFaceSetFactory>();
            Register<Font, FontFactory>();
            Register<SolidColorBrush, SolidColorBrushFactory>();
            Register<TextureBrush, TextureBrushFactory>();
        }
    }
}