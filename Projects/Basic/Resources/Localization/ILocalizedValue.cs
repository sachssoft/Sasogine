namespace Sachssoft.Sasogine.Resources.Localization
{
    public interface ILocalizedValue
    {
        string Key { get; }

        object? Fallback { get; }
    }
}