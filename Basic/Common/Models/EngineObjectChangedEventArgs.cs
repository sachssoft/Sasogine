using System;

namespace Sachssoft.Sasogine.Common.Models
{
    public class EngineObjectChangedEventArgs : EventArgs
    {

        public EngineObjectChangedEventArgs(string? oldId, string? newId, string? oldClass, string? newClass)
        {
            OldId = oldId;
            NewId = newId;
            OldClass = oldClass;
            NewClass = newClass;
        }

        public string? OldId { get; }

        public string? NewId { get; }

        public string? OldClass { get; }

        public string? NewClass { get; }
    }
}
