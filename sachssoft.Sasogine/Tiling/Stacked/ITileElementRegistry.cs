using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace sachssoft.Sasogine.Tiling.Stacked;

public interface ITileElementRegistry : IEnumerable<ITileElement>
{
    bool TryGetTile(uint id, [MaybeNullWhen(false)] out ITileElement? tile);

    bool TryGetId(ITileElement tile, out uint id);
}
