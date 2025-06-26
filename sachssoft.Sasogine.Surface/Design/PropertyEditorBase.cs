using sachssoft.Sasogine.Surface.Forms;
using sachssoft.Sasogine.Surface.Visuals.Controls;
using System;
using System.ComponentModel;

namespace sachssoft.Sasogine.Surface.Design;

public abstract class PropertyEditorBase
{
    internal PropertyListBuilder _builder;

    public PropertyEditorBase()
    {
    }

    public virtual bool IsDisplayLabelVisibilty { get; } = true;

    public object? Source { get; internal set; }

    public string DisplayLabel { get; internal set; }

    public virtual void Update(object? new_value)
    {
    }

    public virtual Widget CreateControl<T>(
        Action<T, string> changed,
        Func<T, object?> getter,
        Action<T, object?> setter)
    {
        return new Widget(); // Platzhalter
    }
}
