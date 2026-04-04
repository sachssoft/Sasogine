using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using Sachssoft.Sasogine.Resources;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling.Factories
{
    internal class TextureBrushFactory : ITypeFactory<TextureBrush, Resource>
    {
        public TextureBrush Create(Skin skin, Resource entry)
        {

            float opacity = 1f;
            ITextureRegion? region = null;

            foreach (var property in entry.Properties)
            {
                switch (property.Name)
                {
                    case nameof(TextureBrush.TextureRegion):
                        if (property.Value is StyleBinding binding)
                            region = binding.Resolve<ITextureRegion, ITextureRegionResource>(skin);
                        break;

                    case nameof(TextureBrush.Opacity):
                        if (property.Value is float o)
                            opacity = o;
                        break;
                }
            }

            return new TextureBrush(region, opacity);
        }
    }
}
