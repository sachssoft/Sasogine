using sachssoft.Sasogine.Surface;
using System;

namespace sachssoft.Sasogine.Elements;

public class PropertyMetadata
{
    /// <summary>
    /// Anzeigename der Property im UI, z.B. PropertyGrid.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Beschreibung oder Tooltip für die Property.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Kategorie, z.B. "Appearance", "Layout", "Physics" für Gruppierung.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Optionaler Editor für die Property, z.B. Farbauswahl, Slider, Dropdown etc.
    /// </summary>
    public IPropertyEditor? Editor { get; init; }

    /// <summary>
    /// Alternativer Name, z.B. für Serialisierung (JSON, XML).
    /// Wenn null, wird automatisch konvertiert.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gibt an, ob die Property im UI editierbar ist.
    /// </summary>
    public bool IsReadOnly { get; init; } = false;

    /// <summary>
    /// Standardwert der Property (optional).
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Validierungsfunktion, die den Wert prüft.  
    /// Liefert true bei gültigem Wert, false sonst.
    /// </summary>
    public Func<object?, bool>? Validator { get; init; }

    /// <summary>
    /// Validierungsfunktion: Liefert bool (gültig) und optional Fehlermeldung.
    /// </summary>
    public Func<object?, (bool IsValid, string? ErrorMessage)>? ValidatorWithMessage { get; init; }

    /// <summary>
    /// Optionaler Callback, der nach erfolgreicher oder fehlgeschlagener Validierung ausgeführt wird.
    /// </summary>
    public Action<object?, bool, string?>? ValidationCallback { get; init; }
}
