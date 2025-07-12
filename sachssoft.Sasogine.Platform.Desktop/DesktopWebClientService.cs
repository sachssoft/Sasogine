using System;
using System.Diagnostics;
using sachssoft.Sasogine.Services;

namespace sachssoft.Sasogine.Platform.Desktop;

public class DesktopWebClientService : IWebClientService
{
    public void Open(Uri uri)
    {
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
        {
            UseShellExecute = true
        });
    }
}
