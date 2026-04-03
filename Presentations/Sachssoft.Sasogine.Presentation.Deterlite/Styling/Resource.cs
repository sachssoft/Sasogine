using System;
using System.Collections.Generic;
using System.IO;

#if SASOGINE
using Sachssoft.Sasogine.Resources.Loaders;
#endif

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public sealed class Resource : SkinEntry<Resource>
    {
        private Stream? _stream;

        public Resource(string? id, Type targetType, PropertySet properties, IEnumerable<Resource>? children = null) : base(id, targetType, properties, children)
        {
        }

        public ResourceFileSource Source { get; init; }

        public string? File { get; init; }

        public Stream GetStream(Skin skin)
        {
            if (_stream != null)
                return _stream;

            if (string.IsNullOrEmpty(File))
                throw new InvalidOperationException("Resource file is not set.");

            string? rootPath = Source switch
            {
                ResourceFileSource.Local => skin.Workspace.Configuration.LocalRootPath,
                ResourceFileSource.Resource => skin.Workspace.Configuration.EmbeddedRootPath,
                _ => null
            };
            string path = rootPath != null ? Path.Combine(rootPath, File) : File;

#if SASOGINE

            LoaderBase? loader = Source switch
            {
                ResourceFileSource.Local => new LocalFileLoader(path),
                ResourceFileSource.Resource => new EmbeddedResourceLoader(path),
                _ => throw new NotSupportedException($"Unsupported ResourceFileSource: {Source}")
            };

            if (loader == null)
                throw new NotSupportedException("Loader could not be created.");

            _stream = loader.GetStream();

            if (_stream == null)
                throw new InvalidOperationException($"Failed to load resource stream for file '{File}'.");

            return _stream;

#else
            throw new NotSupportedException("Resource loading not implemented for this platform.");
#endif
        }

        public T Create<T>(Skin skin)
        {
            //skin.SkinRegistry.Create(Type, Id);
            // Später
            throw new NotSupportedException();
        }
    }
}
