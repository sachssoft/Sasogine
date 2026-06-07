using System;

namespace Sachssoft.Sasogine.Resources;

public enum ResourceSourceType
{
    ExternalFile,
    EmbeddedResource,

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
