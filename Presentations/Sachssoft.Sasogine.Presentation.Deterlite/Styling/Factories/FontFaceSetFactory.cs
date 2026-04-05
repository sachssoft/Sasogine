using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    internal class FontFaceSetFactory : ITypeFactory<FontFaceSet, Resource>
    {
        public FontFaceSet Create(Skin skin, Resource entry)
        {
            string familyName = "";
            List<FontFace> faces = new();

            foreach (var property in entry.Properties)
            {
                switch (property.Name)
                {
                    case nameof(FontFaceSet.Name):
                        if (property.Value is string name)
                            familyName = name;
                        break;
                }
            }

            // Definierte Faces holen
            foreach (var child in entry.Children)
            {
                if (child.TargetType != typeof(FontFace))
                    continue;

                // File obligatorisch
                if (string.IsNullOrEmpty(child.File))
                    continue;

                faces.Add(skin.Registry.Create<FontFace>(skin, child));
            }

            return new FontFaceSet(familyName, faces.ToArray());
        }
    }
}
