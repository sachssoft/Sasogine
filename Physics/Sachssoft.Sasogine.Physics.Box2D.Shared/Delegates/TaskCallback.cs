namespace Box2D;

/// <summary>
/// <b>Task interface</b><br/>
/// This is prototype for a Box2D task. Your task system is expected to invoke the Box2D task with these arguments.<br/>
/// The task spans a range of the parallel-for: [startIndex, endIndex)<br/>
/// The worker index must correctly identify each worker in the user thread pool, expected in [0, workerCount).<br/>
/// A worker must only exist on only one thread at a time and is analogous to the thread index.<br/>
/// The task context is the context pointer sent from Box2D when it is enqueued.<br/>
/// The startIndex and endIndex are expected in the range [0, itemCount) where itemCount is the argument to EnqueueTaskCallback
/// below. Box2D expects startIndex &lt; endIndex and will execute a loop like this:
/// <code>
/// for (int i = startIndex; i &lt; endIndex; ++i)<br/>
/// {<br/>
///     DoWork();<br/>
/// }<br/>
/// </code>
/// </summary>
/// <param name="startIndex">The start index of the task</param>
/// <param name="endIndex">The end index of the task</param>
/// <param name="workerIndex">The worker index</param>
/// <param name="taskContext">The task context</param>
public delegate void TaskCallback(int startIndex, int endIndex, uint workerIndex, nint taskContext);