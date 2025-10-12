using Microsoft.Xna.Framework.Media;
using Sachssoft.Observables;

namespace Sachssoft.Sasogine.Assets
{
    public class MusicAsset : AssetBase<Song>, ITypeRegistry
    {
        static void ITypeRegistry.RegisterProperties(TypeRegistryContext context)
        {
        }
    }
}
