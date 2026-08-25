using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Sachssoft.Sasogine.Common
{
    public class EngineObjectManager
    {
        private readonly List<IEngineObject> _objects = new();

        public void Register(IEngineObject @object)
        {
            if (@object == null)
                throw new ArgumentNullException(nameof(@object));

            _objects.Add(@object);
        }

        public void Unregister(IEngineObject @object)
        {
            if (@object == null)
                return;

            _objects.Remove(@object);
        }

        public IEngineObject? Find(IEngineObjectDefinition definition)
        {
            if (definition == null)
                return null;

            return _objects.FirstOrDefault(o =>
                ReferenceEquals(o.Definition, definition) ||
                (o.Definition is IEngineObjectDefinition eod) && eod.Id == definition.Id);
        }

        public bool TryFind(IEngineObjectDefinition definition,
            [MaybeNullWhen(false)] out IEngineObject? result)
        {
            result = Find(definition);
            return result != null;
        }
    }
}