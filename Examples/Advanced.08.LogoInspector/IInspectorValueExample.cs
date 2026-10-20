namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Builds one explicitly typed value example without type discovery.</summary>
internal interface IInspectorValueExample
{
    string Name { get; }

    Control Build(InspectorView inspector);
}
