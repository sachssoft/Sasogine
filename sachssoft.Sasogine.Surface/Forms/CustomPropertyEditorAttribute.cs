using System;
using Sachssoft.Sasogine.Surface.Design;

namespace Sachssoft.Sasogine.Surface.Forms;
[Obsolete("Remove!!")]

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class CustomPropertyEditorAttribute : Attribute
{

    public CustomPropertyEditorAttribute(Type property_editor_type)
    {
        if (property_editor_type == null)
            throw new ArgumentNullException();

        if (!property_editor_type.IsAssignableTo(typeof(PropertyEditorBase)))
            throw new ArgumentException("Invalid type of the custom property editor");

        Type = property_editor_type;
    }

    public Type Type
    {
        get;
        set;
    }

}
