namespace Sachssoft.Engine.Components.Definitions.Inspectors;

/// <summary>
/// Describes the editable properties of an object.
/// </summary>
public interface IInspector
{
    /// <summary>
    /// Builds the property description for the supplied source.
    /// </summary>
    void Build(IInspectorView view, object source);
}