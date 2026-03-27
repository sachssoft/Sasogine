using System;

namespace Sachssoft.Sasogine.Common
{
    public interface IFactoryRegistry
    {

        void Register(string targetId, Type targetType, Func<object?> factory);

        void Unregister(string targetId);

        object? Create(string targetId, Type targetType);

    }
}
