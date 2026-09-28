using Sachssoft.Engine.Components.Definitions;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Defines the configurable state of an elliptical arc vector segment.
/// </summary>
public class VectorArcSegmentDefinition : VectorSegmentDefinition
{
    /// <summary>
    /// Gets the first control node through which the arc passes.
    /// </summary>
    [DefinitionMember(
        Title = "Control Node 0",
        Category = Categories.Design)]
    public VectorNodeDefinition ControlNode0 { get; } = new();

    /// <summary>
    /// Gets the second control node through which the arc passes.
    /// </summary>
    [DefinitionMember(
        Title = "Control Node 1",
        Category = Categories.Design)]
    public VectorNodeDefinition ControlNode1 { get; } = new();
}
