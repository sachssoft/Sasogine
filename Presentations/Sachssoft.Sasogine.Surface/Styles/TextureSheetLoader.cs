using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Loaders;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;
using System.Xml.Linq;

namespace Sachssoft.Sasogine.Surface.Styles
{
    public static class TextureSheetLoader
    {
        public static TextureSheet Load<TLoader>(string filePath, LoaderOptions options, Func<string, TLoader> loaderInstance)
            where TLoader : LoaderBase
        {
            var loader = loaderInstance.Invoke(filePath);
            var doc = XDocument.Load(loader.GetStream());

            if (doc.Root == null)
                throw new Exception("Die XML-Datei ist leer oder ungültig.");

            return FromXml(doc.Root, options, loaderInstance);
        }

        private static TextureSheet FromXml<TLoader>(XElement root, LoaderOptions options, Func<string, TLoader> loaderInstance)
            where TLoader : LoaderBase
        {
            string imageFilePath = root.Attribute("Image")?.Value
                   ?? throw new Exception("Attribute 'Image' fehlt.");

            var sheet = new TextureSheet();

            sheet.SourceFilePath = imageFilePath;
            sheet.Source = Texture2DLoader.Load(imageFilePath, options, loaderInstance);

            foreach (XElement element in root.Elements())
            {
                var id = (string?)element.Attribute("Id") ?? "";

                switch (element.Name.LocalName)
                {
                    case nameof(TextureRegion):
                        var textureRegion = new TextureRegion(
                            sheet.Source,
                            new Microsoft.Xna.Framework.Rectangle()
                            {
                                X = (int?)element.Attribute("Left") ?? 0,
                                Y = (int?)element.Attribute("Top") ?? 0,
                                Width = (int?)element.Attribute("Width") ?? 0,
                                Height = (int?)element.Attribute("Height") ?? 0
                            });
                        sheet.Regions.Add(id, textureRegion);
                        break;

                    case nameof(NinePatchRegion):
                        var ninePathRegion = new NinePatchRegion(
                            sheet.Source,
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
                        sheet.Regions.Add(id, ninePathRegion);
                        break;
                }
            }

            return sheet;
        }
    }
}
