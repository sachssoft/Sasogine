using System;

namespace Sachssoft.Sasogine.Resources;

/// <summary>
/// Defines the source type used to load a resource.
/// </summary>
public enum ResourceSourceType
{
    /// <summary>
    /// Loads the resource from an external file.
    /// </summary>
    ExternalFile,


    /// <summary>
    /// Loads the resource from an embedded assembly resource.
    /// </summary>
    EmbeddedResource,


    /// <summary>
    /// Loads the resource using MonoGame's content pipeline.
    /// </summary>
    /// <remarks>
    /// This option is not compatible with AOT compilation or trimming,
    /// because MonoGame's <see cref="Microsoft.Xna.Framework.Content.ContentManager"/>
    /// relies on reflection and dynamic code paths.
    /// </remarks>
    [Obsolete(
        message:
            "Content loader is NOT AOT-/Trimmer-safe!\n" +
            "This option uses MonoGame's ContentManager, which relies on reflection and dynamic code.\n" +
            "Do NOT use this in AOT builds or trimmed deployments.\n" +
            "Use ExternalFile or EmbeddedResource instead for fully AOT/trimmer-friendly loading.",
        DiagnosticId = "MM001"
    )]
    Content
}