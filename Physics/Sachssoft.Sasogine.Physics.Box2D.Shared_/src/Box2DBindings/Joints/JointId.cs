using System.Runtime.InteropServices;

namespace Box2D;

[StructLayout(LayoutKind.Sequential)]
struct JointId
{
    internal int index1;
    internal ushort world;
    internal ushort generation;
}