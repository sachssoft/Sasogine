using System;

namespace Sachssoft.Sasogine.Scenes
{
    /// <summary>
    /// Represents an exception that occurs during scene management,
    /// loading, updating, or rendering operations.
    /// Provides optional information about the affected scene.
    /// </summary>
    public class SceneException : Exception
    {
        /// <summary>
        /// Gets the scene associated with this exception.
        /// </summary>
        public IScene? Scene { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="SceneException"/> class.
        /// </summary>
        /// <param name="scene">
        /// The scene associated with the exception.
        /// </param>
        public SceneException(IScene? scene)
        {
            Scene = scene;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SceneException"/> class
        /// with an error message.
        /// </summary>
        /// <param name="scene">
        /// The scene associated with the exception.
        /// </param>
        /// <param name="message">
        /// The error message describing the failure.
        /// </param>
        public SceneException(
            IScene? scene,
            string message)
            : base(message)
        {
            Scene = scene;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SceneException"/> class
        /// with an error message and inner exception.
        /// </summary>
        /// <param name="scene">
        /// The scene associated with the exception.
        /// </param>
        /// <param name="message">
        /// The error message describing the failure.
        /// </param>
        /// <param name="innerException">
        /// The exception that caused this exception.
        /// </param>
        public SceneException(
            IScene? scene,
            string message,
            Exception innerException)
            : base(message, innerException)
        {
            Scene = scene;
        }
    }
}