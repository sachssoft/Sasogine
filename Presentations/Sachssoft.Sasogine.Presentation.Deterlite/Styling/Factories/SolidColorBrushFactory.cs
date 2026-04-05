using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Rendering;
using System;

namespace Sachssoft.Sasogine.Presentation.Styling
{
    internal class SolidColorBrushFactory : ITypeFactory<SolidColorBrush, Resource>
    {
        public SolidColorBrush Create(Skin skin, Resource entry)
        {
            Color color = default;
            float opacity = 1f; // Default-Wert, falls nicht gesetzt

            foreach (var property in entry.Properties)
            {
                switch (property.Name)
                {
                    case nameof(SolidColorBrush.Color):
                        if (property.Value is Color clr)
                            color = clr;
                        else if (property.Value is string str)
                            color = ColorUtils.FromHex(str);
                        break;

                    case nameof(SolidColorBrush.Opacity):
                        if (property.Value is float o)
                            opacity = o;
                        break;
                }
            }

            return new SolidColorBrush(color, opacity);
        }
    }
}
