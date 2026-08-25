using System;

namespace Sachssoft.Sasogine.Common
{
    public interface IReference
    {
        Type TargetType { get; }

        string? Id { get; set; }

        object? Resolve(IEngineObjectResolverProvider provider);
    }
}
