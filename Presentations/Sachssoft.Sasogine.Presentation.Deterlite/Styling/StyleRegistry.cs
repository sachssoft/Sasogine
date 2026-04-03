using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;
using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public sealed class StyleRegistry
    {
        private readonly Dictionary<Type, StyleRegistryPartEntry> _partTypes = new();
        private readonly Dictionary<Type, StyleRegistryStyleableEntry> _styleableTypes = new();

        private record StyleRegistryPartEntry(Type Type, Func<IStylePart> Factory);
        private record StyleRegistryStyleableEntry(Type Type, Func<IStyleable> Factory);

        public StyleRegistry()
        {
            Register();
        }

        // Brush, Font, Region, ...
        public void RegisterPart<T>() where T : class, IStylePart, new()
        {
            _partTypes[typeof(T)] = new StyleRegistryPartEntry(
                Type: typeof(T),
                Factory: () => new T()
            );
        }

        public void RegisterStyleable<T>() where T : class, IStyleable, new()
        {
            _styleableTypes[typeof(T)] = new StyleRegistryStyleableEntry(
                Type: typeof(T),
                Factory: () => new T()
            );
        }

        public IStylePart CreatePart(string name)
        {
            var result = TryCreatePart(name);
            if (result == null)
                throw new InvalidOperationException($"No style part with name '{name}' is registered.");

            return result;
        }

        public IStylePart? TryCreatePart(string name)
        {
            foreach (var entry in _partTypes.Values)
            {
                if (entry.Type.Name.Equals(name, StringComparison.Ordinal))
                    return entry.Factory();
            }

            return null;
        }

        public IStylePart CreatePart(Type type)
        {
            var result = TryCreatePart(type);
            if (result == null)
                throw new InvalidOperationException($"No style part with type '{type}' is registered.");

            return result;
        }

        public IStylePart? TryCreatePart(Type type)
        {
            if (_partTypes.TryGetValue(type, out var entry))
                return entry.Factory();

            return null;
        }

        private void Register()
        {
            // Brushes
            RegisterPart<SolidColorBrush>();
            RegisterPart<TextureBrush>();

            // Regions
            RegisterPart<TextureRegion>();

            // Styled Elements
            RegisterStyleable<CanvasLayout>();
            RegisterStyleable<Button>();
        }
    }
}