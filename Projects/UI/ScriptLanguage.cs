namespace Sachssoft.Engine.Controls;
/// <summary>
/// Selects a deterministic built-in lexer without loading language services.
/// </summary>
public enum ScriptLanguage
{
    /// <summary>
    /// Uncolored text.
    /// </summary>
    PlainText,
    /// <summary>
    /// Lua keywords, comments and long strings.
    /// </summary>
    Lua,
    /// <summary>
    /// Python keywords, comments and triple-quoted strings.
    /// </summary>
    Python,
    /// <summary>
    /// C# keywords, comments and quoted strings.
    /// </summary>
    CSharp
}
