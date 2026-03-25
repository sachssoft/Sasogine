using System;

namespace Sachssoft.Sasogine.Scenes
{
    public class SceneException : Exception
    {
        public IScene? Scene { get; }

        public SceneException(IScene? scene)
        {
            Scene = scene;
        }

        public SceneException(IScene? scene, string message)
            : base(message)
        {
            Scene = scene;
        }

        public SceneException(IScene? scene, string message, Exception innerException)
            : base(message, innerException)
        {
            Scene = scene;
        }
    }
}