using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sachssoft.Engine.Resources;

/// <summary>
/// Provides utility methods for locating and accessing resources
/// embedded in an assembly.
/// </summary>
public static class AssemblyResource
{
    /// <summary>
    /// Gets all embedded resource names from the specified assembly.
    /// </summary>
    /// <param name="assembly">
    /// The assembly containing the embedded resources.
    /// </param>
    /// <returns>
    /// An array containing all normalized embedded resource names.
    /// </returns>
    public static string[] GetNames(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly
            .GetManifestResourceNames()
            .Select(NormalizeFilePath)
            .ToArray();
    }

    /// <summary>
    /// Gets all embedded resource names below the specified root directory.
    /// </summary>
    /// <param name="assembly">
    /// The assembly containing the embedded resources.
    /// </param>
    /// <param name="rootDirectory">
    /// The root directory used to locate resource names.
    /// </param>
    /// <returns>
    /// An array containing the normalized resource names including the
    /// specified root directory.
    /// </returns>
    public static string[] GetNames(
        Assembly assembly,
        string rootDirectory)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        string normalizedRoot = NormalizeFilePath(rootDirectory).Trim('/');
        string manifestRoot = ToManifestPath(normalizedRoot);
        string marker = manifestRoot + ".";

        return assembly
            .GetManifestResourceNames()
            .Select(name => GetNormalizedName(name, marker, normalizedRoot))
            .Where(name => name is not null)
            .Select(name => name!)
            .ToArray();
    }

    /// <summary>
    /// Determines whether the specified embedded resource exists.
    /// </summary>
    /// <param name="assembly">
    /// The assembly containing the embedded resource.
    /// </param>
    /// <param name="filePath">
    /// The file path used to locate the embedded resource.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the resource exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Exists(
        Assembly assembly,
        string filePath)
    {
        return FindName(assembly, filePath) is not null;
    }

    /// <summary>
    /// Determines whether the specified embedded resource exists below
    /// the specified root directory.
    /// </summary>
    /// <param name="assembly">
    /// The assembly containing the embedded resource.
    /// </param>
    /// <param name="rootDirectory">
    /// The root directory containing the resource.
    /// </param>
    /// <param name="filePath">
    /// The file path relative to the root directory.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the resource exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Exists(
        Assembly assembly,
        string rootDirectory,
        string filePath)
    {
        return FindName(assembly, rootDirectory, filePath) is not null;
    }

    /// <summary>
    /// Finds the resource name associated with the specified file path.
    /// </summary>
    /// <param name="assembly">
    /// The assembly containing the embedded resource.
    /// </param>
    /// <param name="filePath">
    /// The file path used to locate the embedded resource.
    /// </param>
    /// <returns>
    /// The normalized resource name, or <see langword="null"/>
    /// if no matching resource exists.
    /// </returns>
    public static string? FindName(
        Assembly assembly,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string normalizedFile = NormalizeFilePath(filePath);

        return GetNames(assembly)
            .FirstOrDefault(name =>
                name.EndsWith(
                    normalizedFile,
                    StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds the resource name associated with the specified file path
    /// below the specified root directory.
    /// </summary>
    /// <param name="assembly">
    /// The assembly containing the embedded resource.
    /// </param>
    /// <param name="rootDirectory">
    /// The root directory containing the resource.
    /// </param>
    /// <param name="filePath">
    /// The file path relative to the root directory.
    /// </param>
    /// <returns>
    /// The normalized resource name, or <see langword="null"/>
    /// if no matching resource exists.
    /// </returns>
    public static string? FindName(
        Assembly assembly,
        string rootDirectory,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string normalizedPath = Combine(rootDirectory, filePath);

        return GetNames(assembly, rootDirectory)
            .FirstOrDefault(name =>
                string.Equals(
                    name,
                    normalizedPath,
                    StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Opens the specified embedded resource as a stream.
    /// </summary>
    /// <param name="assembly">
    /// The assembly containing the embedded resource.
    /// </param>
    /// <param name="filePath">
    /// The file path used to locate the embedded resource.
    /// </param>
    /// <returns>
    /// A readable stream containing the embedded resource data.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// No embedded resource matching <paramref name="filePath"/> was found.
    /// </exception>
    /// <exception cref="IOException">
    /// The embedded resource was found but could not be opened.
    /// </exception>
    public static Stream Open(
        Assembly assembly,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string normalizedPath = NormalizeFilePath(filePath);
        string manifestPath = ToManifestPath(normalizedPath);

        string? manifestName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name =>
                name.EndsWith(
                    manifestPath,
                    StringComparison.OrdinalIgnoreCase));

        if (manifestName is null)
            throw CreateNotFoundException(
                assembly,
                normalizedPath);

        return assembly.GetManifestResourceStream(manifestName) ??
            throw new IOException(
                $"Failed to open embedded resource stream: {normalizedPath}");
    }

    /// <summary>
    /// Opens the specified embedded resource below the specified root directory
    /// as a stream.
    /// </summary>
    /// <param name="assembly">
    /// The assembly containing the embedded resource.
    /// </param>
    /// <param name="rootDirectory">
    /// The root directory containing the resource.
    /// </param>
    /// <param name="filePath">
    /// The file path relative to the root directory.
    /// </param>
    /// <returns>
    /// A readable stream containing the embedded resource data.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// No embedded resource matching <paramref name="filePath"/> was found.
    /// </exception>
    /// <exception cref="IOException">
    /// The embedded resource was found but could not be opened.
    /// </exception>
    public static Stream Open(
        Assembly assembly,
        string rootDirectory,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string normalizedPath = Combine(
            rootDirectory,
            filePath);

        string manifestPath = ToManifestPath(normalizedPath);

        string? manifestName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name =>
                name.EndsWith(
                    manifestPath,
                    StringComparison.OrdinalIgnoreCase));

        if (manifestName is null)
            throw CreateNotFoundException(
                assembly,
                normalizedPath,
                rootDirectory);

        return assembly.GetManifestResourceStream(manifestName) ??
            throw new IOException(
                $"Failed to open embedded resource stream: {normalizedPath}");
    }

    /// <summary>
    /// Combines a root directory and file path into a normalized
    /// embedded resource path.
    /// </summary>
    /// <param name="rootDirectory">
    /// The root directory.
    /// </param>
    /// <param name="filePath">
    /// The file path relative to the root directory.
    /// </param>
    /// <returns>
    /// The normalized combined resource path.
    /// </returns>
    public static string Combine(
        string rootDirectory,
        string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string root = NormalizeFilePath(rootDirectory).Trim('/');
        string path = NormalizeFilePath(filePath).Trim('/');

        if (path.Equals(
            root,
            StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(
                root + "/",
                StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        return $"{root}/{path}";
    }

    /// <summary>
    /// Normalizes a file path for embedded resource lookup and output.
    /// </summary>
    /// <param name="filePath">
    /// The file path to normalize.
    /// </param>
    /// <returns>
    /// The normalized file path using forward slashes.
    /// </returns>
    public static string NormalizeFilePath(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        return filePath
            .Replace('\\', '/')
            .Trim('/')
            .ToLowerInvariant();
    }

    private static string ToManifestPath(string filePath)
    {
        return NormalizeFilePath(filePath)
            .Replace('/', '.');
    }

    private static string ManifestToFilePath(string manifestPath)
    {
        int extensionIndex = manifestPath.LastIndexOf('.');

        if (extensionIndex < 0)
            return manifestPath.Replace('.', '/');

        string path = manifestPath[..extensionIndex].Replace('.', '/');
        string extension = manifestPath[extensionIndex..];

        return path + extension;
    }

    private static string? GetNormalizedName(
        string manifestName,
        string manifestRootMarker,
        string normalizedRoot)
    {
        string normalizedManifest = manifestName.ToLowerInvariant();

        int index = normalizedManifest.IndexOf(
            manifestRootMarker,
            StringComparison.Ordinal);

        if (index < 0)
            return null;

        string relativeManifestName = normalizedManifest[
            (index + manifestRootMarker.Length)..];

        string relativePath = ManifestToFilePath(relativeManifestName);

        return $"{normalizedRoot}/{relativePath}";
    }

    private static Stream OpenNormalizedResource(
        Assembly assembly,
        string normalizedName)
    {
        string manifestPath = ToManifestPath(normalizedName);

        string? manifestName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name =>
                name.EndsWith(
                    manifestPath,
                    StringComparison.OrdinalIgnoreCase));

        if (manifestName is null)
            throw new FileNotFoundException(
                $"Embedded resource not found: {normalizedName}");

        return assembly.GetManifestResourceStream(manifestName) ??
            throw new IOException(
                $"Failed to open embedded resource stream: {normalizedName}");
    }

    private static FileNotFoundException CreateNotFoundException(
        Assembly assembly,
        string resourcePath,
        string? rootDirectory = null)
    {
        string[] resourceNames = string.IsNullOrWhiteSpace(rootDirectory)
            ? GetNames(assembly)
            : GetNames(assembly, rootDirectory);

        string availableResources = string.Join(
            Environment.NewLine + "  ",
            resourceNames);

        return new FileNotFoundException(
            $"Embedded resource not found: {NormalizeFilePath(resourcePath)}" +
            Environment.NewLine +
            $"Available resources:{Environment.NewLine}  " +
            availableResources);
    }
}