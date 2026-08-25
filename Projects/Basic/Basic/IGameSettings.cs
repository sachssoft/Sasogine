using Sachssoft.Sasogine.Resources;

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
