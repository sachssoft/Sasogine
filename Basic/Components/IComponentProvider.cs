using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sachssoft.Sasogine.Components
{
    public interface IComponentProvider
    {
        bool TryGetComponent<T>([MaybeNullWhen(false)] out T component) where T : class,IComponent;

    }
}
