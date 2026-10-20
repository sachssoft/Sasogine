namespace Sachssoft.Engine.Controls.Examples;

/// <summary>Demonstrates automatic enum choices, aliases, flags and a UInt64 high bit.</summary>
[Flags]
internal enum InspectorExampleMode : ulong
{
    None = 0,
    Read = 1,
    Write = 2,
    ReadWrite = Read | Write,
    Both = ReadWrite,
    HighBit = 1UL << 63
}
