using Sachssoft.Engine.Common.Collections;
using Sachssoft.Engine.Resources;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sachssoft.Engine.Assets;

/// <summary>
/// Provides synchronization between resource files and asset definitions.
/// </summary>
public static class AssetSynchronization
{
    /// <summary>
    /// Synchronizes asset definitions with a local asset directory.
    /// </summary>
    /// <param name="definitions">
    /// The asset definitions to synchronize.
    /// </param>
    /// <param name="assetsPath">
    /// The local asset directory.
    /// </param>
    /// <param name="searchPattern">
    /// The search pattern used to filter files.
    /// </param>
    /// <param name="recursive">
    /// Whether files in subdirectories are included.
    /// </param>
    /// <param name="resolver">
    /// The asset resolver used to resolve asset definitions.
    /// </param>
    public static void SynchronizeLocal(
        IList<IAssetDefinition> definitions,
        string assetsPath,
        string searchPattern = "*",
        bool recursive = true,
        AssetResolver? resolver = null)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentException.ThrowIfNullOrWhiteSpace(assetsPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        var source = new LocalFileSynchronizationSource(
            assetsPath);

        Synchronize(
            definitions,
            source,
            string.Empty,
            searchPattern,
            recursive,
            resolver);
    }

    /// <summary>
    /// Synchronizes asset definitions with an asset package.
    /// </summary>
    /// <param name="definitions">
    /// The asset definitions to synchronize.
    /// </param>
    /// <param name="packagePath">
    /// The asset package path.
    /// </param>
    /// <param name="searchPattern">
    /// The search pattern used to filter files.
    /// </param>
    /// <param name="recursive">
    /// Whether files in subdirectories are included.
    /// </param>
    /// <param name="resolver">
    /// The asset resolver used to resolve asset definitions.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// Package synchronization is not currently supported.
    /// </exception>
    public static void SynchronizePackage(
        IList<IAssetDefinition> definitions,
        string packagePath,
        string searchPattern = "*",
        bool recursive = true,
        AssetResolver? resolver = null)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        throw new NotSupportedException(
            "Package synchronization is not currently supported.");
    }

    /// <summary>
    /// Synchronizes asset definitions with a file synchronization source.
    /// </summary>
    /// <param name="definitions">
    /// The asset definitions to synchronize.
    /// </param>
    /// <param name="source">
    /// The file synchronization source.
    /// </param>
    /// <param name="path">
    /// The relative resource path.
    /// </param>
    /// <param name="searchPattern">
    /// The search pattern used to filter files.
    /// </param>
    /// <param name="recursive">
    /// Whether files in subdirectories are included.
    /// </param>
    /// <param name="resolver">
    /// The asset resolver used to resolve asset definitions.
    /// </param>
    public static void Synchronize(
        IList<IAssetDefinition> definitions,
        IFileSynchronizationSource source,
        string path,
        string searchPattern = "*",
        bool recursive = true,
        AssetResolver? resolver = null)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchPattern);

        resolver ??= new AssetResolver();

        source.Synchronize(path);

        RemoveMissing(
            definitions,
            source);

        AddMissing(
            definitions,
            source,
            path,
            searchPattern,
            recursive,
            resolver);
    }

    private static void RemoveMissing(
        IList<IAssetDefinition> definitions,
        IFileSynchronizationSource source)
    {
        for (var i = definitions.Count - 1; i >= 0; i--)
        {
            var definition = definitions[i];
            var file = definition.File;

            if (file is null)
            {
                definitions.RemoveAt(i);
                continue;
            }

            if (!source.FileExists(file.FullRelativePath))
                definitions.RemoveAt(i);
        }
    }

    private static void AddMissing(
        IList<IAssetDefinition> definitions,
        IFileSynchronizationSource source,
        string path,
        string searchPattern,
        bool recursive,
        AssetResolver resolver)
    {
        foreach (var filePath in source.GetFiles(
            path,
            searchPattern,
            recursive))
        {
            if (ContainsFile(
                definitions,
                filePath))
            {
                continue;
            }

            using var stream = source.OpenStream(
                filePath);

            var definition = resolver.ResolveDefinition(
                filePath,
                stream);

            definitions.Add(definition);
        }
    }

    private static bool ContainsFile(
        IList<IAssetDefinition> definitions,
        string filePath)
    {
        foreach (var definition in definitions)
        {
            var file = definition.File;

            if (file is null)
                continue;

            if (string.Equals(
                NormalizePath(file.FullRelativePath),
                NormalizePath(filePath),
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }
}