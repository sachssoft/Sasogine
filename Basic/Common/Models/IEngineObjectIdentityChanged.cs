using System;

namespace Sachssoft.Sasogine.Common.Models
{
    public interface IEngineObjectIdentityChanged
    {
        event EventHandler<EngineObjectChangedEventArgs>? IdChanged;
        event EventHandler<EngineObjectChangedEventArgs>? ClassChanged;
    }
}
