namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Demonstrates a registered object excluded by a texture-reference filter.</summary>
internal sealed class AssetExampleObject : IEngineReferenceable
{
    internal AssetExampleObject(string id) => Id = id;
    public string Id { get; }
}
