using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Loaders;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Sasogine.Presentation.Styling;

public sealed class Resource : SkinEntry<Resource>
{
    private Stream? _stream;

    public Resource(string? id, Type targetType, PropertySet properties, IEnumerable<Resource>? children = null) : base(id, targetType, properties, children)
    {
    }

    public ResourceFileSource Source { get; init; }

    public string? File { get; init; }

    public Stream GetStream(string? rootPath = null)
    {
        if (_stream != null)
            return _stream;

        if (string.IsNullOrEmpty(File))
            throw new InvalidOperationException("Resource file is not set.");

        //string path = rootPath != null ? Path.Combine(rootPath, File) : File;

        //LoaderBase? loader = Source switch
        //{
        //    ResourceFileSource.Local => new LocalFileLoader(path),
        //    ResourceFileSource.Resource => new EmbeddedResourceLoader(path),
        //    _ => throw new NotSupportedException($"Unsupported ResourceFileSource: {Source}")
        //};

        //if (loader == null)
        //    throw new NotSupportedException("Loader could not be created.");

        //_stream = loader.GetStream();

        _stream = Source.OpenStream(File, rootPath);

        if (_stream == null)
            throw new InvalidOperationException($"Failed to load resource stream for file '{File}'.");

        return _stream;
    }

    public T Create<T>(Skin skin)
    {
        //skin.SkinRegistry.Create(Type, Id);
        // Später
        throw new NotSupportedException();
    }
}
