using System;
using System.Collections;
using System.Collections.Generic;

namespace sachssoft.Sasogine.Features;

public class GameDispatcher
{
    private static readonly Queue<Action> _queue = new();
    private static readonly object _lock = new();

    public void Invoke(Action action)
    {
        if (action == null) return;

        lock (_lock)
        {
            _queue.Enqueue(action);
        }
    }

    public void ExecutePending()
    {
        Queue<Action> localQueue;

        lock (_lock)
        {
            if (_queue.Count == 0)
                return;

            localQueue = new Queue<Action>(_queue);
            _queue.Clear();
        }

        while (localQueue.Count > 0)
        {
            try
            {
                localQueue.Dequeue().Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Dispatcher-Fehler: " + ex);
            }
        }
    }
}
