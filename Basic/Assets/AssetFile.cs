using System;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    public sealed class AssetFile<T> : IAssetFile
        where T : class, IAsset
    {
        private bool _wasResolved;
        private T? _asset;
        private Exception? _error;

        public AssetFile(string? relativeFilePath)
        {
            if (string.IsNullOrWhiteSpace(relativeFilePath))
                throw new ArgumentException("Invalid asset file path.", nameof(relativeFilePath));

            // Plattform unabhängig machen
            relativeFilePath = relativeFilePath.Replace('\\', '/');

            if (relativeFilePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException("Asset path contains invalid characters.", nameof(relativeFilePath));

            var fileName = Path.GetFileName(relativeFilePath);

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new ArgumentException("Asset filename contains invalid characters.", nameof(relativeFilePath));

            FullRelativePath = relativeFilePath;

            Name = Path.GetFileNameWithoutExtension(relativeFilePath);

            var directory = Path.GetDirectoryName(relativeFilePath);
            RelativePath = directory?.Replace('\\', '/') ?? string.Empty;
        }

        public Type AssetType => typeof(T);

        public string Name { get; }

        public string RelativePath { get; }

        public string FullRelativePath { get; }

        public bool IsUnknown { get; private set; }

        public Exception? Error => _error;

        public T? Resolve(Stream stream, IAssetResolverProvider resolver)
        {
            if (_wasResolved)
                return _asset;

            _wasResolved = true;

            try
            {
                var result = resolver.Resolve(stream);

                if (result is T typedAsset)
                {
                    _asset = typedAsset;
                }
                else
                {
                    IsUnknown = true;
                }
            }
            catch (Exception ex)
            {
                _error = ex;
            }

            return _asset;
        }

        IAsset? IAssetFile.Resolve(Stream stream, IAssetResolverProvider resolver)
        {
            return Resolve(stream, resolver);
        }

        public IAssetFile Clone()
        {
            return new AssetFile<T>(RelativePath);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}