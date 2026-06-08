using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Assets;

public abstract class AssetDefinitionBase : IAssetDefinition
{
    public string? Id { get; set; }

    public string? Class { get; set; }
}
