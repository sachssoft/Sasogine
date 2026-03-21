using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Colors;
using Sachssoft.Sasogine.Surface.Visuals;

namespace Sachssoft.Sasogine.Surface.Styles
{
    internal class ValueConverterHelpers
    {
        public static bool TryParseColor(string? value, out Color result)
        {
            // 1️⃣ Versuch: Hex-, RGB-, RGBA- oder sonstige Werte über ColorUtils
            if (ColorUtils.TryParse(value, out result))
                return true;

            // 2️⃣ Versuch: Standard-Farbnamen (AOT-sicher, Dictionary Lookup)
            if (ColorNameList.Colors.TryGetValue(value ?? string.Empty, out result))
                return true;

            // 3️⃣ Kein Treffer
            result = default;
            return false;
        }

    }
}
