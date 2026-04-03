using FontStashSharp;
using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    internal class FontTypefaceFactory : ITypeFactory<FontTypeface, Definition>
    {
        public FontTypeface Create(Skin skin, Definition definition)
        {
            string name = "";
            List<FontFamily> faces = new();

            foreach(var property in definition.Properties)
            {
                switch(property.Name)
                {
                    case nameof(FontTypeface.Name):
                        if (property.Value is string sName)
                            name = sName;
                        break;
                }
            }

            // Untere Definitionen für Varianten
            foreach(var subDefinition in definition.Definitions)
            {
                string? resource = "";
                FontWeight definedWeight = FontWeight.Normal;
                FontStyle definedStyle = FontStyle.Normal;

                foreach (var property in subDefinition.Properties)
                {
                    switch (property.Name)
                    {
                        case nameof(FontFamily.Face):
                            if (property.Value is string res)
                                resource = res;
                            break;
                        case nameof(FontFamily.Weight):
                            if (property.Value is FontWeight weight)
                                definedWeight = weight;
                            break;
                        case nameof(FontFamily.Style):
                            if (property.Value is FontStyle style)
                                definedStyle = style;
                            break;
                    }
                }

                var skinResource = skin.FindResource(resource);

                if (skinResource != null)
                {
                    var fontFace = skinResource.Create<FontFace>(skin) ??
                        throw new InvalidOperationException("FontFace not found");

                    var face = new FontFamily(fontFace, definedWeight, definedStyle);
                }
            }

            return new FontTypeface(name, faces.ToArray());
        }
    }
}
