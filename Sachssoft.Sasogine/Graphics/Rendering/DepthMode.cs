namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Defines how depth testing and depth writing are handled during rendering.
    /// </summary>
    public enum DepthMode
    {
        /// <summary>
        /// Depth test is disabled, depth writes are disabled.
        /// Used for HUD, UI, and screen overlays that should ignore depth.
        /// </summary>
        Disabled,     // Kein Test, kein Write → HUD, UI, Overlays

        /// <summary>
        /// Depth test is enabled and depth writes are enabled.
        /// Standard mode for opaque geometry.
        /// </summary>
        Opaque,       // Standard: Test + Write → normale undurchsichtige Geometrie

        /// <summary>
        /// Depth test is enabled but depth writes are disabled.
        /// Used for transparent objects (glass, particles, alpha blending).
        /// </summary>
        Transparent,  // Alpha-Blending: Test ja, Write nein → transparente Objekte

        /// <summary>
        /// Depth test is disabled and depth writes are disabled.
        /// Used for elements that should always render on top, such as crosshairs or 2D overlays.
        /// </summary>
        Overlay,      // Immer sichtbar: Test nein, Write nein → Crosshair, Overlay

        /// <summary>
        /// Depth test is enabled and depth writes are enabled, but color output may be disabled.
        /// Typically used for depth pre-passes or shadow map rendering.
        /// </summary>
        DepthOnly     // Nur Tiefe: Test + Write (optional ColorWrite = None) → Shadow/Prepass
    }
}