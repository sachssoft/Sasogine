using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Scenes
{
    public interface ISceneManager
    {
        bool IsLoaded { get; }

        IScene CurrentScene { get; }

        IEnumerable<IScene> ActiveScenes { get; }

        void ChangeScene(IScene newScene);

        void Load();

        void Update(GameTime gameTime);

        void Draw(GameTime gameTime);
    }
}
