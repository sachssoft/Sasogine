using Sachssoft.Engine.Components.Tools;

namespace Sachssoft.Engine.Controls;

/// <summary>Describes the lifecycle of one editable canvas transformation.</summary>
public sealed class CanvasTransformEventArgs : EventArgs
{
    /// <summary>Creates a notification for the supplied item and gesture.</summary>
    /// <param name="item">The item being transformed.</param>
    /// <param name="operation">The gesture being performed.</param>
    /// <param name="state">The current gesture lifecycle state.</param>
    /// <param name="initialState">The snapshot captured at gesture start; recipients must not update it.</param>
    internal CanvasTransformEventArgs(CanvasItem item, CanvasEditOperation operation,
        SelectionTransformState state, Transform2State initialState)
    {
        Item = item;
        Operation = operation;
        State = state;
        InitialState = initialState;
    }

    /// <summary>Gets the transformed item.</summary>
    public CanvasItem Item
    {
        get;
    }

    /// <summary>Gets the gesture being performed.</summary>
    public CanvasEditOperation Operation
    {
        get;
    }

    /// <summary>Gets whether the gesture started, changed, completed or was cancelled.</summary>
    public SelectionTransformState State
    {
        get;
    }

    /// <summary>Gets the original transform for an undo transaction. Do not call UpdateState on this snapshot.</summary>
    public Transform2State InitialState
    {
        get;
    }
}
