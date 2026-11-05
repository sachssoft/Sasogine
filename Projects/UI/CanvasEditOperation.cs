namespace Sachssoft.Engine.Controls;

/// <summary>Identifies an editor gesture on a canvas item.</summary>
public enum CanvasEditOperation
{
    /// <summary>No gesture is active.</summary>
    None,

    /// <summary>Changes position in canvas coordinates.</summary>
    Move,

    /// <summary>Changes layout dimensions.</summary>
    Resize,

    /// <summary>Changes visual scale factors.</summary>
    Scale,

    /// <summary>Changes rotation around the pivot.</summary>
    Rotate,

    /// <summary>Moves the pivot while preserving the visual placement.</summary>
    Pivot
}
