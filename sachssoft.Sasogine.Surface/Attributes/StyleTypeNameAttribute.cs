using System;

namespace sachssoft.Sasogine.Surface.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class StyleTypeNameAttribute : Attribute
{
    private readonly string _name;

    public string Name
    {
        get => _name;
    }

    public StyleTypeNameAttribute(string name)
    {
        _name = name;
    }
}
