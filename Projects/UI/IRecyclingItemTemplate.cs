namespace Sachssoft.Engine.Controls;

/// <summary>Creates item controls that can present different data items during their lifetime.</summary>
public interface IRecyclingItemTemplate : ITemplate<object?, Control>
{
    /// <summary>Updates an existing presentation after its data context has changed.</summary>
    /// <param name="control">The presentation returned by this template.</param>
    /// <param name="item">The next data item.</param>
    void Rebind(Control control, object? item);
}
