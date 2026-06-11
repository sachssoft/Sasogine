using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Assets
{
    public interface IAssetDefinition : IEngineObjectDefinition
    {
        string? RelativePath { get; set; } 
    }
}
