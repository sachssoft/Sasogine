//using System.Runtime.InteropServices;

//namespace Sachssoft.Sasogine.Input
//{
//    // ---------------------
//    // Universelle Strings für alle OS
//    // ---------------------
//    public struct ShortcutStringProvider
//    {
//        public string Ctrl;
//        public string Alt;
//        public string Shift;
//        public string Gamepad;

//        public static ShortcutStringProvider GetUniversalProvider()
//        {
//            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
//            {
//                return new ShortcutStringProvider
//                {
//                    Ctrl = "⌘",   // Command-Taste
//                    Alt = "⌥",    // Option-Taste
//                    Shift = "⇧",
//                    Gamepad = "Gamepad"
//                };
//            }

//            // Windows / Linux
//            return new ShortcutStringProvider
//            {
//                Ctrl = "Ctrl",
//                Alt = "Alt",
//                Shift = "Shift",
//                Gamepad = "Gamepad"
//            };
//        }
//    }
//}
