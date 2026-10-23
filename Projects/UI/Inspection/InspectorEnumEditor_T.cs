namespace Sachssoft.Engine.Controls.Inspection;

/// <summary>
/// Caches the declared choices and shared editor factory for a runtime enum type.
/// </summary>
/// <typeparam name="T">The statically bound inspector value type.</typeparam>
internal static class InspectorEnumEditor<T>
{
    internal static readonly InspectorEditorFactory<T>? Factory = CreateFactory();

    private static InspectorEditorFactory<T>? CreateFactory()
    {
        if (!typeof(T).IsEnum)
            return null;

        // Names and parsing preserve aliases and all integral underlying types.
        // Unlike Enum.GetValues(Type), these APIs require no runtime array generation.
        string[] names = Enum.GetNames(typeof(T));
        var choices = new KeyValuePair<string, T>[names.Length];
        for (int i = 0; i < names.Length; i++)
            choices[i] = new(names[i], (T)Enum.Parse(typeof(T), names[i]));

        return (view, title, read, write, source, propertyName) =>
            view.AddChoice(title, read, write, choices, source, propertyName);
    }
}
