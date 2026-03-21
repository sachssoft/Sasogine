using System;

namespace Sachssoft.Sasogine.Containers
{
    [Flags]
    public enum PackageEntryFlags
    {
        None = 0,
        OverwriteExisting = 1,
        CreateIfNotExists = 2,
        CreateEmptyEntryIfMissing = 4
    }
}
