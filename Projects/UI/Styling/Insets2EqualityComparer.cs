using System.Collections.Generic;

namespace Sachssoft.Engine.Controls.Styling;
/// <summary>Compares engine insets component-wise without modifying or boxing the engine value type.</summary>
internal sealed class Insets2EqualityComparer : IEqualityComparer<Insets2>
{
    internal static readonly Insets2EqualityComparer Instance = new();
    /// <summary>Performs equals using the current control state.</summary>
    public bool Equals(Insets2 a, Insets2 b) => a.Left == b.Left && a.Top == b.Top && a.Right == b.Right && a.Bottom == b.Bottom;
    /// <summary>Performs get hash code using the current control state.</summary>
    public int GetHashCode(Insets2 value) => System.HashCode.Combine(value.Left, value.Top, value.Right, value.Bottom);
}
