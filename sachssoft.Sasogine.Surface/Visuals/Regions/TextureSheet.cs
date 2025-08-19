using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Sachssoft.Sasogine.Surface.Visuals.Regions
{
    public class TextureSheet
    {
        public string? SourceFilePath { get; set; }

        public Texture2D? Source { get; set; }

        public TextureSheet()
        {
        }

        public Dictionary<string, ITextureRegion> Regions { get; } = new Dictionary<string, ITextureRegion>();

        public void LoadXml(Stream stream, Func<string, Texture2D> textureLoaderFunc)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            XDocument doc = XDocument.Load(stream);
            XElement root = doc.Root ?? throw new InvalidDataException("Kein Root-Element gefunden");

            string imageFilePath = root.Attribute("Image")?.Value
                   ?? throw new Exception("Attribute 'Image' fehlt.");

            SourceFilePath = imageFilePath;
            Source = textureLoaderFunc(imageFilePath);

            foreach (XElement element in root.Elements())
            {
                var id = (string?)element.Attribute("Id") ?? "";

                switch (element.Name.LocalName)
                {
                    case nameof(TextureRegion):
                        var textureRegion = new TextureRegion(
                            Source, 
                            new Microsoft.Xna.Framework.Rectangle()
                            {
                                X = (int?)element.Attribute("Left") ?? 0,
                                Y = (int?)element.Attribute("Top") ?? 0,
                                Width = (int?)element.Attribute("Width") ?? 0,
                                Height = (int?)element.Attribute("Height") ?? 0
                            });
                        Regions.Add(id, textureRegion);
                        break;

                    case nameof(NinePatchRegion):
                        var ninePathRegion = new NinePatchRegion(
                            Source, 
                            new Microsoft.Xna.Framework.Rectangle()
                            {
                                X = (int?)element.Attribute("Left") ?? 0,
                                Y = (int?)element.Attribute("Top") ?? 0,
                                Width = (int?)element.Attribute("Width") ?? 0,
                                Height = (int?)element.Attribute("Height") ?? 0
                            },
                            left: (int?)element.Attribute("NinePatchLeft") ?? 0,
                            right: (int?)element.Attribute("NinePatchRight") ?? 0,
                            top: (int?)element.Attribute("NinePatchTop") ?? 0,
                            bottom: (int?)element.Attribute("NinePatchBottom") ?? 0);
                        Regions.Add(id, ninePathRegion);
                        break;
                }
            }
        }

        public void SaveXml(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var root = new XElement("TextureAtlas",
                new XAttribute("Image", SourceFilePath ?? "unknown") // optional: Name speichern
            );

            foreach (var kvp in Regions)
            {
                string id = kvp.Key;
                var region = kvp.Value;

                if (region is NinePatchRegion ninePatch)
                {
                    // Wir speichern nur die ursprünglichen Bounds + Insets, nicht die 9 Subregions
                    root.Add(new XElement("NinePatchRegion",
                        new XAttribute("Id", id),
                        new XAttribute("Left", ninePatch.Bounds.X),
                        new XAttribute("Top", ninePatch.Bounds.Y),
                        new XAttribute("Width", ninePatch.Bounds.Width),
                        new XAttribute("Height", ninePatch.Bounds.Height),
                        new XAttribute("NinePatchLeft", ninePatch.Left),
                        new XAttribute("NinePatchTop", ninePatch.Top),
                        new XAttribute("NinePatchRight", ninePatch.Right),
                        new XAttribute("NinePatchBottom", ninePatch.Bottom)
                    ));
                }
                else if (region is TextureRegion texRegion)
                {
                    root.Add(new XElement("TextureRegion",
                        new XAttribute("Id", id),
                        new XAttribute("Left", texRegion.Bounds.X),
                        new XAttribute("Top", texRegion.Bounds.Y),
                        new XAttribute("Width", texRegion.Bounds.Width),
                        new XAttribute("Height", texRegion.Bounds.Height)
                    ));
                }
            }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            doc.Save(stream);
        }


    }
}
