using Sachssoft.Sasogine.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sachssoft.Sasogine.Diagnostics.Internals
{
    internal sealed class DesktopApplicationDebug : IApplicationDebug
    {
        public void Flush()
        {
            Console.Out.Flush();
        }

        public void Clear()
        {
            Console.Clear();
        }

        public void Write(string? message)
        {
            Console.Write(message);
        }

        public void WriteLine(string? message)
        {
            Console.WriteLine(message);
        }
    }
}
