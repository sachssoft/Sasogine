using FontStashSharp;
using System;
using System.IO;
using System.Reflection;

namespace Sachssoft.Sasogine.Surface.Visuals
{
    internal static class FontResourceLoader
    {
        public static FontSystem LoadFontSystem(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new ArgumentException($"Embedded resource '{resourceName}' not found.", nameof(resourceName));

            var fontSystem = new FontSystem();
            fontSystem.AddFont(stream);
            return fontSystem;
        }
    }
}
