using Sachssoft.Sasogine.Presentation.Layouts;
using Sachssoft.Sasogine.Presentation.Widgets;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public sealed class StyleRegistry
    {
        private readonly Dictionary<Type, StyleRegistryStyleableEntry> _styleableTypes = new();
        private record StyleRegistryStyleableEntry(Type Type);

        public StyleRegistry()
        {
            Register();
        }

        public void Register<T>() where T : class, IStyleable
        {
            _styleableTypes[typeof(T)] = new StyleRegistryStyleableEntry(
                Type: typeof(T)
            );
        }

        private void Register()
        {
            Register<Workspace>();
            Register<CanvasLayout>();
            Register<Button>();
        }
    }
}