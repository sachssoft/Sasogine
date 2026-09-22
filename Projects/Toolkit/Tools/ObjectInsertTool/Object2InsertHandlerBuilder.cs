using Sachssoft.Engine.Common;
using System;

namespace Sachssoft.Engine.Components.Tools
{
    /// <summary>
    /// Provides a fluent builder for creating 2D object insert handlers.
    /// </summary>
    public sealed class Object2InsertHandlerBuilder
    {
        private readonly Func<Object2InsertContext, IDefinition> _create;
        private Action<IDefinition, Object2InsertContext>? _drag;
        private Action<IDefinition, Object2InsertContext>? _complete;
        private Action<IDefinition, Object2InsertContext>? _cancel;

        private Object2InsertHandlerBuilder(
            Func<Object2InsertContext, IDefinition> create)
        {
            _create = create;
        }

        /// <summary>
        /// Creates a new builder using the specified definition creation callback.
        /// </summary>
        /// <param name="callback">
        /// The callback used to create the definition when insertion begins.
        /// </param>
        /// <returns>
        /// A new builder instance.
        /// </returns>
        public static Object2InsertHandlerBuilder Create(
            Func<Object2InsertContext, IDefinition> callback)
        {
            ArgumentNullException.ThrowIfNull(callback);

            return new Object2InsertHandlerBuilder(callback);
        }

        /// <summary>
        /// Sets the callback invoked while the definition is being dragged.
        /// </summary>
        /// <param name="callback">
        /// The callback invoked while the definition is being dragged.
        /// </param>
        /// <returns>
        /// This builder instance.
        /// </returns>
        public Object2InsertHandlerBuilder OnDrag(
            Action<IDefinition, Object2InsertContext> callback)
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
        public Object2InsertHandlerBuilder OnComplete(
            Action<IDefinition, Object2InsertContext> callback)
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
        public Object2InsertHandlerBuilder OnCancel(
            Action<IDefinition, Object2InsertContext> callback)
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
            private readonly Func<Object2InsertContext, IDefinition> _create;
            private readonly Action<IDefinition, Object2InsertContext>? _drag;
            private readonly Action<IDefinition, Object2InsertContext>? _complete;
            private readonly Action<IDefinition, Object2InsertContext>? _cancel;

            public ObjectInsertHandler(
                Func<Object2InsertContext, IDefinition> create,
                Action<IDefinition, Object2InsertContext>? drag,
                Action<IDefinition, Object2InsertContext>? complete,
                Action<IDefinition, Object2InsertContext>? cancel)
            {
                _create = create;
                _drag = drag;
                _complete = complete;
                _cancel = cancel;
            }

            public IDefinition Create(
                Object2InsertContext context)
            {
                return _create(context) ??
                    throw new InvalidOperationException(
                        "The create callback returned null.");
            }

            public void Drag(
                IDefinition definition,
                Object2InsertContext context)
            {
                _drag?.Invoke(
                    definition,
                    context);
            }

            public void Complete(
                IDefinition definition,
                Object2InsertContext context)
            {
                _complete?.Invoke(
                    definition,
                    context);
            }

            public void Cancel(
                IDefinition definition,
                Object2InsertContext context)
            {
                _cancel?.Invoke(
                    definition,
                    context);
            }
        }
    }
}