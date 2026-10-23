namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Links an immutable index range into its texture group.
/// </summary>
internal struct ControlBatchItem
{
    internal int Start, Count, Next;
}
