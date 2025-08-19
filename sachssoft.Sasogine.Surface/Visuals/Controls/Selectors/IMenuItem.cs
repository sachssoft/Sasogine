using System.ComponentModel;
using System.Xml.Serialization;
using Sachssoft.Sasogine.Surface.MML;

namespace Sachssoft.Sasogine.Surface.Visuals.Controls;

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
