using Sachssoft.Sasogine.Basic.Components.Models;
using Sachssoft.Sasogine.Common;
using System.ComponentModel;

namespace Sachssoft.Sasogine.World;

public abstract class EntityDefinitionBase : IEntityDefinition
{
    [Category(Categories.Common)]
    [DisplayName("Id")]
    public string? Id { get; set; }

    [Category(Categories.Common)]
    [DisplayName("Class")]
    public string? Class { get; set; }
}
