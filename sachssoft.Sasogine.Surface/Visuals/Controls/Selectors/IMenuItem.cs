using System.ComponentModel;
using System.Xml.Serialization;
using sachssoft.Sasogine.Surface.MML;

namespace sachssoft.Sasogine.Surface.Visuals.Controls;

public interface IMenuItem : IItemWithId
{
    [Browsable(false)]
    [XmlIgnore]
    Menu Menu { get; set; }

    [Browsable(false)]
    [XmlIgnore]
    char? UnderscoreChar { get; }

    [Browsable(false)]
    [XmlIgnore]
    int Index { get; set; }
}
