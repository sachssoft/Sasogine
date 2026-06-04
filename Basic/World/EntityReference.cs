using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.World
{
    public class EntityReference : Reference<IEntity>
    {
        public EntityReference(IEntityCollectionProvider provider) { }

        protected override IEntity? ResolveValue(string id)
        {
            throw new NotImplementedException();
        }
    }
}
