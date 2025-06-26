using System;
using System.Diagnostics;
using sachssoft.Sasogine.Providers;

namespace sachssoft.Sasogine.Platform.Desktop;

public class DesktopWebBrowser : IWebBrowserProvider
{
    public void Open(Uri uri)
    {
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
        {
            UseShellExecute = true
        });
    }
}
