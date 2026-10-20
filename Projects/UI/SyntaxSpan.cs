namespace Sachssoft.Engine.Controls;
/// <summary>A classified UTF-16 source range with an optional application-defined color.</summary>
/// <param name = "Start">Zero-based index into the complete source.</param>
/// <param name = "Length">Number of UTF-16 code units in the range.</param>
/// <param name = "Kind">The editor palette category.</param>
/// <param name = "Color">An optional color overriding the category's style color.</param>
public readonly record struct SyntaxSpan(int Start, int Length, ScriptTokenKind Kind = ScriptTokenKind.Text, Color? Color = null);
