using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sachssoft.Sasogine.Common.Models
{
    public interface IReference
    {
        Type TargetType { get; }

        string? Id { get; set; }

        object? Resolve(IEngineObjectResolverProvider provider);
    }
}
