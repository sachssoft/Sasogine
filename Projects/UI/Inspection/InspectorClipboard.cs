namespace Sachssoft.Engine.Controls.Inspection;

/// <summary>Stores one typed property value for copy/paste inside a UI registry, without reflection or conversion.</summary>
public sealed class InspectorClipboard
{
    private object? _key, _value;
    /// <summary>Copies a value under its exact generic identity; structs are boxed only on this explicit operation.</summary>
    public void Copy<T>(T value) { _key = InspectorEditorKey<T>.Value; _value = value; }
    /// <summary>Checks whether the stored value has the requested exact type.</summary>
    public bool Contains<T>() => ReferenceEquals(_key, InspectorEditorKey<T>.Value);
    /// <summary>Attempts to read a value of the exact copied type, including a copied null.</summary>
    public bool TryGet<T>(out T value)
    {
        if (!Contains<T>()) { value = default!; return false; }
        value = (T)_value!; return true;
    }
    /// <summary>Releases the copied value.</summary>
    public void Clear() { _key = null; _value = null; }
}
