using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Demonstrates custom script keywords while reusing Lua string, number and comment rules.</summary>
public sealed class GallerySyntaxHighlighter : ISyntaxHighlighter
{
    /// <inheritdoc/>
    public void Highlight(string text, List<SyntaxSpan> spans)
    {
        ScriptSyntaxHighlighter.ForLanguage(ScriptLanguage.Lua).Highlight(text, spans);
        for (int i = 0; i < spans.Count; i++)
        {
            SyntaxSpan span = spans[i];
            if (span.Kind != ScriptTokenKind.Text)
                continue;
            ReadOnlySpan<char> word = text.AsSpan(span.Start, span.Length);
            if (word.SequenceEqual("entity".AsSpan()) || word.SequenceEqual("move".AsSpan()) || word.SequenceEqual("wait".AsSpan()) || word.SequenceEqual("say".AsSpan()))
                spans[i] = span with
                {
                    Kind = ScriptTokenKind.Keyword,
                    Color = new Color(220, 150, 235)
                };
        }
    }
}
