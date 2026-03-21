using System;
using System.Diagnostics;


namespace Sachssoft.Sasogine.Platform.Windows
{
    public class WindowsWebClientService : IWebClientService
    {
        public void Open(Uri uri)
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
        }
    }
}
