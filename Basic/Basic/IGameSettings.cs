using Sachssoft.Sasogine.Resources.Source;

namespace Sachssoft.Sasogine.Basic
{
    public interface IGameSettings
    {
        SourceBase Source { get; set; }

        void Load();

        void Save();

        T? GetValue<T>(string key);

        void SetValue<T>(string key, T? value);
    }
}
