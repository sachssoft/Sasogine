namespace Sachssoft.Engine.Controls;
/// <summary>A cached line fragment with a premeasured horizontal offset.</summary>
public readonly record struct ScriptToken(int Line, int Start, string Text, ScriptTokenKind Kind, float Offset)
{
    /// <summary>Gets an optional highlighter color overriding the editor palette.</summary>
    public Color? Color { get; init; }
}
