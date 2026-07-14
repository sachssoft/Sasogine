namespace Box2D;

/// <summary>
/// Finishes a user task object that wraps a Box2D task.
/// </summary>
/// <param name="userTask">The user task</param>
/// <param name="userContext">The user context</param>
public delegate void FinishTaskCallback(nint userTask, nint userContext);