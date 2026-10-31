using Sachssoft.Engine.Assets.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Components.Definitions;
using Sachssoft.Engine.Resources;
using System.ComponentModel;
using System.Collections.Generic;

namespace Sachssoft.Engine.Assets.Data;

/// <summary>
/// Defines the configuration of a keyed frame set asset.
/// </summary>
/// <remarks>
/// Specifies the document format and texture assets used to build a
/// <see cref="KeyedFrameSet"/> runtime instance.
/// </remarks>
public class KeyedFrameSetAssetDefinition : AssetDefinitionBase
{
    /// <summary>
    /// Gets or sets the document format used to load the frame set data.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Format Type")]
    public DocumentFormatType FormatType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the fallback texture reference used when <see cref="Textures"/> is null or empty.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Texture")]
    public Reference<Texture2DAsset>? Texture
    {
        get;
        set;
    }

    /// <summary>Gets or sets ordered atlas page references, taking precedence over <see cref="Texture"/> when nonempty.</summary>
    [Category(Categories.Common)]
    [DisplayName("Textures")]
    public IList<Reference<Texture2DAsset>>? Textures
    {
        get;
        set;
    }
}
