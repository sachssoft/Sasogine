using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Basic.Common
{
    public interface IHasDirtyMark
    {
        DirtyMark Dirty { get; }
    }
}