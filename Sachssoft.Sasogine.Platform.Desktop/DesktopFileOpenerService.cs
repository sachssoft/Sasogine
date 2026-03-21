using System;
using System.Diagnostics;
using Sachssoft.Sasogine.Services;

namespace Sachssoft.Sasogine.Platform.Desktop;

public class DesktopFileOpenerService : ILocalFileOpenerService
{
    public void Open(string path)
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                Process.Start("explorer.exe", path);
                return;

        }

        throw new NotImplementedException();
    }
}
