using System;

using Sachssoft.Sasogine.Presentation.Styling;

public sealed class StyleBinding
{
    private readonly string _name;

    public StyleBinding(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
    }

    // Suche nur im Skin
    public T? Resolve<T>(Skin skin)
        where T : class
    {
        if (skin == null) return null;

        if (skin.TryGetResource<T>(_name, out var value))
            return value;

        return null;
    }

    // Suche im Skin und ggf. in Ancestor-Hierarchie
    public T? Resolve<T, TAncestor>(Skin sheet)
    {
        // 1️⃣ Zuerst Skin prüfen
        var local = Resolve<T>(sheet);
        if (local != null) return local;

        // 2️⃣ Ancestor-Typ prüfen
        if (ancestorType == null) return null;

        object? ancestor = sheet.FindAncestor(ancestorType);
        if (ancestor == null) return null;

        // 3️⃣ Cast & Lookup
        if (ancestor is IResourceContainer container)
        {
            if (container.TryGetResource<T>(_name, out var value))
                return value;
        }

        return null;
    }
}