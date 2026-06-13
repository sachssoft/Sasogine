using Sachssoft.Sasogine.Common.Models;

namespace Sachssoft.Sasogine.Common
{
    public interface IEngineObjectDefinition : IEngineReferenceable
    {
        new string? Id { get; set; }

        string? Class { get; set; }
    }
}
