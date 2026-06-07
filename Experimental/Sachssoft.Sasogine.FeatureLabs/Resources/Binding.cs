using System;

namespace Sachssoft.Sasogine.Resources;

public sealed class Binding
{
    private readonly string _name;

    public Binding(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    // Suche nur im Skin
    public T Resolve<T>(ResourceStore store)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        if (store.TryGetResource<T>(_name, out var resource))
            return resource; // resource ist nicht null, weil TryGetResource true zurückgegeben hat

        // Resource nicht gefunden → aussagekräftige Exception
        throw new InvalidOperationException(
            $"Resource '{_name ?? "<unnamed>"}' of type '{typeof(T).FullName}' could not be resolved in the provided ResourceStore.");
    }

    // Try-Variante: true/false zurückgeben, kein Exception
    public bool TryResolve<T>(ResourceStore store, out T? resource)
    {
        if (store == null)
            throw new ArgumentNullException(nameof(store));

        return store.TryGetResource<T>(_name, out resource);
    }

    // Suche im Skin und ggf. in Ancestor-Hierarchie
    // später implementieren
    //public T? Resolve<T, TAncestor>(Skin sheet)
    //{
    //    // 1️⃣ Zuerst Skin prüfen
    //    var local = Resolve<T>(sheet);
    //    if (local != null) return local;

    //    // 2️⃣ Ancestor-Typ prüfen
    //    if (ancestorType == null) return null;

    //    object? ancestor = sheet.FindAncestor(ancestorType);
    //    if (ancestor == null) return null;

    //    // 3️⃣ Cast & Lookup
    //    if (ancestor is IResourceContainer container)
    //    {
    //        if (container.TryGetResource<T>(_name, out var value))
    //            return value;
    //    }

    //    return null;
    //}
}