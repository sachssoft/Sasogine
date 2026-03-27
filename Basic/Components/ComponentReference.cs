using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Components
{
    public class ComponentReference<TComponent> : ReferenceBase<TComponent>
        where TComponent : class, IComponent
    {
        private ReferenceRegistry<IComponent>? _registry;

        public ReferenceRegistry<IComponent>? Registry
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