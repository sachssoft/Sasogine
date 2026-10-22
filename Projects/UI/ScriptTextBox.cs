namespace Sachssoft.Engine.Controls;
/// <summary>An editable multiline document with cached built-in or application-defined syntax highlighting.</summary>
public class ScriptTextBox : TextBox
{
    private string? _cachedText;
    private ISyntaxHighlighter? _highlighter, _cachedHighlighter;
    private bool _highlightingDirty = true;
    private readonly List<SyntaxSpan> _spans = new();
    private static readonly System.ComponentModel.PropertyChangedEventArgs HighlighterArgs = new(nameof(Highlighter));
    private IControlFont? _cachedFont;
    private List<ScriptToken> _tokens = new();
    private IReadOnlyList<ScriptToken> _view = Array.Empty<ScriptToken>();
    private readonly List<(int Start, int Count)> _lines = new();
    private readonly List<string> _displayTokens = new();
    /// <summary>Creates an editor with multiline and tab entry enabled.</summary>
    public ScriptTextBox()
    {
        AcceptsReturn = true;
        AcceptsTab = true;
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ScriptTextBox";

    /// <summary>Gets or sets the language. Tokenization is refreshed at the next layout boundary.</summary>
    public ScriptLanguage Language
    {
        get => _Language.Value;
        set
        {
            if (_Language.SetLocal(value))
                NotifyPropertyChanged(LanguageArgs);
        }
    }

    private StyleProperty<ScriptLanguage> _Language = new(ScriptLanguage.Lua);
    private static readonly System.ComponentModel.PropertyChangedEventArgs LanguageArgs = new(nameof(Language));
    /// <summary>Gets or sets a custom highlighter; null uses the built-in Language selection.</summary>
    /// <remarks>Assign an instance directly. No language registration or reflection is required.</remarks>
    public ISyntaxHighlighter? Highlighter
    {
        get => _highlighter;
        set
        {
            if (ReferenceEquals(_highlighter, value))
                return;
            _highlighter = value;
            NotifyPropertyChanged(HighlighterArgs);
        }
    }

    /// <summary>Rebuilds highlighting at the next layout boundary after custom rules change.</summary>
    /// <remarks>Call this for each editor using a mutable shared highlighter. Drawing never invokes the highlighter.</remarks>
    public void InvalidateHighlighting()
    {
        _highlightingDirty = true;
        InvalidateLayout();
    }

    /// <summary>Gets the read-only token snapshot prepared by the latest update/layout.</summary>
    public IReadOnlyList<ScriptToken> Tokens => _view;

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        var size = base.MeasureOverride(context, available);
        Cache(GetFont(context));
        return size;
    }

    /// <inheritdoc/>
    protected override void OnFontChanged()
    {
        _cachedFont = null;
        base.OnFontChanged();
    }

    private void Cache(IControlFont font)
    {
        ISyntaxHighlighter highlighter = Highlighter ?? ScriptSyntaxHighlighter.ForLanguage(Language);
        bool classify = _highlightingDirty || _cachedText != Text || !ReferenceEquals(_cachedHighlighter, highlighter);
        if (!classify && ReferenceEquals(_cachedFont, font))
            return;
        List<ScriptToken> tokens;
        if (classify)
        {
            _spans.Clear();
            highlighter.Highlight(Text, _spans);
            ValidateSpans();
            tokens = CreateTokens();
        }
        else
            tokens = new List<ScriptToken>(_tokens);
        // Commit only after classification and range validation succeed.
        _tokens = tokens;
        _view = _tokens.AsReadOnly();
        _lines.Clear();
        _displayTokens.Clear();
        int current = -1, start = 0;
        float offset = 0;
        for (int i = 0; i < _tokens.Count; i++)
        {
            var token = _tokens[i];
            if (token.Line != current)
            {
                while (_lines.Count < token.Line)
                    _lines.Add((0, 0));
                current = token.Line;
                start = i;
                offset = 0;
                _lines.Add((start, 0));
            }

            _tokens[i] = token with
            {
                Offset = offset
            };
            string visible = token.Text.Replace("\t", "    ", StringComparison.Ordinal);
            _displayTokens.Add(visible);
            offset += font.Measure(visible).Width;
            _lines[current] = (start, i - start + 1);
        }

        _cachedText = Text;
        _cachedHighlighter = highlighter;
        _highlightingDirty = false;
        _cachedFont = font;
    }

    private void ValidateSpans()
    {
        int end = 0;
        for (int i = 0; i < _spans.Count; i++)
        {
            SyntaxSpan span = _spans[i];
            if (span.Start < end || span.Start < 0 || span.Length < 0 || span.Start > Text.Length - span.Length || span.Kind is not (ScriptTokenKind.Text or ScriptTokenKind.Keyword or ScriptTokenKind.Number or ScriptTokenKind.String or ScriptTokenKind.Comment))
                throw new InvalidOperationException("Syntax ranges must be ordered, non-overlapping and inside the source with a known token kind.");
            end = span.Start + span.Length;
        }
    }

    private List<ScriptToken> CreateTokens()
    {
        var tokens = new List<ScriptToken>();
        int position = 0, line = 0;
        for (int i = 0; i < _spans.Count; i++)
        {
            SyntaxSpan span = _spans[i];
            AppendTokens(tokens, ref position, ref line, span.Start, ScriptTokenKind.Text, null);
            AppendTokens(tokens, ref position, ref line, span.Start + span.Length, span.Kind, span.Color);
        }

        AppendTokens(tokens, ref position, ref line, Text.Length, ScriptTokenKind.Text, null);
        return tokens;
    }

    private void AppendTokens(List<ScriptToken> tokens, ref int position, ref int line, int end, ScriptTokenKind kind, Color? color)
    {
        while (position < end)
        {
            if (Text[position] == '\n')
            {
                position++;
                line++;
                continue;
            }

            int start = position;
            while (position < end && Text[position] != '\n')
                position++;
            tokens.Add(new ScriptToken(line, start, Text[start..position], kind, 0) { Color = color });
        }
    }

    /// <inheritdoc/>
    protected override void DrawTextLine(ControlDrawContext context, IControlFont font, string line, int lineIndex, Vector2 position, Color color)
    {
        if (IsPassword)
        {
            base.DrawTextLine(context, font, line, lineIndex, position, color);
            return;
        }

        if (lineIndex >= _lines.Count)
            return;
        var range = _lines[lineIndex];
        for (int i = range.Start; i < range.Start + range.Count; i++)
        {
            var token = _tokens[i];
            Color tint = !IsAvailable() ? color : token.Color ?? (token.Kind switch
            {
                ScriptTokenKind.Keyword => KeywordColor,
                ScriptTokenKind.String => StringColor,
                ScriptTokenKind.Comment => CommentColor,
                ScriptTokenKind.Number => NumberColor,
                _ => color
            });
            context.DrawText(font, _displayTokens[i], new(position.X + token.Offset, position.Y), tint);
        }
    }

    private StyleProperty<Color> _KeywordColor = new(new Color(107, 177, 255));
    private static readonly System.ComponentModel.PropertyChangedEventArgs KeywordColorArgs = new(nameof(KeywordColor));
    /// <summary>Gets the effective KeywordColor value or assigns a local override.</summary>
    public Color KeywordColor
    {
        get => _KeywordColor.Value;
        set
        {
            if (_KeywordColor.SetLocal(value))
                NotifyPropertyChanged(KeywordColorArgs);
        }
    }

    private StyleProperty<Color> _StringColor = new(new Color(215, 164, 112));
    private static readonly System.ComponentModel.PropertyChangedEventArgs StringColorArgs = new(nameof(StringColor));
    /// <summary>Gets the effective StringColor value or assigns a local override.</summary>
    public Color StringColor
    {
        get => _StringColor.Value;
        set
        {
            if (_StringColor.SetLocal(value))
                NotifyPropertyChanged(StringColorArgs);
        }
    }

    private StyleProperty<Color> _CommentColor = new(new Color(115, 162, 104));
    private static readonly System.ComponentModel.PropertyChangedEventArgs CommentColorArgs = new(nameof(CommentColor));
    /// <summary>Gets the effective CommentColor value or assigns a local override.</summary>
    public Color CommentColor
    {
        get => _CommentColor.Value;
        set
        {
            if (_CommentColor.SetLocal(value))
                NotifyPropertyChanged(CommentColorArgs);
        }
    }

    private StyleProperty<Color> _NumberColor = new(new Color(181, 206, 168));
    private static readonly System.ComponentModel.PropertyChangedEventArgs NumberColorArgs = new(nameof(NumberColor));
    /// <summary>Gets the effective NumberColor value or assigns a local override.</summary>
    public Color NumberColor
    {
        get => _NumberColor.Value;
        set
        {
            if (_NumberColor.SetLocal(value))
                NotifyPropertyChanged(NumberColorArgs);
        }
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
            switch (entry.Key)
            {
                case "Language":
                    if (_Language.Apply(Converters.Convert<ScriptLanguage>("ScriptLanguage", entry.Value)))
                        NotifyPropertyChanged(LanguageArgs);
                    break;
                case "KeywordColor":
                    if (_KeywordColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(KeywordColorArgs);
                    break;
                case "StringColor":
                    if (_StringColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(StringColorArgs);
                    break;
                case "CommentColor":
                    if (_CommentColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(CommentColorArgs);
                    break;
                case "NumberColor":
                    if (_NumberColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(NumberColorArgs);
                    break;
            }
    }

    internal override bool IsStyleLocal(string property) => property switch
    {
        "Language" => _Language.HasLocalValue,
        "KeywordColor" => _KeywordColor.HasLocalValue,
        "StringColor" => _StringColor.HasLocalValue,
        "CommentColor" => _CommentColor.HasLocalValue,
        "NumberColor" => _NumberColor.HasLocalValue,
        _ => base.IsStyleLocal(property)};
    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "Language":
                if (_Language.ClearLocal())
                    NotifyPropertyChanged(LanguageArgs);
                break;
            case "KeywordColor":
                if (_KeywordColor.ClearLocal())
                    NotifyPropertyChanged(KeywordColorArgs);
                break;
            case "StringColor":
                if (_StringColor.ClearLocal())
                    NotifyPropertyChanged(StringColorArgs);
                break;
            case "CommentColor":
                if (_CommentColor.ClearLocal())
                    NotifyPropertyChanged(CommentColorArgs);
                break;
            case "NumberColor":
                if (_NumberColor.ClearLocal())
                    NotifyPropertyChanged(NumberColorArgs);
                break;
            default:
                base.ResetStyleProperty(property);
                break;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _Language.BeginStyle(ScriptLanguage.Lua);
        _KeywordColor.BeginStyle(new Color(107, 177, 255));
        _StringColor.BeginStyle(new Color(215, 164, 112));
        _CommentColor.BeginStyle(new Color(115, 162, 104));
        _NumberColor.BeginStyle(new Color(181, 206, 168));
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_Language.CommitStyle())
            NotifyPropertyChanged(LanguageArgs);
        if (_KeywordColor.CommitStyle())
            NotifyPropertyChanged(KeywordColorArgs);
        if (_StringColor.CommitStyle())
            NotifyPropertyChanged(StringColorArgs);
        if (_CommentColor.CommitStyle())
            NotifyPropertyChanged(CommentColorArgs);
        if (_NumberColor.CommitStyle())
            NotifyPropertyChanged(NumberColorArgs);
    }
}
