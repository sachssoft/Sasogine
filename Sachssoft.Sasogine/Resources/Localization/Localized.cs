using System;

namespace Sachssoft.Sasogine.Resources.Localization
{
    public static class Localized
    {
        public static LocalizationBinding<T> Bind<T>(string key, T? defaultValue, Action<T?> setter)
            where T : class
        {
            return new LocalizationBinding<T>(key, defaultValue, setter);
        }

        public static T? GetValue<T>(string key, T? defaultValue)
            where T : class
        {
            var dict = GameApplication.Current.Localization.Entries;
            if (dict.TryGetValue<T>(key, out var value))
            {
                if (value is T tValue)
                {
                    return tValue;
                }
            }
            return defaultValue;
        }
    }
}
