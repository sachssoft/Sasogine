using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common.Schedule
{
    public sealed class AsyncLoader : IScheduledOperation
    {
        private readonly Func<AsyncLoader, Task> _loader;
        private readonly Queue<Action> _mainThreadQueue = new();

        private Task? _task;

        private bool _completed;
        private bool _activated;

        private float _progress;


        public float Progress => _progress;

        public bool IsCompleted => _completed;

        public bool IsLoading =>
            _task != null && !_task.IsCompleted;

        public bool HasError =>
            Error != null;

        public Exception? Error { get; private set; }


        public object? Result => null;


        public AsyncLoader(Func<AsyncLoader, Task> loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        }


        public void Activate()
        {
            if (_activated)
                return;

            _activated = true;

            _task = ExecuteAsync();
        }


        private async Task ExecuteAsync()
        {
            try
            {
                await _loader(this);
            }
            catch (Exception ex)
            {
                Error = ex;
            }
        }


        public void ReportProgress(float value)
        {
            _progress = value;
        }


        public void EnqueueMainThread(Action action)
        {
            lock (_mainThreadQueue)
            {
                _mainThreadQueue.Enqueue(action);
            }
        }

        public void Update()
        {
            while (true)
            {
                Action? action;

                lock (_mainThreadQueue)
                {
                    if (_mainThreadQueue.Count == 0)
                        break;

                    action = _mainThreadQueue.Dequeue();
                }

                action();
            }


            if (!_activated || _task == null || _completed)
                return;


            if (!_task.IsCompleted)
                return;


            _completed = true;
        }
    }
}