using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Models;
using Sachssoft.Sasogine.Resources;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Assets.Data;

/// <summary>
/// Defines the configuration of a keyed frame set asset.
/// </summary>
/// <remarks>
/// Specifies the document format and texture asset used to build a
/// <see cref="KeyedFrameSet"/> runtime instance.
/// </remarks>
public class KeyedFrameSetAssetDefinition : AssetDefinitionBase
{
    /// <summary>
    /// Gets or sets the document format used to load the frame set data.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Format Type")]
    public DocumentFormatType FormatType { get; set; }

    /// <summary>
    /// Gets or sets the reference to the texture asset used by the frame set.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Texture")]
    public Reference<Texture2DAsset>? Texture { get; set; }
}