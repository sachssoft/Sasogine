namespace Sachssoft.Sasogine.Presentation.Deterlite
{
    public enum FrameLayer
    {
        Background = 0,   // Hintergründe, Panels, Fenster-Backgrounds
        Content = 1,      // Normale UI-Inhalte (Buttons, Labels, Listen, etc.)
        Overlay = 2,      // UI über dem Content (Toolbar, HUD, Statusanzeigen)
        Popup = 3,        // Kontextmenüs, Dropdowns, Dialoge
        Tooltip = 4,      // Tooltips, Hilfe-Overlays
        Debug = 5         // Debug-UI, Layout-Visualizer
    }
}
