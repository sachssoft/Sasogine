using Sachssoft.Sasogine.Assets.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Models;
using Sachssoft.Sasogine.Resources;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Assets.Data;

/// <summary>
/// Defines an indexed frame set asset.
/// </summary>
/// <remarks>
/// An indexed frame set associates numeric frame indices with enum value names
/// and uses a texture asset as the source texture for the runtime frame set.
/// </remarks>
public class IndexedFrameSetDefinition : AssetDefinitionBase
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

    /// <summary>
    /// Gets or sets the mappings between numeric frame indices and enum value names.
    /// </summary>
    [Category(Categories.Common)]
    [DisplayName("Index Mapping")]
    public IList<IndexMapping>? IndexMapping { get; set; }
}