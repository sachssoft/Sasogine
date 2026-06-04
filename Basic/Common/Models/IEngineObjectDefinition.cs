using System;

namespace Sachssoft.Sasogine.Common
{
    public interface IEngineObjectDefinition
    {
        string? Id { get; set; }

        string? Class { get; set; }
    }
}
