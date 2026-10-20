# Eigene Skriptsprachen

`ScriptTextBox.Highlighter` akzeptiert jede Implementierung von `ISyntaxHighlighter`. Eine Registrierung ist nicht erforderlich; der Aufruf erfolgt direkt und ohne Reflection.

```csharp
var editor = new ScriptTextBox
{
    Highlighter = new MyScriptHighlighter(),
    Text = "move player 10"
};
```

Ein Highlighter erhält den gesamten Text und schreibt `SyntaxSpan`-Bereiche in den bereitgestellten Puffer:

```csharp
public sealed class MyScriptHighlighter : ISyntaxHighlighter
{
    public void Highlight(string text, List<SyntaxSpan> spans)
    {
        // Minimalbeispiel: das erste Vorkommen markieren.
        int start = text.IndexOf("move", StringComparison.Ordinal);
        if (start >= 0)
            spans.Add(new SyntaxSpan(start, 4, ScriptTokenKind.Keyword));
    }
}
```

- `Start` und `Length` beziehen sich auf UTF-16-Indizes im gesamten Text, wie bei C#-Strings.
- Bereiche müssen aufsteigend sortiert sein, dürfen sich nicht überlappen und müssen innerhalb des Textes liegen. Sie dürfen mehrere Zeilen umfassen.
- Nicht markierte Bereiche erscheinen als normaler Text. Ein leerer Puffer bedeutet Klartext.
- `Kind` verwendet die Style-Farben `KeywordColor`, `StringColor`, `CommentColor`, `NumberColor` beziehungsweise `TextColor`.
- Für zusätzliche Kategorien ist eine eigene Farbe möglich: `new SyntaxSpan(start, length, ScriptTokenKind.Text, Color.Magenta)`.
- Der Puffer gehört dem Editor. Nicht speichern oder nach dem Aufruf verändern.

Die Klassifizierung wird bei Textänderungen, beim Wechsel des Highlighters und nach `InvalidateHighlighting()` neu berechnet. Fontwechsel berechnen nur die Textpositionen neu; Style-Farben benötigen keine neue Klassifizierung. Unveränderte Updates und Draws rufen den Highlighter nicht auf.

Wenn eigene Regeln im bereits zugewiesenen Objekt geändert werden:

```csharp
editor.InvalidateHighlighting();
```

Mit `editor.Highlighter = null` gilt wieder `editor.Language`. Auch die eingebauten Sprachen verwenden die Schnittstelle:

```csharp
editor.Highlighter = ScriptSyntaxHighlighter.ForLanguage(ScriptLanguage.Python);
```

Die Galerie zeigt unter **Tools & scripts → Custom script** eine eigene Sprache mit `entity`, `move`, `wait` und `say`. Ihre Implementierung `GallerySyntaxHighlighter` erweitert die Lua-Regeln für Strings, Zahlen und Kommentare. Eigene Parser oder Lexer können stattdessen ihre eigenen Bereiche liefern. Der Editor führt keine Skripte aus.
