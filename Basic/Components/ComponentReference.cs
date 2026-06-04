using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Components
{
    public class ComponentReference<TComponent> : Reference<TComponent>
        where TComponent : class, IComponent
    {
        private EngineObjectCollection<IComponent>? _registry;

        public EngineObjectCollection<IComponent>? Registry
        {
            get => _registry;
            set
            {
                if (ReferenceEquals(_registry, value)) return;

                if (_registry != null)
                {
                    _registry.Added -= OnRegistryChanged;
                    _registry.Removed -= OnRegistryChanged;
                }

                _registry = value;

                if (_registry != null)
                {
                    _registry.Added += OnRegistryChanged;
                    _registry.Removed += OnRegistryChanged;
                }

                MarkDirty();
            }
        }

        protected override TComponent? ResolveValue(string id)
        {
            return _registry?.Find(id) as TComponent;
        }

        private void OnRegistryChanged(object? sender, EventArgs e)
        {
            MarkDirty();
        }
    }
}