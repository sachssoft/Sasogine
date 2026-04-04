using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public interface ISkinEntry
    {
        /// <summary>
        /// Eindeutige Id des Skin-Eintrags.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Optional zugeordnete Klasse für Gruppierungen.
        /// </summary>
        string? Class { get; }

        /// <summary>
        /// Der Typ, auf den dieser Skin-Eintrag angewendet wird.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Unveränderliche Properties des Eintrags.
        /// </summary>
        PropertySet Properties { get; }

        /// <summary>
        /// Kinder dieses Eintrags (immutable).
        /// </summary>
        IReadOnlySet<ISkinEntry> Children { get; }

        /// <summary>
        /// Liefert ein direktes Kind nach Id oder null, wenn nicht gefunden.
        /// </summary>
        ISkinEntry? GetChild(string id);

        /// <summary>
        /// Liefert alle direkten Kinder mit der angegebenen Klasse.
        /// </summary>
        IEnumerable<ISkinEntry> GetChildrenByClass(string className);

        /// <summary>
        /// Liefert alle Nachkommen (rekursiv).
        /// </summary>
        IEnumerable<ISkinEntry> GetDescendants();
    }
}