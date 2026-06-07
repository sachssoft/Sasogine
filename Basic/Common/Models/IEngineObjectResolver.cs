using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Common
{
    public interface IEngineObjectResolver
    {
        IEngineObject? Find(string id);

        IEnumerable<IEngineObject> FindAll(string? @class);

        bool TryGet(string? id, [MaybeNullWhen(false)] out IEngineObject? result);
    }
}
