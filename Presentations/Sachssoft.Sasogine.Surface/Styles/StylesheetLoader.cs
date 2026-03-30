using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Loaders;
using Sachssoft.Sasogine.Surface.Visuals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Linq;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public static class StylesheetLoader
    {
        public static Stylesheet Load<TLoader>(string filePath, LoaderOptions options, Func<string, TLoader> loaderInstance)
            where TLoader : LoaderBase
        {
            var loader = loaderInstance.Invoke(filePath);
            var doc = XDocument.Load(loader.GetStream());

            if (doc.Root == null)
                throw new Exception("Die XML-Datei ist leer oder ungültig.");

            return FromXml(doc.Root, options, loaderInstance);
        }

        private static Stylesheet FromXml<TLoader>(XElement root, LoaderOptions options, Func<string, TLoader> loaderInstance)
            where TLoader : LoaderBase
        {
            var sheet = new Stylesheet();

            LoadAtlases(sheet, root, options, loaderInstance);
            LoadFontTypefaces(sheet, root, loaderInstance);
            LoadFonts(sheet, root);
            LoadBrushes(sheet, root);
            LoadStyles(sheet, root, options, loaderInstance);

            return sheet;
        }

        private static void LoadAtlases<TLoader>(Stylesheet sheet, XElement root, LoaderOptions options, Func<string, TLoader> loaderInstance)
            where TLoader : LoaderBase
        {
            foreach (var atlasElement in root.Elements("Atlas"))
            {
                //var file = atlasElement.Attribute("File")?.Value;
                var name = atlasElement.Attribute("Name")?.Value;
                var file = name + ".atlas.xml";

                if (!string.IsNullOrWhiteSpace(file))
                {
                    string key = file;
                    if (!sheet.TextureAtlases.ContainsKey(key))
                    {
                        sheet.TextureAtlases[key] = TextureSheetLoader.Load(file, options, loaderInstance);
                    }
                }
            }
        }

        private static void LoadFontTypefaces<TLoader>(Stylesheet sheet, XElement root, Func<string, TLoader> loaderInstance)
            where TLoader : LoaderBase
        {
            foreach (var typefaceElement in root.Elements("Typeface"))
            {
                var name = typefaceElement.Attribute("Name")?.Value;

                if (string.IsNullOrWhiteSpace(name))
                    return;

                var variants = new List<FontVariant>();

                foreach (var variantElement in typefaceElement.Elements("Variant"))
                {
                    var file = variantElement.Attribute("File")?.Value;
                    var fontSystem = FontSystemLoader.Load(file, loaderInstance);

                    var variant = new FontVariant(fontSystem)
                    {
                        Weight = variantElement.Attribute("Weight")?.Value switch
                        {
                            "Normal" => FontWeight.Normal,
                            "Bold" => FontWeight.Bold,
                            _ => FontWeight.Normal
                        },
                        Style = variantElement.Attribute("Style")?.Value switch
                        {
                            "Normal" => FontStyle.Normal,
                            "Italic" => FontStyle.Italic,
                            _ => FontStyle.Normal
                        }
                    };

                    variants.Add(variant);
                }

                var family = new FontTypeface(name, variants.ToArray());
                sheet.Typefaces[name] = family;
            }
        }

        private static void LoadFonts(Stylesheet sheet, XElement root)
        {
            foreach (var fontElement in root.Elements("Font"))
            {
                var id = fontElement.Attribute("Id")?.Value ?? "";
                var name = fontElement.Attribute("Typeface")?.Value;
                var size = fontElement.Attribute("Size")?.Value;
                var weight = fontElement.Attribute("Weight")?.Value;
                var style = fontElement.Attribute("Style")?.Value;
                var decoration = fontElement.Attribute("Decoration")?.Value;

                var font = new Font()
                {
                    Typeface = name ?? "Noto",
                    Size = float.TryParse(size, CultureInfo.InvariantCulture, out var sizeResult) ? sizeResult : 12f,
                    Weight = weight switch
                    {
                        "Bold" => FontWeight.Bold,
                        _ => FontWeight.Normal
                    },
                    Style = style switch
                    {
                        "Italic" => FontStyle.Italic,
                        _ => FontStyle.Normal
                    },
                    Decoration = decoration switch
                    {
                        "Underline" => TextDecoration.Underline,
                        "StrikeThrough" => TextDecoration.StrikeThrough,
                        _ => TextDecoration.None
                    }
                };

                sheet.Fonts[id] = font;
            }
        }

        private static void LoadBrushes(Stylesheet sheet, XElement root)
        {
            foreach (var brushElement in root.Elements("Brush"))
            {
                var typeKey = brushElement.Attribute("Type")?.Value;

                if (string.IsNullOrEmpty(typeKey) || BrushRegistry.IsRegistered(typeKey))
                    return;

                var id = brushElement.Attribute("Id")?.Value ?? "";

                // ValueMap direkt aus XML-Attributen erstellen
                var map = new Dictionary<string, string?>();

                foreach (var attr in brushElement.Attributes())
                {
                    // Id und Type case-insensitive überspringen
                    if (string.Equals(attr.Name.LocalName, "Id", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(attr.Name.LocalName, "Type", StringComparison.OrdinalIgnoreCase))
                        continue;

                    map[attr.Name.LocalName] = attr.Value;
                }

                // Brush-Instanz erstellen und registrieren
                var instance = BrushRegistry.CreateInstance(sheet, typeKey, new PropertyMap(map));
                sheet.Brushes[id] = instance;
            }
        }

        private static void LoadStyles<TLoader>(Stylesheet sheet, XElement root, LoaderOptions options, Func<string, TLoader> loaderInstance)
            where TLoader : LoaderBase
        {
            foreach (var styleElement in root.Elements("Style"))
            {
                LoadStyleRecursive(sheet, styleElement);
            }
        }

        private static void LoadStyleRecursive(
            StyleMap parent,
            XElement element)
        {
            var targetTypeKey = element.Attribute("Type")?.Value;
            var targetId = element.Attribute("Id")?.Value;
            var applyForKey = element.Attribute("ApplyFor")?.Value;

            if (string.IsNullOrWhiteSpace(targetTypeKey))
                return;

            var targetTypeFound = StyleableTypeRegistry.FindType(targetTypeKey);
            var applyForFound = StyleableTypeRegistry.FindType(applyForKey);

            if (targetTypeFound == null)
                return;

            // Attribute der <Value>-Einträge auslesen
            var propertyMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var valuesElement in element.Elements("Values"))
            {
                foreach (var attr in valuesElement.Attributes())
                {
                    propertyMap[attr.Name.LocalName] = attr.Value;
                }
            }

            // Style im Stylesheet registrieren
            var child = parent.Add(targetTypeFound, targetId ?? string.Empty, applyForFound, propertyMap);

            // Kind-Styles rekursiv laden
            foreach (var childElement in element.Elements("Style"))
            {
                LoadStyleRecursive(child, childElement);
            }
        }
    }
}
