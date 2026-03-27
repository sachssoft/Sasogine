namespace Sachssoft.Sasogine.Experimental.Platform
{
    public interface IPlatformWindow
    {
        void Initialize();      // Fenster + Context aufsetzen
        void Poll();            // alle Input/OS-Events abarbeiten
        void Present();         // fertiges Frame anzeigen
        void Close();           // Fenster schließen

        nint Handle { get; }
    }
}
