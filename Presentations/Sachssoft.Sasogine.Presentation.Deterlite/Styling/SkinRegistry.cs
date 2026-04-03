using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    public class SkinRegistry
    {
        private Dictionary<Type, object?> _factories = new();

        public void Register<T>(Type targetType, ITypeTemplate<> template) where T : SkinEntry
        {
            
        }

        public TEntry Create<TEntry>(Type targetType, string? id) where TEntry : SkinEntry
        {

        }
    }
}
