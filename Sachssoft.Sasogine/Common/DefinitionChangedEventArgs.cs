using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Common
{
    public class DefinitionChangedEventArgs  : EventArgs
    {

        public DefinitionChangedEventArgs()
        {
        }

        [SetsRequiredMembers]
        public DefinitionChangedEventArgs(string key, object? oldValue, object? newValue)
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public required string Key { get; init; }

        public object? OldValue { get; init; }

        public object? NewValue { get; init; }

    }
}
