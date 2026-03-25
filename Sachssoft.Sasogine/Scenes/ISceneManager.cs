using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Scenes
{
    public interface ISceneManager
    {
        IScene CurrentScene { get; }

        IEnumerable<IScene> ActiveScenes { get; }

        void ChangeScene(IScene newScene);

        void Update(GameTime gameTime);

        void Draw(GameTime gameTime);
    }
}
