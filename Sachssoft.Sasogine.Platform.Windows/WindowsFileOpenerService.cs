using System;
using System.Diagnostics;

namespace Sachssoft.Sasogine.Platform.Windows
{
    public class WindowsFileOpenerService : ILocalFileOpenerService
    {
        public void Open(string path)
        {
            Process.Start("explorer.exe", path);
        }
    }
}
