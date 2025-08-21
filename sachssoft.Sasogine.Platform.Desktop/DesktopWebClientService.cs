using System;
using System.Diagnostics;
using Sachssoft.Sasogine.Services;

namespace Sachssoft.Sasogine.Platform.Desktop;

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
