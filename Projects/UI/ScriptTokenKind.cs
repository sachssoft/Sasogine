namespace Sachssoft.Engine.Controls;
/// <summary>
/// Classifies a cached syntax fragment.
/// </summary>
public enum ScriptTokenKind
{
    /// <summary>
    /// Ordinary text.
    /// </summary>
    Text,
    /// <summary>
    /// A language keyword.
    /// </summary>
    Keyword,
    /// <summary>
    /// A numeric literal.
    /// </summary>
    Number,
    /// <summary>
    /// A string literal.
    /// </summary>
    String,
    /// <summary>
    /// A language comment.
    /// </summary>
    Comment
}
