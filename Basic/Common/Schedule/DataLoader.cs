using System;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common.Schedule
{
    public sealed class DataLoader<T> : IScheduledOperation
    {
        private readonly Func<Task<T>> _loadFunc;
        private readonly int _maxRetries;

        private Task<T>? _loadTask;

        private bool _isActivated;
        private int _retryCount;
        private bool _isCompleted;

        public T? Result { get; private set; }

        public Exception? Error { get; private set; }

        public bool IsCompleted => _isCompleted;

        public bool IsLoading =>
            _loadTask != null && !_loadTask.IsCompleted;

        public bool HasError =>
            Error != null;

        public Action? Completed { get; set; }

        public Action? Failed { get; set; }

        object? IScheduledOperation.Result => Result;


        public DataLoader(
            Func<Task<T>> loadFunc,
            int maxRetries = 3)
        {
            _loadFunc = loadFunc ?? throw new ArgumentNullException(nameof(loadFunc));
            _maxRetries = Math.Max(0, maxRetries);
        }


        public void Activate()
        {
            if (_isActivated)
                return;

            _isActivated = true;

            StartLoad();
        }


        private void StartLoad()
        {
            _loadTask = _loadFunc();

            Error = null;
            Result = default;
            _isCompleted = false;
        }


        public void Update()
        {
            if (!_isActivated ||
                _loadTask == null ||
                _isCompleted)
            {
                return;
            }


            if (!_loadTask.IsCompleted)
                return;


            if (_loadTask.IsFaulted)
            {
                Error = _loadTask.Exception?.GetBaseException();


                if (_retryCount < _maxRetries)
                {
                    _retryCount++;

                    StartLoad();
                    return;
                }


                _isCompleted = true;

                Failed?.Invoke();

                return;
            }


            if (_loadTask.IsCanceled)
            {
                Error = new TaskCanceledException(_loadTask);

                _isCompleted = true;

                Failed?.Invoke();

                return;
            }


            Result = _loadTask.GetAwaiter().GetResult();

            _isCompleted = true;

            Completed?.Invoke();
        }


        public void Reload()
        {
            _retryCount = 0;

            StartLoad();
        }
    }
}