using Sachssoft.Sasogine.Resources;
using Sachssoft.Sasogine.Resources.Sources;

namespace Sachssoft.Sasogine
{
    public interface IGameSettings
    {
        ResourceSourceBase Source { get; set; }

        void Load();

        void Save();

        T? GetValue<T>(string key);

        void SetValue<T>(string key, T? value);
    }
}
