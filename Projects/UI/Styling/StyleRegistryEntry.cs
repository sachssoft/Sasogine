using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls.Styling;
internal sealed class StyleRegistryEntry(Style style)
{
    internal Style Style = style;
    internal int Version = style.Version;
}
