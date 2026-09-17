namespace Sachssoft.Sasogine.Diagnostics.Internals
{
    internal sealed class NullApplicationDebug : IApplicationDebug
    {
        public void Flush()
        {
        }

        public void Clear()
        {
        }

        public void Write(string? message)
        {
        }

        public void WriteLine(string? message)
        {
        }
    }
}
