using Microsoft.Xna.Framework;
using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Components;
using Sachssoft.Sasogine.Presentation;
using System;

namespace Sachssoft.Sasogine.Inspection
{
    [Obsolete(
        "This class is deprecated and will be removed in a future release. " +
        "Please use the new component system " + nameof(GameComponent) + " instead. ")]
    public class GameObject : NotifyingElement, IDrawableRuntimeComponent, IResourceComponent
    {
        private bool _isLoaded;

        public bool IsLoaded => _isLoaded;

        public virtual void Load()
        {
            if (_isLoaded)
                return;

            _isLoaded = true;
        }

        public virtual void Unload()
        {
            if (!_isLoaded)
                return;

            _isLoaded = false;
        }

        public virtual void Update(RuntimeContext context)
        {
        }

        public virtual void Draw(RuntimeViewportContext context)
        {
        }
    }
}
