namespace Sachssoft.Engine.Controls;
/// <summary>
/// Represents script syntax highlighter.
/// </summary>
public sealed class ScriptSyntaxHighlighter : ISyntaxHighlighter
{
    private static readonly ScriptSyntaxHighlighter PlainText = new(ScriptLanguage.PlainText);
    private static readonly ScriptSyntaxHighlighter Lua = new(ScriptLanguage.Lua);
    private static readonly ScriptSyntaxHighlighter Python = new(ScriptLanguage.Python);
    private static readonly ScriptSyntaxHighlighter CSharp = new(ScriptLanguage.CSharp);
    private ScriptSyntaxHighlighter(ScriptLanguage language) => Language = language;
    /// <summary>
    /// Gets the immutable built-in language.
    /// </summary>
    public ScriptLanguage Language { get; }

    /// <summary>
    /// Gets a shared stateless highlighter; no registration or reflection is required.
    /// </summary>
    public static ScriptSyntaxHighlighter ForLanguage(ScriptLanguage language) => language switch
    {
        ScriptLanguage.PlainText => PlainText,
        ScriptLanguage.Lua => Lua,
        ScriptLanguage.Python => Python,
        ScriptLanguage.CSharp => CSharp,
        _ => throw new ArgumentOutOfRangeException(nameof(language))};
    /// <inheritdoc/>
    public void Highlight(string text, List<SyntaxSpan> spans)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(spans);
        ScriptLexer.Highlight(text, Language, spans);
    }
}
