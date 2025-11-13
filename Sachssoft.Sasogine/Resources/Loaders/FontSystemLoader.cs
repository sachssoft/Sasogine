using FontStashSharp;
using System;

namespace Sachssoft.Sasogine.Resources
{
    public static class FontSystemLoader
    {
        public static FontSystem Load<TLoader>(string filePath, Func<string, TLoader> loaderInstance)
            where TLoader : LoaderBase
        {
            var loader = loaderInstance.Invoke(filePath);
            var system = new FontSystem();

            system.AddFont(loader.GetStream());
            return system;
        }
    }
}
