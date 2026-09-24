using Sachssoft.Engine.Assets;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Resources.Sources;

/// <summary>
/// Provides access to resources embedded in an assembly.
/// </summary>
public sealed class EmbeddedResourceSource : ResourceSourceBase, IFileSource
{
    private Assembly _assembly = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EmbeddedResourceSource"/> class.
    /// </summary>
    public EmbeddedResourceSource()
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EmbeddedResourceSource"/> class with the specified file path.
    /// </summary>
    /// <param name="filePath">
    /// The path used to locate the embedded resource.
    /// </param>
    public EmbeddedResourceSource(string? filePath)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EmbeddedResourceSource"/> class with the specified file path
    /// and assembly.
    /// </summary>
    /// <param name="filePath">
    /// The path used to locate the embedded resource.
    /// </param>
    /// <param name="assembly">
    /// The assembly containing the embedded resource.
    /// </param>
    public EmbeddedResourceSource(
        string? filePath,
        Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        FilePath = filePath;
        Assembly = assembly;
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EmbeddedResourceSource"/> class with the specified file path
    /// and game application.
    /// </summary>
    /// <param name="filePath">
    /// The path used to locate the embedded resource.
    /// </param>
    /// <param name="application">
    /// The game application whose assembly contains the embedded resource.
    /// </param>
    public EmbeddedResourceSource(
        string? filePath,
        GameApplicationBase application)
    {
        ArgumentNullException.ThrowIfNull(application);

        FilePath = filePath;
        Assembly = application.Assembly;
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EmbeddedResourceSource"/> class with the specified file path
    /// and asset store.
    /// </summary>
    /// <param name="filePath">
    /// The path used to locate the embedded resource.
    /// </param>
    /// <param name="assetStore">
    /// The asset store whose application context provides the assembly
    /// containing the embedded resource.
    /// </param>
    public EmbeddedResourceSource(
        string? filePath,
        AssetStore assetStore)
    {
        ArgumentNullException.ThrowIfNull(assetStore);

        FilePath = filePath;
        Assembly = assetStore.GameApplication.Assembly;
    }

    /// <summary>
    /// Gets or sets the assembly containing the embedded resource.
    /// </summary>
    public Assembly Assembly
    {
        get => _assembly;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _assembly = value;
        }
    }

    /// <summary>
    /// Gets or sets the path used to locate the embedded resource.
    /// </summary>
    public string? FilePath { get; set; }

    /// <inheritdoc/>
    protected override Stream OpenStream()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            throw new InvalidOperationException(
                $"{nameof(FilePath)} is not set.");

        return AssemblyResource.Open(
            Assembly,
            FilePath);
    }

    /// <inheritdoc/>
    protected override async Task<Stream> OpenStreamAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using Stream originalStream = OpenStream();
        var memoryStream = new MemoryStream();

        try
        {
            await originalStream
                .CopyToAsync(memoryStream, cancellationToken)
                .ConfigureAwait(false);

            memoryStream.Position = 0;
            return memoryStream;
        }
        catch
        {
            memoryStream.Dispose();
            throw;
        }
    }
}