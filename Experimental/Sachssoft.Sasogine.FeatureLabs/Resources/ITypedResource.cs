using System;

namespace Sachssoft.Sasogine.Resources;

public interface ITypedResource
{
    /// <summary>
    /// Der Typ, auf den dieser Skin-Eintrag angewendet wird.
    /// </summary>
    Type TargetType { get; }

    /// <summary>
    /// Unveränderliche Properties des Eintrags.
    /// </summary>
    PropertySet Properties { get; }
}
