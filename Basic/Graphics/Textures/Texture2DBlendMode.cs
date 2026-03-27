namespace Sachssoft.Sasogine.Graphics
{
    public enum Texture2DBlendMode
    {
        /// <summary>
        /// Standardmäßiges Alpha-Blending (häufig für transparente Texturen).
        /// </summary>
        AlphaBlend,

        /// <summary>
        /// Additives Blending (Farben werden addiert, gut für Effekte wie Licht).
        /// </summary>
        Additive,

        /// <summary>
        /// Nicht-prämultipliziertes Alpha-Blending.
        /// </summary>
        NonPremultiplied,

        /// <summary>
        /// Kein Blending, die Textur überschreibt die Pixel direkt.
        /// </summary>
        Opaque,

        /// <summary>
        /// Benutzerdefiniertes BlendState, wird aus CustomBlendState verwendet.
        /// </summary>
        Custom

    }
}
