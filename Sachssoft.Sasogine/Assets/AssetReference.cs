using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Assets
{
    // TAsset z.B. Texture2D

    public class AssetReference<TAsset> : ReferenceBase<TAsset>
        where TAsset : class, IAsset
    {
        private ReferenceRegistry<IAsset>? _registry;

        public ReferenceRegistry<IAsset>? Registry
        {
            get => _registry;
            set
            {
                if (ReferenceEquals(_registry, value)) return;

                if (_registry != null)
                {
                    _registry.Registered -= OnRegistryChanged;
                    _registry.Unregistered -= OnRegistryChanged;
                }

                _registry = value;

                if (_registry != null)
                {
                    _registry.Registered += OnRegistryChanged;
                    _registry.Unregistered += OnRegistryChanged;
                }

                MarkDirty();
            }
        }

        protected override TAsset? ResolveValue(string id)
        {
            return _registry?.Find(id) as TAsset;
        }

        private void OnRegistryChanged(object? sender, EventArgs e)
        {
            MarkDirty();
        }
    }
}