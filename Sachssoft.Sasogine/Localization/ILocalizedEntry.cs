using Sachssoft.Sasogine.Resources;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Sasogine.Localization
{
    public interface ILocalizedEntry
    {
        bool IsLoaded { get; }

        void Load(CultureInfo culture, GameResourceManager resourceManager, LocalizedEntryData data);

        object? GetValue(int count);
    }
}
