# Fonts

Controls verwenden Sasogines vorhandene `FontRegistry`, `FontFamily`, `FontFace`, `FontWeight` und `FontStyle`. Keine zweite Schriftverwaltung und keine Laufzeit-Reflection.

```csharp
using Sachssoft.Engine.Graphics.Text;

workspace.Fonts.Register("Noto Sans",
    new FontFace(File.ReadAllBytes("NotoSans-Regular.ttf"), "Regular"),
    new FontFace(File.ReadAllBytes("NotoSans-Bold.ttf"), "Bold", FontWeight.Bold),
    new FontFace(File.ReadAllBytes("NotoSans-Italic.ttf"), "Italic",
        FontWeight.Normal, FontStyle.Italic),
    new FontFace(File.ReadAllBytes("NotoSans-BoldItalic.ttf"), "Bold Italic",
        FontWeight.Bold, FontStyle.Italic));

workspace.Content.FontName = "Noto Sans";
workspace.Content.FontSize = 18;

var heading = new TextBlock
{
    Text = "Sasogine",
    FontSize = 28,
    FontWeight = FontWeight.Bold
};
```

Registrierung und Grundeinstellung erfolgen vor dem ersten `Update`. Ein bestehendes Engine-Register kann mit `new UIRegistry(fonts)` übergeben werden. Die TTF-Dateien müssen zum Projekt passen und korrekt lizenziert sein.

## Vererbung und Styles

- `FontName`, `FontSize`, `FontWeight` und `FontStyle` stehen auf jedem `Control` zur Verfügung. `null` bedeutet Vererbung vom Parent; Popup und Tooltip übernehmen die Schrift ihres Besitzers.
- Lokale Werte überschreiben Styles. Eine lokale Zuweisung von `null` erzwingt Vererbung; `ResetStyleProperty("FontSize")` aktiviert wieder den Stylewert.
- `Normal` ist ein expliziter Wert: Ein TextBlock kann damit das geerbte `Bold` überschreiben.
- Styles unterstützen dieselben Properties als typisierte Kurzform oder Stringwerte, beispielsweise `style["FontWeight"] = "Bold"` und `style["FontSize"] = "inherit"`. Grundwerte am Root setzen, damit Kindstyles die Vererbung nicht unbeabsichtigt überschreiben.

```csharp
registry.Styles.Register("App.Heading", new Style
{
    BasedOn = "TextBlock",
    FontSize = 28,
    FontWeight = FontWeight.Bold
});
```

Fehlende Varianten folgen dem vorhandenen `FontFamily.GetFace`-Fallback. Unbekannte Familien lösen beim Layout eine verständliche Exception aus. Ohne diese Properties funktioniert der bisherige `IControlFont` aus Layout- und DrawContext unverändert. Eine explizite `control.Font` beginnt einen eigenen, an Kinder vererbbaren Schriftbereich. Für Größen- oder Variantenwechsel bei Bitmapfonts und anderen eigenen Adaptern muss eine benannte Familie verwendet werden.

## Cache und Lebensdauer

Jeder Workspace teilt Glyph-Systeme je FontFace und Adapter je FontFace/Größe. Layout und Zeichnen verwenden dieselbe aufgelöste Schrift; `Draw` sucht keine Familien und erstellt keine Adapter. `EffectiveFont` zeigt die zuletzt aufgelöste Schrift nach dem Layout. Eigene Textcontrols verwenden `GetFont(context)` und können abgeleitete Textmetriken in `OnFontChanged()` verwerfen.

Unveränderte Frames verwenden den Layoutcache. Schriftwechsel invalidieren die betroffenen Layoutpfade. DPI bleibt einheitlich über `Workspace.Dpi`; `FontSize` ist eine logische Größe.

Neue oder entfernte Familien erkennt der Workspace anhand der Anzahl. Nach Austausch einer Familie bei gleicher Anzahl `workspace.InvalidateFonts()` aufrufen. Der Cache wird am nächsten sicheren Layoutzeitpunkt erneuert. Der Workspace entsorgt seine erzeugten Adapter und Glyph-Systeme, aber keine über `Font` oder den Kontext geliehenen Schriften. Ein optionaler `UIRegistry`-Adapter-Factory überträgt die Lebensdauer der zurückgegebenen Adapter an den jeweiligen Workspace; dort keine extern besessenen Adapter zurückgeben.

Das aktualisierte Beispiel `Beginner.09.Controls` besitzt eine Seite **Fonts** mit vier Schriftschnitten, Vererbung und interaktiver Größenwahl. Installierte Systemfonts werden zur Laufzeit gelesen und nicht mitgeliefert.
