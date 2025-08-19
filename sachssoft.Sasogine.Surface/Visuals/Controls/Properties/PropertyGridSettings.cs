using AssetManagementBase;
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Sachssoft.Sasogine.Surface.Visuals.Controls;

public class PropertyGridSettings
{
    [Browsable(false)]
    [XmlIgnore]
    public AssetManager AssetManager;

    public string BasePath;

    public Func<string, string> ImagePropertyValueGetter;
    public Action<string, string> ImagePropertyValueSetter;
}
