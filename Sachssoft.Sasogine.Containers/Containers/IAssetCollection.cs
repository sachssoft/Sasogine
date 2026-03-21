using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Assets;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Containers
{
    public interface IAssetCollection : IEnumerable<IAsset>, INotifyingEnumerable<IAsset>
    {
    }

    public interface IAssetCollection<T> : IEnumerable<T>, INotifyingEnumerable<T> where T : IAsset
    {
    }
}
