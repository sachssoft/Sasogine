using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Common.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector;

public class VectorShapeDefinition : IDefinition
{

    [Browsable(false)]
    public TrackableCollection<VectorPathDefinition> Paths { get; } = [];

    [Category(nameof(CategoryAttribute.Design))]
    [DisplayName("Is Locked")]
    public bool IsLocked { get; set; }
}
