using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Deterlite.Layouts;

namespace Sachssoft.Sasogine.Presentation
{
    public readonly struct DebugVisuals
    {
        // Vordefinierte Standardwerte
        public static DebugVisuals Default => new DebugVisuals(
            boundsBorderColor: Color.Lime,
            contentBorderColor: Color.Cyan,
            layerColor: Color.Magenta,
            tooltipColor: Color.Yellow,
            lineThickness: new Insets(2f)
        );

        public DebugVisuals(
            Color boundsBorderColor,
            Color contentBorderColor,
            Color layerColor,
            Color tooltipColor,
            Insets lineThickness)
        {
            BoundsBorderColor = boundsBorderColor;
            ContentBorderColor = contentBorderColor;
            LayerColor = layerColor;
            TooltipColor = tooltipColor;
            LineThickness = lineThickness;
        }

        public Color BoundsBorderColor { get; }

        public Color ContentBorderColor { get; }

        public Color LayerColor { get; }

        public Color TooltipColor { get; }

        public Insets LineThickness { get; }
    }
}
