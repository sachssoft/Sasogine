using Sachssoft.Sasogine.Surface.Basic;

namespace Sachssoft.Sasogine.Surface.Services
{
    internal static class ClipboardServiceHelper
    {
        public static string GetText()
        {
            var clipboard = UIEnvironment.Clipboard;

            return ""; // Später Implementieren
        }

        public static void SetText(string text)
        {
            var clipboard = UIEnvironment.Clipboard;
        }
    }
}
