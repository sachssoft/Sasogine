// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;

namespace Sachssoft.Sasogine.Surface.Utility.Clipboard
{
    internal enum OS
    {
        Windows,
        Linux,
        MacOSX,
        Unknown
    }

    internal static class CurrentPlatform
    {
        private static bool init = false;
        private static OS os;

        [DllImport("libc")]
        static extern int uname(nint buf);

        private static void Init()
        {
            if (!init)
            {
                PlatformID pid = Environment.OSVersion.Platform;

                switch (pid)
                {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                        os = OS.Windows;
                        break;
                    case PlatformID.MacOSX:
                        os = OS.MacOSX;
                        break;
                    case PlatformID.Unix:

                        // Mac can return a value of Unix sometimes, We need to double check it.
                        nint buf = nint.Zero;
                        try
                        {
                            buf = Marshal.AllocHGlobal(8192);

                            if (uname(buf) == 0)
                            {
                                string sos = Marshal.PtrToStringAnsi(buf);
                                if (sos == "Darwin")
                                {
                                    os = OS.MacOSX;
                                    return;
                                }
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            if (buf != nint.Zero)
                                Marshal.FreeHGlobal(buf);
                        }

                        os = OS.Linux;
                        break;
                    default:
                        os = OS.Unknown;
                        break;
                }

                init = true;
            }
        }

        public static OS OS
        {
            get
            {
                Init();
                return os;
            }
        }
    }
}

