using System.Collections.Generic;
using System.IO;

namespace Sachssoft.Sasogine.Assets
{
    /// <summary>
    /// Provides functionality to synchronize assets from a directory.
    /// </summary>
    public static class AssetSynchronization
    {
        /// <summary>
        /// Synchronizes assets from the specified directory.
        /// </summary>
        /// <param name="assetsPath">
        /// Root directory containing the assets.
        /// </param>
        /// <param name="resolver">
        /// Asset resolver used to detect asset types.
        /// </param>
        /// <returns>
        /// A collection of resolved asset files.
        /// </returns>
        public static IEnumerable<IAssetFile> Synchronize(
            string assetsPath,
            AssetResolver? resolver = null)
        {
            if (string.IsNullOrEmpty(assetsPath) || !Directory.Exists(assetsPath))
                yield break;

            resolver ??= new AssetResolver();

            var files = Directory.GetFiles(
                assetsPath,
                "*.*",
                SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var relativePath = System.IO.Path.GetRelativePath(
                    assetsPath,
                    file);

                using var stream = File.OpenRead(file);

                var asset = resolver.Resolve(
                    relativePath,
                    stream);

                if (asset != null)
                    yield return asset;
            }
        }
    }
}