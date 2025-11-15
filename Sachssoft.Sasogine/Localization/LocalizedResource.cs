using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Sasogine.Localization
{
    public class LocalizedResource : ILocalizedEntry
    {
        private GameResourceManager? _resourceManager;
        private CultureInfo? _culture;
        private string? _resourceKey;
        private bool _isLoaded = false;

        public bool IsLoaded => _isLoaded;

        public void Load(CultureInfo culture, GameResourceManager resourceManager, LocalizedEntryData data)
        {
            _culture = culture ?? throw new ArgumentNullException(nameof(culture));
            _resourceManager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));

            if (!data.Attributes.TryGetValue("ResourceKey", out _resourceKey))
                throw new InvalidOperationException("Resource entry must have a ResourceKey.");

            _isLoaded = true;
        }

        public object? GetValue(int count)
        {
            if (_resourceManager == null || _culture == null)
                throw new InvalidOperationException("ResourceManager or Culture not initialized.");

            _resourceKey ??= string.Empty;
            return _resourceManager.Load(_resourceKey, _culture);
        }
    }

}
