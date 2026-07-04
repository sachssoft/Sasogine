using System;

namespace Sachssoft.Sasogine.Common.Models
{
    public interface IReference
    {
        Type TargetType { get; }

        string? Id { get; set; }

        object? Resolve(IEngineObjectResolverProvider provider);
    }
}
