using Sachssoft.Sasogine.Interactions;

namespace Sachssoft.Sasogine.Services
{
    public interface IPlatformModifierService
    {
        string GetModifierString(int index);   // z. B. 0=Ctrl/Cmd, 1=Alt/Option, 2=Shift
        int ModifierCount { get; }             // Anzahl der unterstützten Modifier
        bool IsModifierPressed(int index);     // Aktueller Zustand (optional, falls Runtime-Abfrage)
        string ToString(Shortcut shortcut);    // Shortcut als String darstellen
    }
}
