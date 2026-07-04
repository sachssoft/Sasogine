using Sachssoft.Sasogine.Resources;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    public sealed class Style : IResourceTreeEntry
    {

        public IReadOnlySet<IResourceTreeEntry> Children => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public string? Class => throw new NotImplementedException();

        public Type TargetType => throw new NotImplementedException();

        public PropertySet Properties => throw new NotImplementedException();

        public IResourceTreeEntry? GetChild(string id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IResourceTreeEntry> GetChildrenByClass(string className)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IResourceTreeEntry> GetDescendants()
        {
            throw new NotImplementedException();
        }
    }
}
