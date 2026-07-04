using System;

namespace Sachssoft.Sasogine.Common.Models
{
    public class EngineObjectDefinition : IEngineObjectDefinition
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        public string? Class { get; set; }
    }
}
