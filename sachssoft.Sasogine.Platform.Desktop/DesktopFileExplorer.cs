using System;
using System.Diagnostics;
using sachssoft.Sasogine.Providers;

namespace sachssoft.Sasogine.Platform.Desktop;

public class DesktopFileExplorer : IFileExplorerProvider
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
