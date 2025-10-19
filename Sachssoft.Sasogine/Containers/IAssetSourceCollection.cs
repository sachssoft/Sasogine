using Sachssoft.Inspection;
using Sachssoft.Sasogine.Assets;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Containers
{
    public interface IAssetSourceCollection : IEnumerable<IAssetSource>, INotifyingEnumerable<IAssetSource>
    {
    }
}
