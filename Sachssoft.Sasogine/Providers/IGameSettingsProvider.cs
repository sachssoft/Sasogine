namespace Sachssoft.Sasogine.Providers
{
    public interface IGameSettingsProvider
    {
        object? GetValue(string key);

        void SetValue(string key, object? value);

        void Load();

        void Save();

    }
}
