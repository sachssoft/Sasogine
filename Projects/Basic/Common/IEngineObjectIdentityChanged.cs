using System;

namespace Sachssoft.Sasogine.Common
{
    public interface IEngineObjectIdentityChanged
    {
        event EventHandler<EngineObjectChangedEventArgs>? IdChanged;
        event EventHandler<EngineObjectChangedEventArgs>? ClassChanged;
    }
}
