using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Assets;
using Sachssoft.Sasogine.Containers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Surface.Controls.Inspectors
{
    public sealed class InspectorValueEditorRegistry
    {
        private record RegistryEntry
        {
            public required Type PropertyType { get; init; }
            public Func<Type, IEnumerable<PropertyDescriptor>?, bool>? CheckType { get; init; }
            public IEnumerable<PropertyDescriptor>? Descriptors { get; init; }
            public required Func<InspectorValueEditorBase> Factory { get; init; }
            public Func<object?>? TypeFactory { get; init; }

            public bool IsApplicable(Type type, IEnumerable<PropertyDescriptor>? desc = null)
            {
                if (CheckType != null)
                    return CheckType(type, desc);

                // Standardprüfung: exakter Typ oder Enum-Kompatibilität
                return type == PropertyType || PropertyType == typeof(Enum) && type.IsEnum;
            }
        }

        private readonly List<RegistryEntry> _registryEntries = new();
        private readonly Inspector _inspector;

        internal InspectorValueEditorRegistry(Inspector inspector)
        {
            _inspector = inspector;

            // Numerische Typen
            Register<byte, NumericValueEditor>();
            Register<sbyte, NumericValueEditor>();
            Register<short, NumericValueEditor>();
            Register<ushort, NumericValueEditor>();
            Register<int, NumericValueEditor>();
            Register<uint, NumericValueEditor>();
            Register<long, NumericValueEditor>();
            Register<ulong, NumericValueEditor>();
            Register<float, NumericValueEditor>();
            Register<double, NumericValueEditor>();
            Register<decimal, NumericValueEditor>();

            // Vektorische Typen
            Register<Vector2, Vector2ValueEditor>();
            Register<Vector3, Vector3ValueEditor>();

            // Bool
            Register<bool, BooleanValueEditor>();

            // Zeichen / String
            Register<char, TextValueEditor>();
            Register<string, TextValueEditor>();

            // Datum/Zeit
            Register<DateTime, DateTimeValueEditor>();
            Register<TimeSpan, DateTimeValueEditor>();

            // Sonstiges
            Register<Color, ColorValueEditor>();

            // Enums, Arrays, ...
            RegisterEnum();
            RegisterNotifyingObjectArray();

            // Associations
            RegisterAssetAssociation<Texture2DAsset, Texture2D>();
        }

        private void ReplaceEntries(IEnumerable<RegistryEntry> entries)
        {
            _registryEntries.Clear();
            _registryEntries.AddRange(entries);
        }

        private IEnumerable<RegistryEntry> GetEntries()
        {
            return _registryEntries.ToArray(); // defensive copy
        }

        // Public API
        public void ApplyFrom(Inspector otherInspector)
        {
            if (otherInspector is null)
                throw new ArgumentNullException(nameof(otherInspector));

            if (ReferenceEquals(otherInspector, _inspector))
                throw new InvalidOperationException("Ein Controls kann nicht von sich selbst kopieren.");

            ReplaceEntries(otherInspector.Editors.GetEntries());
        }

        public void Register(
            Type type,
            Func<InspectorValueEditorBase> factory,
            Func<Type, IEnumerable<PropertyDescriptor>?, bool>? checkType = null,
            IEnumerable<PropertyDescriptor>? attributes = null)
        {
            _registryEntries.Add(new RegistryEntry
            {
                PropertyType = type,
                Factory = factory,
                CheckType = checkType,
                Descriptors = attributes
            });
        }

        public void Register<TType, TEditor>() where TEditor : InspectorValueEditorBase, new()
        {
            _registryEntries.Add(new RegistryEntry
            {
                PropertyType = typeof(TType),
                Factory = () => new TEditor()
            });
        }

        public void Register<TType>(Func<InspectorValueEditorBase> factory,
            Func<Type, IEnumerable<PropertyDescriptor>?, bool>? checkType = null,
            IEnumerable<PropertyDescriptor>? descriptors = null)
        {
            _registryEntries.Add(new RegistryEntry
            {
                PropertyType = typeof(TType),
                Factory = factory,
                CheckType = checkType,
                Descriptors = descriptors
            });
        }

        private void RegisterEnum()
        {
            _registryEntries.Add(new RegistryEntry
            {
                PropertyType = typeof(Enum),
                Factory = () => new EnumValueEditor(),
                CheckType = (type, _) =>
                {
                    var t = Nullable.GetUnderlyingType(type) ?? type;
                    return t.IsEnum;
                }
            });
        }

        private void RegisterNotifyingObjectArray()
        {
            _registryEntries.Add(new RegistryEntry
            {
                PropertyType = typeof(NotifyingObject[]), // exemplarischer Typ
                Factory = () => new NotifyingObjectArrayEditor(),
                CheckType = (type, _) =>
                {
                    // Arrays
                    if (type.IsArray)
                    {
                        var elementType = type.GetElementType();
                        return elementType != null &&
                               typeof(NotifyingObject).IsAssignableFrom(elementType);
                    }

                    // IEnumerable<T>
                    if (type.IsGenericType &&
                        typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                    {
                        var elementType = type.GetGenericArguments()[0];
                        return typeof(NotifyingObject).IsAssignableFrom(elementType);
                    }

                    return false;
                }
            });
        }

        public void RegisterAssetAssociation<TAsset, TUnderlying>()
            where TAsset : AssetBase<TUnderlying>
            where TUnderlying : class
        {
            _registryEntries.Add(new RegistryEntry
            {
                PropertyType = typeof(AssetReference<TAsset>),
                Factory = () => new AssociationValueEditor(),
                CheckType = (type, _) => type.IsAssignableTo(typeof(AssetReference<TAsset>)),
            });
        }

        // --- 🔥 Verbesserter Teil ab hier ---

        private static Type NormalizeType(Type type)
        {
            // Wandelt Nullable<T> in T um, sonst bleibt der Typ gleich
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public bool TryCreateEditor(Type type, out InspectorValueEditorBase? editor, IEnumerable<PropertyDescriptor>? descriptors = null)
        {
            var normalizedType = NormalizeType(type);

            // Letzter passender Eintrag gewinnt (spezifischere Registrierungen am Ende)
            var entry = _registryEntries.LastOrDefault(e => e.IsApplicable(normalizedType, descriptors));

            if (entry != null)
            {
                editor = entry.Factory();
                editor.TypeFactory = entry.TypeFactory;
                return true;
            }

#if DEBUG
            System.Diagnostics.Debug.WriteLine(
                $"[Inspector] Kein Editor für Typ '{type.FullName}' (Normalisiert: '{normalizedType.FullName}') gefunden.");
#endif

            editor = null;
            return false;
        }

        public InspectorValueEditorBase CreateEditor(Type type, IEnumerable<PropertyDescriptor>? descriptors = null)
        {
            var normalizedType = NormalizeType(type);

            var entry = _registryEntries.LastOrDefault(e => e.IsApplicable(normalizedType, descriptors));
            if (entry != null)
            {
                var editor = entry.Factory();
                editor.TypeFactory = entry.TypeFactory;
                return editor;
            }

            throw new NotSupportedException(
                $"Kein InspectorValueEditor für Typ {type.Name} (Normalisiert: {normalizedType.Name}) registriert.");
        }

        public IReadOnlyList<Type> RegisteredTypes => _registryEntries.Select(e => e.PropertyType).Distinct().ToList();
    }
}
