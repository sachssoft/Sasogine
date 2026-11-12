# Schriften

Ein `TextBlock` oder `Button` verwendet ohne Font-Konfiguration **Noto Sans Regular, 16 logische Einheiten**. `ViewSceneBase` übernimmt die Standardschrift. Eigene Hosts können `workspace.Update(frame)` und `new ControlDrawContext(application, viewport)` verwenden. Der DrawContext übernimmt die beim Layout verwendete Schrift.

```csharp
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Graphics.Text;

var title = new TextBlock
{
    Text = "Sasogine",
    Font = BuiltInFonts.GetFont(BuiltInFont.Serif, 24f, FontStyle.Italic)
};
```

`BuiltInFonts` liefert auch `Sans` und `Mono`. Die eingebetteten Familien lassen sich per `FontName` als `Noto Sans`, `Noto Serif` und `Noto Sans Mono` auswählen. Mono verwendet bei fehlender Kursivvariante seine Regular-Schrift mit fester Zeichenbreite.

## Eigene Familien und Styles

```csharp
workspace.Fonts.Register("App Text",
    new FontFace(File.ReadAllBytes("App-Regular.ttf"), "App Regular"),
    new FontFace(File.ReadAllBytes("App-Bold.ttf"), "App Bold", FontWeight.Bold));

workspace.Content.FontName = "App Text";
workspace.Content.FontSize = 18;
```

`FontName`, `FontSize`, `FontWeight` und `FontStyle` können am Root, in Styles und an einzelnen Controls gesetzt werden. Popups und Tooltips erben vom Besitzer. Eine explizite `Font`-Zuweisung beginnt einen neuen Schriftbereich. `Font = null` stellt die normale Auflösung wieder her. Unbekannte Familien werden beim Layout mit ihrem Namen gemeldet.

Eine fehlende Variante einer proportionalen Familie kann einen passenden Noto-Schnitt verwenden. Familien mit `FontFamily.IsMonospaced = true` behalten ihre feste Zeichenbreite. Die aufgelöste Schrift wird für Messung und Zeichnen verwendet.

## Backend und Lebensdauer

Eigene Hosts können einen `IFontBackend` an `UIRegistry` übergeben. Anwendungen können ihn als Dienst registrieren, beispielsweise über `configuration.Services.AddSingleton<IFontBackend>(backend)`. Controls verwendet die gemeinsame Font-API, einschließlich der nativen Glyphenübergabe für den FrameBatch. Andere `IFont`-Implementierungen können über SpriteBatch zeichnen.

Font-Anfragen mit gleicher Größe und gleichem Schnitt verwenden dieselbe Instanz. Der Standard-Backend verwaltet Glyphenatlanten je Grafikgerät und gibt sie beim Reset oder Dispose des Geräts frei. Der Workspace leiht Fonts und Backends. Änderungen an registrierten Familien werden beim nächsten Layout erkannt.

Die eingebetteten Noto-Schriften stehen unter der SIL Open Font License 1.1. [Lizenz](../../Assets/Fonts/OFL-Noto.txt)
