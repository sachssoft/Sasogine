using System;

namespace sachssoft.Sasogine.Surface.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class StylePropertyPathAttribute : Attribute
{
    private readonly string _name;

    public string Name
    {
        get => _name;
    }

    public StylePropertyPathAttribute(string name)
    {
        _name = name;
    }
}