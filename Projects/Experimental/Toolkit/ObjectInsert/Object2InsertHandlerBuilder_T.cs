using System;

namespace Sachssoft.Sasogine.Experimental.Components.Tools
{
    /// <summary>
    /// Provides a fluent builder for creating strongly typed 2D object insert handlers.
    /// </summary>
    /// <typeparam name="T">
    /// The type of object handled during insertion.
    /// </typeparam>
    public sealed class Object2InsertHandlerBuilder<T>
        where T : class
    {
        private readonly Func<Object2InsertContext, T> _create;
        private Action<T, Object2InsertContext>? _drag;
        private Action<T, Object2InsertContext>? _complete;
        private Action<T, Object2InsertContext>? _cancel;

        private Object2InsertHandlerBuilder(
            Func<Object2InsertContext, T> create)
        {
            _create = create;
        }

        /// <summary>
        /// Creates a new builder using the specified object creation callback.
        /// </summary>
        /// <param name="callback">
        /// The callback used to create the object when insertion begins.
        /// </param>
        /// <returns>
        /// A new builder instance.
        /// </returns>
        public static Object2InsertHandlerBuilder<T> Create(
            Func<Object2InsertContext, T> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return new Object2InsertHandlerBuilder<T>(callback);
        }

        /// <summary>
        /// Sets the callback invoked while the object is being dragged.
        /// </summary>
        /// <param name="callback">
        /// The callback invoked while the object is being dragged.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public Object2InsertHandlerBuilder<T> OnDrag(
            Action<T, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            _drag = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked when insertion is completed.
        /// </summary>
        /// <param name="callback">
        /// The callback invoked when insertion is completed.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public Object2InsertHandlerBuilder<T> OnComplete(
            Action<T, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            _complete = callback;
            return this;
        }

        /// <summary>
        /// Sets the callback invoked when insertion is canceled.
        /// </summary>
        /// <param name="callback">
        /// The callback invoked when insertion is canceled.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public Object2InsertHandlerBuilder<T> OnCancel(
            Action<T, Object2InsertContext> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            _cancel = callback;
            return this;
        }

        /// <summary>
        /// Builds the configured 2D object insert handler.
        /// </summary>
        /// <returns>
        /// The configured object insert handler.
        /// </returns>
        public IObject2InsertHandler Build()
        {
            return new ObjectInsertHandler(
                _create,
                _drag,
                _complete,
                _cancel);
        }

        private sealed class ObjectInsertHandler : IObject2InsertHandler
        {
            private readonly Func<Object2InsertContext, T> _create;
            private readonly Action<T, Object2InsertContext>? _drag;
            private readonly Action<T, Object2InsertContext>? _complete;
            private readonly Action<T, Object2InsertContext>? _cancel;

            public ObjectInsertHandler(
                Func<Object2InsertContext, T> create,
                Action<T, Object2InsertContext>? drag,
                Action<T, Object2InsertContext>? complete,
                Action<T, Object2InsertContext>? cancel)
            {
                _create = create;
                _drag = drag;
                _complete = complete;
                _cancel = cancel;
            }

            public object Create(
                Object2InsertContext context)
            {
                return _create(context) ??
                    throw new InvalidOperationException(
                        "The create callback returned null.");
            }

            public void Drag(
                object value,
                Object2InsertContext context)
            {
                _drag?.Invoke(
                    GetValue(value),
                    context);
            }

            public void Complete(
                object value,
                Object2InsertContext context)
            {
                _complete?.Invoke(
                    GetValue(value),
                    context);
            }

            public void Cancel(
                object value,
                Object2InsertContext context)
            {
                _cancel?.Invoke(
                    GetValue(value),
                    context);
            }

            private static T GetValue(
                object value)
            {
                if (value is not T typedValue)
                {
                    throw new InvalidOperationException(
                        $"The inserted object must be of type '{typeof(T).FullName}'.");
                }

                return typedValue;
            }
        }
    }
}