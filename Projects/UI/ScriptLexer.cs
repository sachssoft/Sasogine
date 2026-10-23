namespace Sachssoft.Engine.Controls;
/// <summary>
/// Explicit lexical rules for common script languages; no parsing, execution or reflection.
/// </summary>
internal static class ScriptLexer
{
    private static readonly HashSet<string> Lua = new("and break do else elseif end false for function goto if in local nil not or repeat return then true until while".Split(' '), StringComparer.Ordinal);
    private static readonly HashSet<string> Python = new("False None True and as assert async await break class continue def del elif else except finally for from global if import in is lambda nonlocal not or pass raise return try while with yield match case".Split(' '), StringComparer.Ordinal);
    private static readonly HashSet<string> CSharp = new("abstract as async await base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach if implicit in int interface internal is lock long namespace new null object operator out override params private protected public readonly record ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using var virtual void volatile while yield get set init required".Split(' '), StringComparer.Ordinal);
    internal static void Highlight(string text, ScriptLanguage language, List<SyntaxSpan> spans)
    {
        int i = 0;
        string? terminator = null;
        ScriptTokenKind blockKind = ScriptTokenKind.Text;
        while (i < text.Length)
        {
            if (text[i] == '\n')
            {
                i++;
                continue;
            }

            int start = i;
            ScriptTokenKind kind = ScriptTokenKind.Text;
            if (terminator is not null)
            {
                kind = blockKind;
                ConsumeBlock(text, ref i, terminator, ref terminator);
            }
            else if (language == ScriptLanguage.PlainText)
            {
                while (i < text.Length && text[i] != '\n')
                    i++;
            }
            else if (language == ScriptLanguage.CSharp && At(text, i, "/*") || language == ScriptLanguage.Lua && At(text, i, "--[["))
            {
                kind = ScriptTokenKind.Comment;
                terminator = language == ScriptLanguage.CSharp ? "*/" : "]]";
                blockKind = kind;
                i += language == ScriptLanguage.CSharp ? 2 : 4;
                ConsumeBlock(text, ref i, terminator, ref terminator);
            }
            else if (language == ScriptLanguage.Python && text[i] == '#' || language == ScriptLanguage.Lua && At(text, i, "--") || language == ScriptLanguage.CSharp && At(text, i, "//"))
            {
                kind = ScriptTokenKind.Comment;
                while (i < text.Length && text[i] != '\n')
                    i++;
            }
            else if (language == ScriptLanguage.Lua && At(text, i, "[["))
            {
                kind = ScriptTokenKind.String;
                terminator = "]]";
                blockKind = kind;
                i += 2;
                ConsumeBlock(text, ref i, terminator, ref terminator);
            }
            else if (text[i] is '\'' or '"' || language == ScriptLanguage.CSharp && At(text, i, "@\""))
            {
                kind = ScriptTokenKind.String;
                bool verbatim = text[i] == '@';
                if (verbatim)
                    i++;
                char quote = text[i++];
                if (language == ScriptLanguage.Python && i + 1 < text.Length && text[i] == quote && text[i + 1] == quote)
                {
                    i += 2;
                    terminator = new string (quote, 3);
                    blockKind = kind;
                    ConsumeBlock(text, ref i, terminator, ref terminator);
                }
                else
                {
                    while (i < text.Length && text[i] != '\n')
                    {
                        if (text[i] == quote)
                        {
                            i++;
                            if (verbatim && i < text.Length && text[i] == quote)
                            {
                                i++;
                                continue;
                            }

                            break;
                        }

                        if (!verbatim && text[i] == '\\' && i + 1 < text.Length && text[i + 1] != '\n')
                            i++;
                        i++;
                    }
                }
            }
            else if (char.IsDigit(text[i]))
            {
                kind = ScriptTokenKind.Number;
                i++;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] is '.' or '_'))
                    i++;
            }
            else if (char.IsLetter(text[i]) || text[i] == '_')
            {
                i++;
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_'))
                    i++;
                string word = text[start..i];
                var words = language == ScriptLanguage.Lua ? Lua : language == ScriptLanguage.Python ? Python : CSharp;
                if (words.Contains(word))
                    kind = ScriptTokenKind.Keyword;
            }
            else
            {
                i++;
                while (i < text.Length && text[i] != '\n' && char.IsWhiteSpace(text[i]))
                    i++;
            }

            spans.Add(new(start, i - start, kind));
        }
    }

    private static bool At(string text, int index, string value) => text.AsSpan(index).StartsWith(value.AsSpan(), StringComparison.Ordinal);
    private static void ConsumeBlock(string text, ref int i, string end, ref string? terminator)
    {
        while (i < text.Length && text[i] != '\n')
        {
            if (At(text, i, end))
            {
                i += end.Length;
                terminator = null;
                return;
            }

            i++;
        }
    }
}
