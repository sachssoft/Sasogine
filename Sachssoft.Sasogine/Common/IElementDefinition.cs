using System;

namespace Sachssoft.Sasogine.Common
{
    public interface IElementDefinition
    {
        event EventHandler<DefinitionChangedEventArgs>? Changed;

        string? Id { get; set; }

        string? Class { get; set; }
    }
}
