using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common
{
    public class ReferenceRegistry<T> where T : class
    {
        public event EventHandler? Registered;
        public event EventHandler? Unregistered;

        public void Register()
        {

        }

        public void Unregister() { }

        public T? Find(string id)
        {
            throw new NotImplementedException();
        }
    }
}
