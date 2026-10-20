using Sachssoft.Engine.Controls;

/// <summary>A configurable classifier for cache and range-contract checks.</summary>
internal sealed class TestSyntaxHighlighter : ISyntaxHighlighter
{
    /// <summary>Gets how often classification was requested.</summary>
    public int Calls { get; private set; }
    /// <summary>Gets ranges returned by the next classification request.</summary>
    public List<SyntaxSpan> Spans { get; } = new();

    /// <inheritdoc/>
    public void Highlight(string text, List<SyntaxSpan> spans)
    {
        Calls++;
        spans.AddRange(Spans);
    }
}
