using Sachssoft.Sasogine.Presentation.Deterlite.Styling.Assets;
using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Loaders;
using System;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    internal class TextureAtlasResource : IResourceFactory
    {
        public object Create(Type targetType, Skin skin, PropertyMap properties)
        {
            string? file = null;
            ResourceLoaderType source = ResourceLoaderType.ExternalFile;
            float scaleDefinition = 1f;


            foreach (var property in properties)
            {
                switch (property.Name)
                {
                    case nameof(File):
                        if (property.Value is string strFile)
                            file = strFile;
                        break;
                    case nameof(Source):
                        if (property.Value is ResourceLoaderType enSource)
                            source = enSource;
                        break;
                    case nameof(ScaleDefinition):
                        if (property.Value is float fScale)
                            scaleDefinition = fScale;
                        break;
                }
            }

            file = Path.Combine(rootPath, file);

#if SASOGINE
            LoaderBase? loader = null;

            switch (source)
            {
                case FileSource.Local:
                    loader = new LocalFileLoader(file);
                    break;
                case FileSource.Resource:
                    loader = new EmbeddedResourceLoader(file);
                    break;
            }
#else
            switch (source)
            {
                case FileSource.Local:
                    _textureAtlas = TextureAtlas.FromFile(file);
                    break;
                case FileSource.Resource:
                    _textureAtlas = TextureAtlas.FromResource(file);
                    break;
            }
#endif

        }
    }
}
