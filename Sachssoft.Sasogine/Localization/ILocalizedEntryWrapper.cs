using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Localization
{
    public interface ILocalizedEntryWrapper
    {
        void Load(Dictionary<string, string?> attributes, LoaderBase? loader);

        object? Value {  get; }
    }
}
