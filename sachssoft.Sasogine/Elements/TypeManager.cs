using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.AccessControl;

namespace sachssoft.Sasogine.Elements;

// AOT-freundliches Typenmanagement: keine Reflection, explizite Registrierung
public static class TypeManager
{
    private static readonly Dictionary<string, Tuple<Func<GameObject>, Type>> _key_factories = new();
    private static readonly Dictionary<int, Tuple<Func<GameObject>, Type>> _identifier_factories = new();
    private static readonly Dictionary<Type, (int? Identifier, string? Key, Func<GameObject> Factory)> _type_factories = new();
    private static readonly HashSet<Type> _declared_types = new();
    private static HashSet<Type> _known_types = new();
    private static bool _initialized = false;

    public static event EventHandler? Initialize;

    // Erzwingt die Initialisierung der statischen Klasse,
    // sodass der statische Konstruktor garantiert ausgeführt wird.
    // Nützlich in AOT-Szenarien oder bei Verwendung über Reflection.
    // Zusätzlich wird das optionale Initialize-Ereignis ausgelöst, falls gesetzt.
    internal static void EnsureInitialized()
    {
        if (!_initialized)
        {
            Initialize?.Invoke(null, EventArgs.Empty);
            _initialized = true;
        }
    }

    // Deklariert einen Typ zur Laufzeit, damit er in AOT-Szenarien ohne Reflection 
    // bekannt ist und zur Typauflösung oder zur initialen Registrierung verwendet werden kann.
    // Diese Methode dient insbesondere für Typen, die keine GameObject-Instanzen sind, 
    // aber dennoch wichtige Funktionen oder Registrierungslogik beinhalten (z. B. IAssetRegistrationLoader).
    public static void DeclareType<T>()
    {
        _declared_types.Add(typeof(T));
        _known_types.Add(typeof(T));
    }

    public static void IsTypeDeclared<T>() => _declared_types.Contains(typeof(T));

    public static void Register<T>(string name, Func<T> factory) where T : GameObject
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        var type = typeof(T);

        if (_type_factories.ContainsKey(type))
            throw new InvalidOperationException($"Factory for type {type.FullName} is already registered.");

        if (_key_factories.ContainsKey(name))
            throw new InvalidOperationException($"Factory with name '{name}' is already registered.");

        _type_factories[type] = new(null, name, factory);
        _key_factories[name] = new(factory, type);

        _known_types.Add(type);
    }

    public static void Register<T>(string name) where T : GameObject, new()
    {
        Register(name, () => new T());
    }

    public static void Register<T>(int identifier, Func<T> factory) where T : GameObject
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        var type = typeof(T);

        if (_identifier_factories.ContainsKey(identifier))
            throw new InvalidOperationException($"Factory for identifier {identifier} is already registered.");

        if (_type_factories.ContainsKey(type))
            throw new InvalidOperationException($"Factory for type {type.FullName} is already registered.");

        _identifier_factories[identifier] = new(factory, type);
        _type_factories[type] = (identifier, null, factory);

        _known_types.Add(type); // HashSet vermeidet Duplikate automatisch
    }

    public static void Register<T>(int identifier) where T : GameObject, new()
    {
        Register(identifier, () => new T());
    }

    public static void Register<T>(string name, int identifier, Func<T> factory) where T : GameObject
    {
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        var type = typeof(T);

        if (_identifier_factories.ContainsKey(identifier))
            throw new InvalidOperationException($"Factory for identifier {identifier} is already registered.");

        if (_key_factories.ContainsKey(name))
            throw new InvalidOperationException($"Factory with name '{name}' is already registered.");

        if (_type_factories.ContainsKey(type))
            throw new InvalidOperationException($"Factory for type {type.FullName} is already registered.");

        _identifier_factories[identifier] = new(factory, type);
        _key_factories[name] = new(factory, type);
        _type_factories[type] = new(identifier, name, factory);

        _known_types.Add(type); // HashSet vermeidet Duplikate automatisch
    }

    public static void Register<T>(string name, int identifier) where T : GameObject, new()
    {
        Register(name, identifier, () => new T());
    }

    public static void Register<T>(Func<T> factory) where T : GameObject
    {
        _ = factory ?? throw new ArgumentNullException(nameof(factory));
        _type_factories[typeof(T)] = (null, null, factory);

        if (!_known_types.Contains(typeof(T)))
            _known_types.Add(typeof(T));
    }

    public static void Register<T>() where T : GameObject, new()
    {
        Register(() => new T());
    }

    public static object CreateInstance(string name)
    {
        if (_key_factories.TryGetValue(name, out var creator))
            return creator.Item1();

        throw new KeyNotFoundException($"No type registered for name: {name}");
    }

    public static object CreateInstance(int identifier)
    {
        if (_identifier_factories.TryGetValue(identifier, out var creator))
            return creator.Item1();

        throw new KeyNotFoundException($"No type registered for identifier: {identifier}");
    }

    public static object CreateInstance(Type type)
    {
        if (_type_factories.TryGetValue(type, out var item))
            return item.Factory();

        throw new KeyNotFoundException($"No type registered for type: {type}");
    }

    public static object CreateInstance<T>() where T : class
    {
        return CreateInstance(typeof(T));
    }

    public static bool TryCreateInstance(string name, [NotNullWhen(true)] out GameObject? instance)
    {
        if (_key_factories.TryGetValue(name, out var creator))
        {
            instance = creator.Item1();
            return true;
        }

        instance = null;
        return false;
    }

    public static bool TryCreateInstance(int identifier, [NotNullWhen(true)] out GameObject? instance)
    {
        if (_identifier_factories.TryGetValue(identifier, out var creator))
        {
            instance = creator.Item1();
            return true;
        }

        instance = null;
        return false;
    }

    public static bool TryCreateInstance(Type type, [NotNullWhen(true)] out GameObject? instance)
    {
        if (_type_factories.TryGetValue(type, out var item))
        {
            instance = item.Factory();
            return true;
        }

        instance = null;
        return false;
    }

    public static bool TryCreateInstance<T>([NotNullWhen(true)] out T? instance) where T : GameObject
    {
        if (TryCreateInstance(typeof(T), out var obj) && obj is T t)
        {
            instance = t;
            return true;
        }

        instance = null;
        return false;
    }

    public static bool Contains(string name)
    {
        return _key_factories.ContainsKey(name);
    }

    public static bool Contains(int identifier)
    {
        return _identifier_factories.ContainsKey(identifier);
    }

    public static bool Contains(Type type)
    {
        return _type_factories.ContainsKey(type);
    }

    public static bool Contains<T>() where T : GameObject
    {
        return Contains(typeof(T));
    }

    public static Type? FindType(string name)
    {
        if (_key_factories.TryGetValue(name, out var result))
        {
            return result.Item2;
        }
        return null;
    }

    public static string? FindName(int identifier)
    {
        if (_identifier_factories.TryGetValue(identifier, out var result))
        {
            // Versuche, den zugehörigen Typ zu ermitteln
            var type = result.Item2;

            // Suche in den Schlüssel-Factories nach dem Namen
            foreach (var kvp in _key_factories)
            {
                if (kvp.Value.Item2 == type)
                {
                    return kvp.Key;
                }
            }
        }

        return null;
    }

    public static int? FindIdentifier(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (_key_factories.TryGetValue(name, out var entry))
        {
            var type = entry.Item2;
            if (_type_factories.TryGetValue(type, out var tuple))
            {
                return tuple.Identifier;
            }
        }

        return null;
    }

    public static string GetName<T>() where T : class => GetName(typeof(T));

    public static string GetName(Type type)
    {
        foreach (var item in _key_factories)
        {
            if (item.Value.Item2 == type)
            {
                return item.Key;
            }
        }

        throw new InvalidOperationException("Type not found");
    }

    public static int GetIdentifier<T>() where T : GameObject => GetIdentifier(typeof(T));

    public static int GetIdentifier(Type type)
    {
        foreach (var item in _identifier_factories)
        {
            if (item.Value.Item2 == type)
            {
                return item.Key;
            }
        }

        throw new InvalidOperationException("Type not found");
    }

    public static bool TryGetName<T>([NotNullWhen(true)] out string? name) where T : GameObject
        => TryGetName(typeof(T), out name);

    public static bool TryGetName(Type type, [NotNullWhen(true)] out string? name)
    {
        name = null;

        foreach (var item in _key_factories)
        {
            if (item.Value.Item2 == type)
            {
                name = item.Key;
                return true;
            }
        }

        return false;
    }

    public static bool TryFindIdentifier<T>([NotNullWhen(true)] out int? identifier) where T : GameObject
        => TryFindIdentifier(typeof(T), out identifier);

    public static bool TryFindIdentifier(Type type, [NotNullWhen(true)] out int? identifier)
    {
        identifier = null;
        foreach (var item in _identifier_factories)
        {
            if (item.Value.Item2 == type)
            {
                identifier = item.Key;
                return true;
            }
        }

        return false;
    }

    public static Type? FindType(int identifier)
    {
        if (_identifier_factories.TryGetValue(identifier, out var result))
        {
            return result.Item2;
        }
        return null;
    }

    public static IEnumerable<Type> KnownTypes => _known_types;

    public static IEnumerable<Type> RegisteredTypes => _type_factories.Keys;

    public static IEnumerable<Type> DeclaredTypes => _declared_types;

    public static (int? Identifier, string? Name) GetTypeInfo(Type type)
    {
        var item = _type_factories[type];
        return new(item.Identifier, item.Key);
    }

    public static IEnumerable<Type> GetTypesDerivedFrom<TBase>()
        => GetTypesDerivedFrom(typeof(TBase));

    public static IEnumerable<Type> GetTypesDerivedFrom(Type base_type)
    {
        if (base_type == null)
            throw new ArgumentNullException(nameof(base_type));

        if (!base_type.IsClass && !base_type.IsInterface)
            throw new ArgumentException("Parameter must be a class or interface type.", nameof(base_type));

        foreach (var type in _known_types)
        {
            if (type.IsClass && !type.IsAbstract && base_type.IsAssignableFrom(type) && type != base_type)
            {
                yield return type;
            }
        }
    }

    internal static void InvokeAssetRegistrations()
    {
        foreach (var type in _known_types)
        {
            if (_declared_types.Contains(type))
                continue;

            var item = _type_factories[type];
            var instance = item.Factory.Invoke();

            if (instance is IAssetRegistrationLoader loader)
            {
                loader.OnRegistered();
            }
        }
    }
}
