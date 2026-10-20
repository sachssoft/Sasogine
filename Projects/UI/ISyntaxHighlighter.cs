namespace Sachssoft.Engine.Controls;
/// <summary>Classifies source ranges without depending on a control, renderer or language registry.</summary>
public interface ISyntaxHighlighter
{
    /// <summary>Appends ordered, non-overlapping UTF-16 ranges to the supplied empty buffer.</summary>
    /// <param name = "text">The complete normalized source, using LF line endings.</param>
    /// <param name = "spans">Caller-owned output. Do not retain this buffer or modify it after returning.</param>
    /// <remarks>
    /// Ranges may cross lines. Unclassified gaps use the editor's normal text color.
    /// Implementations run synchronously on the UI thread when highlighting is invalidated.
    /// </remarks>
    void Highlight(string text, List<SyntaxSpan> spans);
}
