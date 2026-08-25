namespace Sachssoft.Sasogine.Common
{
    public interface IEngineObjectDefinition : IDefinition, IEngineReferenceable
    {
        new string? Id { get; set; }

        string? Class { get; set; }
    }
}
