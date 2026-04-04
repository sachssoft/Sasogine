using Sachssoft.Sasogine.Common.Performance;
using Sachssoft.Sasogine.Presentation.Rendering;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    internal class FontFaceFactory : ITypeFactory<FontFace, Resource>
    {
        public FontFace Create(Skin skin, Resource entry)
        {
            FontWeight definedWeight = FontWeight.Normal;
            FontStyle definedStyle = FontStyle.Normal;

            foreach (var property in entry.Properties)
            {
                switch (property.Name)
                {
                    case nameof(FontFace.Weight):
                        if (property.Value is FontWeight weight)
                            definedWeight = weight;
                        break;
                    case nameof(FontFace.Style):
                        if (property.Value is FontStyle style)
                            definedStyle = style;
                        break;
                }
            }

            return new FontFace()
            {
                Resource = entry,
                Weight = definedWeight,
                Style = definedStyle
            };
        }
    }
}
