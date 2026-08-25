using System;

namespace Sachssoft.Sasogine.Components.Tools
{
    public sealed class VectorPathEventArgs : EventArgs
    {
        public VectorPathEventArgs(ToolVectorPath path)
        {
            Path = path;
        }

        public ToolVectorPath Path { get; }
    }
}