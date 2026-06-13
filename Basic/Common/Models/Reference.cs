using Sachssoft.Sasogine.Common.Models;
using System;

namespace Sachssoft.Sasogine.Common
{
    public class Reference<T> : IReference
        where T : class, IEngineReferenceable
    {
        /// <summary>
        /// Id des referenzierten Objekts
        /// </summary>
        public string? Id { get; set; }

        public bool IsEmpty => string.IsNullOrEmpty(Id);

        Type IReference.TargetType => typeof(T);

        public T? Resolve(IEngineObjectResolverProvider provider)
        {
            _ = provider ?? throw new ArgumentNullException(nameof(provider));

            if (string.IsNullOrEmpty(Id))
                return null;

            var obj = provider.Resolver.Find(Id);

            if (obj == null)
                return null;

            if (obj is not T result)
            {
                throw new InvalidOperationException(
                    $"Object '{Id}' is not of type '{typeof(T).Name}'.");
            }

            return result;
        }

        object? IReference.Resolve(IEngineObjectResolverProvider provider)
        {
            return Resolve(provider);
        }
    }
}