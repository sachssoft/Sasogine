using System;

namespace Sachssoft.Sasogine.Surface.Basic
{
    public static class Localized
    {
        public static LocalizationBinding<T> Bind<T>(string key, T? defaultValue, Action<T?> setter)
        {
            return new LocalizationBinding<T>(key, defaultValue, setter);
        }
    }
}
