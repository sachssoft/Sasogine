using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Embedding
{

    // Das ist eine Interface für das Interop / Renderhosting
    public interface IEmbeddedView
    {
        public void SetMouseState(EmbeddedMouseState state);
    }
}
