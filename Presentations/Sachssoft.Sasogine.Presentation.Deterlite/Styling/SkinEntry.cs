using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    /// <summary>
    /// Abstrakte Basisklasse für Skin-Einträge. 
    /// Kinder sind vom gleichen Typ <typeparamref name="T"/> und immutable.
    /// </summary>
    /// <typeparam name="T">Der konkrete Typ des Skin-Eintrags.</typeparam>
    public abstract class SkinEntry<T> : ISkinEntry where T : SkinEntry<T>
    {
        private readonly IReadOnlyList<T> _children;

        protected SkinEntry(
            string? id,
            Type targetType,
            PropertySet properties,
            IEnumerable<T>? children = null)
        {
            Id = id;
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));

            _children = children?.ToList().AsReadOnly() ?? Array.Empty<T>();
        }

        /// <summary>
        /// Eindeutige Id des Skin-Eintrags.
        /// </summary>
        public string? Id { get; }

        /// <summary>
        /// Optional zugeordnete Klasse für Gruppierungen.
        /// </summary>
        public string? Class => Properties.Get<string>("Class");

        /// <summary>
        /// Der Typ, auf den dieser Skin-Eintrag angewendet wird.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Unveränderliche Properties des Eintrags.
        /// </summary>
        public PropertySet Properties { get; }

        /// <summary>
        /// Kinder dieses Eintrags (immutable), vom gleichen Typ T.
        /// </summary>
        public IReadOnlyList<T> Children => _children;

        /// <summary>
        /// Implementierung von ISkinEntry: Liefert Kinder als ISkinEntry.
        /// </summary>
        IReadOnlySet<ISkinEntry> ISkinEntry.Children => new HashSet<ISkinEntry>(_children);

        /// <summary>
        /// Liefert ein direktes Kind nach Id oder null, wenn nicht gefunden.
        /// </summary>
        public T? GetChild(string id) => _children.FirstOrDefault(c => c.Id == id);

        ISkinEntry? ISkinEntry.GetChild(string id) => GetChild(id);

        /// <summary>
        /// Liefert alle direkten Kinder mit der angegebenen Klasse.
        /// </summary>
        public IEnumerable<T> GetChildrenByClass(string className) =>
            _children.Where(c => c.Class == className);

        IEnumerable<ISkinEntry> ISkinEntry.GetChildrenByClass(string className) =>
            GetChildrenByClass(className);

        /// <summary>
        /// Liefert alle Nachkommen (rekursiv).
        /// </summary>
        public IEnumerable<T> GetDescendants()
        {
            foreach (var child in _children)
            {
                yield return child;
                foreach (var descendant in child.GetDescendants())
                    yield return descendant;
            }
        }

        IEnumerable<ISkinEntry> ISkinEntry.GetDescendants() => GetDescendants();
    }
}