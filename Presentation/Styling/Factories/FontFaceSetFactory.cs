using Sachssoft.Sasogine.Resources;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Styling.Factories;

internal class FontFaceSetFactory : ITypeFactory<FontFaceSet, Resource>
{
    public FontFaceSet Create(ResourceStore store, Resource entry)
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
            if (child is Resource resChild && resChild.TargetType == typeof(FontFace))
            {
                // File obligatorisch
                if (string.IsNullOrEmpty(resChild.File))
                    continue;

                faces.Add(store.Registry.Create<FontFace>(store, child));
            }
        }

        return new FontFaceSet(familyName, faces.ToArray());
    }
}