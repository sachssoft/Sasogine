using Sachssoft.Engine.Components.Definitions.Inspectors;

/// <summary>Builds a Controls-independent description through the Basic inspector API.</summary>
internal sealed class InspectorViewContractDescription : InspectorBase<InspectorViewContractDefinition>
{
    public override void Build(IInspectorView view, InspectorViewContractDefinition definition)
    {
        var common = view.Group("Common", "Common");
        common.Add(nameof(definition.Title), "Title", () => definition.Title, value => definition.Title = value);
        common.Add(nameof(definition.Count), "Count", () => definition.Count, value => definition.Count = value);
        common.Add(nameof(definition.ReadOnlyCount), "Read-only count", () => definition.ReadOnlyCount);
        common.Add(nameof(definition.Color), "Color", () => definition.Color);
        common.Add(nameof(definition.Position), "Position", () => definition.Position);
    }
}
