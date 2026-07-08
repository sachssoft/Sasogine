using Sachssoft.Sasogine.Components.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Common
{
    public interface IEngineObjectResolver
    {
        IEngineReferenceable? Find(string? id);

        IEnumerable<IEngineReferenceable> FindAll(string? @class);

        bool TryGet(string? id, [MaybeNullWhen(false)] out IEngineReferenceable? result);
    }
}
