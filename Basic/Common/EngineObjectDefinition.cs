using Sachssoft.Sasogine.Components.Models;
using System;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Common
{
    public class EngineObjectDefinition : IEngineObjectDefinition
    {
        [Category(Categories.Common)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [Category(Categories.Common)]
        public string? Class { get; set; }
    }
}
