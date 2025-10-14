using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Surface
{
    /// <summary>
    /// Base class for the game context associated with a View.
    /// Encapsulates core information such as the current View, Runtime,
    /// GraphicsDevice, and optional payload data.
    /// </summary>
    public class GameBaseContext : IDisposable
    {
        /// <summary>
        /// Creates a new instance of <see cref="GameBaseContext"/> based on an existing instance.
        /// </summary>
        /// <param name="self">The existing instance from which View and Runtime are copied.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="self"/> is null.</exception>
        public GameBaseContext(GameBaseContext self)
            : this(self?.View ?? throw new ArgumentNullException(nameof(self)))
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="GameBaseContext"/> with a specified View.
        /// </summary>
        /// <param name="view">The View to associate with this context, from which the Runtime is copied.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="view"/> is null.</exception>
        public GameBaseContext(ViewBase? view)
        {
            View = view; // ?? throw new ArgumentNullException(nameof(view));
            Runtime = View?.Runtime;
        }

        /// <summary>
        /// The currently running instance of the game application.
        /// </summary>
        public IMyGameApp CurrentApp => IMyGameApp.Current ?? throw new InvalidOperationException("CurrentApp is null.");

        /// <summary>
        /// The <see cref="GraphicsDevice"/> of the current game application.
        /// </summary>
        public GraphicsDevice GraphicsDevice => IMyGameApp.Current?.GraphicsDevice
            ?? throw new InvalidOperationException("GraphicsDevice is null.");

        /// <summary>
        /// The View associated with this context.
        /// </summary>
        [AllowNull]
        public ViewBase View { get; }

        /// <summary>
        /// The Runtime associated with the View.
        /// </summary>
        [AllowNull]
        public RuntimeBase Runtime { get; }

        public CameraBase Camera => Runtime.Camera;

        public IEffectAdapter Effect => Runtime.Effect;

        /// <summary>
        /// Benchmark time that can be used for profiling or performance measurements.
        /// </summary>
        public TimeSpan BenchmarkTime { get; }

        /// <summary>
        /// Optional additional objects passed to the context.
        /// Can contain any data required by the View or the GameFrameContext.
        /// </summary>
        public object[]? Payload { get; set; }

        /// <summary>
        /// Retrieves an object from <see cref="Payload"/> at the specified index and casts it to type T.
        /// </summary>
        /// <typeparam name="T">The type to cast the object to.</typeparam>
        /// <param name="index">The index of the object in the Payload array.</param>
        /// <returns>The object cast to type T.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="Payload"/> is null, empty, or the index is invalid.
        /// </exception>
        public T? CastPayload<T>(int index)
        {
            if (Payload == null || index < 0 || index >= Payload.Length)
                throw new InvalidOperationException("Payload is null, empty, or the index is invalid.");

            return (T?)Payload[index];
        }

        /// <summary>
        /// Attempts to retrieve an object from <see cref="Payload"/> at the specified index and cast it to type T.
        /// </summary>
        /// <typeparam name="T">The type to cast the object to.</typeparam>
        /// <param name="index">The index of the object in the Payload array.</param>
        /// <param name="value">
        /// When this method returns, contains the object cast to T if successful; otherwise, default(T).
        /// </param>
        /// <returns>
        /// True if the cast was successful; false if Payload is null, index is invalid, or the object is of a different type.
        /// </returns>
        public bool TryCastPayload<T>(int index, [NotNullWhen(true)] out T? value)
        {
            value = default;

            if (Payload == null || index < 0 || index >= Payload.Length)
                return false;

            value = Payload[index] is T t ? t : default;
            return value != null;
        }

        /// <summary>
        /// Releases resources used by this context.
        /// </summary>
        public virtual void Dispose()
        {
        }
    }
}
