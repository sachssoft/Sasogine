using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

/// <summary>
/// Represents an engine object that can be selected by a selection tool.
/// </summary>
public interface ISelectionTarget : IEngineObject
{
    /// <summary>
    /// Gets a value indicating whether the selection target is selected.
    /// </summary>
    bool IsSelected { get; }

    /// <summary>
    /// Gets a value indicating whether the selection target is locked.
    /// </summary>
    bool IsLocked { get; }
}