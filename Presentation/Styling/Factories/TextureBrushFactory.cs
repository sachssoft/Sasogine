using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Presentation.Rendering;
using Sachssoft.Sasogine.Resources;
using System;

namespace Sachssoft.Sasogine.Presentation.Styling.Factories;

internal class TextureBrushFactory : ITypeFactory<TextureBrush, Resource>
{
    public TextureBrush Create(ResourceStore store, Resource entry)
    {
        Texture2D? texture = null;
        float opacity = 1f;
        ITextureRegion? region = null;

        foreach (var property in entry.Properties)
        {
            switch (property.Name)
            {
                case nameof(TextureBrush.TextureRegion):
                    if (property.Value is Binding binding)
                        region = binding.Resolve<ITextureRegion>(store);
                    break;

                case nameof(TextureBrush.Opacity):
                    if (property.Value is float o)
                        opacity = o;
                    break;
            }
        }

        if (texture == null)
            throw new InvalidOperationException("no texture");

        return new TextureBrush(texture, region, opacity);
    }
}
