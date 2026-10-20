# Prüfstand Controls / FrameBatch

22 Gruppen und 2.187 Assertions bestehen unter .NET 8.0.31, .NET 9.0.20 und .NET 10.0.12 sowie nach tatsächlichem NativeAOT-Publish und Start. Geprüft werden auch SplitButton-Commands, unabhängige Menüverfügbarkeit, Dropdown-Framezustände, Expander-Besitzwechsel, Werteingabe mit Fokusverlust, Diagramm-Ringpuffer und die Lexer für Lua, Python und C#.

`ValueTextBox` wird mit tatsächlichen Pointer-/Tastaturereignissen auf Klick versus Drag, Zurückziehen, verschobenes Loslassen ohne Move-Event, Touch, Abbruch, Repeat-Pfeile, Grenzen, Eingabevalidierung und synchronen Fokusverlust geprüft. Unveränderte Updates erzeugen nach dem Aufwärmen 0 verwaltete Bytes. Balken, Textmodus und gezogener Wert werden auf echtem Grafikgerät pixelgleich über FrameBatch und SpriteBatch gezeichnet.

Menütests öffnen zunächst ein Untermenü und bewegen den Pointer anschließend über benachbarte Commands. Hover und Rechtsklick führen weder Command noch Action aus und ändern keine Check-Zustände. Das Wechseln zu einem Untermenü sowie explizite Klick-/Tastaturaktivierung funktionieren weiterhin.

Eigene `ISyntaxHighlighter` werden auf Textlücken, UTF-16-Indizes, mehrzeilige Bereiche, eigene Farben, ungültige/überlappende Bereiche, Wiederholung nach Fehlern und den Wechsel zu eingebauten Sprachen geprüft. Font- oder Paletteänderungen lösen keine neue Klassifizierung aus. 1.000 unveränderte Script-Updates erzeugen nach dem Aufwärmen 0 verwaltete Bytes und rufen den Highlighter nicht erneut auf. Eigene Script-Farben werden bei normaler und 1,25-facher Skalierung pixelgleich über FrameBatch und SpriteBatch gezeichnet.

1.000 Append-/Update-Schritte eines aufgewärmten Charts erzeugen sowohl mit fester als auch mit automatischer Skalierung 0 verwaltete Bytes. Der neue Script-Editor behält seinen Token-Snapshot bei unverändertem Text, Font und Sprache. Der tatsächliche NativeAOT-Build der Galerie startet und zeichnet die neue Tools-Seite erfolgreich.

Die bestehende Suite deckt außerdem Styles, Benachrichtigungen, automatische Factories, Commands, Grid, Layout-/Text-Caches, Bindings, Content-Besitz, Tabwechsel, Joysticks, Kalender und Input-Snapshots ab. 100 Buttons über 10.000 unveränderte Input-/Update-Frames und 20.000 Padding-/Margin-Zuweisungen erzeugen jeweils 0 verwaltete Bytes.

## Grafikprüfung

`Renderer/RenderValidation.csproj` kompiliert die aktuellen Controls-Quellen und vergleicht FrameBatch mit dem ausdrücklich gewählten SpriteBatch-Pfad:

- Fünf Galerie-Seiten mit Atlas-Skin, einschließlich Linien-/Balkendiagrammen und Syntaxfarben; bei Standard-Pixeladressierung und Skalierung 1 pixelgleich.
- Datei-Dialog mit Favoriten und Laufwerken in der linken Seitenleiste; ebenfalls pixelgleich.
- DPI 1,25/1,3, versetzter Viewport, Clipgrenzen, Alpha-Überlagerungen, gedrehte Texturen und Linien.
- Modal-Dialog inklusive Abdunklung, Schatten und Überdeckungsreihenfolge.
- FontStashSharp, SpriteFont und ein ausschließlich SpriteBatch unterstützender Custom-Font.
- Gespeicherte SpriteBatch-Referenz zusammen mit Context-Zeichenmethoden und verschachtelten Clips; Wechsel zurück zu FrameBatch als ausdrückliche Barriere.
- 12.000 Quads für wiederholte Buffer-Flushes; pixelgleich zum Referenzpfad.
- Zusätzlich Half-Pixel-Adressierung mit `--half-pixel` und explizitem Sprite-Layerdepth.

Kleine Farb-Rundungen bis 1/255 treten in DPI-Fällen auf. Bei einem gedrehten Geometrie-Sonderfall unter Standard-Pixeladressierung weicht die Deckung eines Randpixels ab. Die Prüfung akzeptiert nur eng begrenzte Nachbarpixel-Abweichungen an Kanten und keine Veränderungen im Bildinneren.

Der Grafiktest besteht auch als tatsächlich veröffentlichte und gestartete Windows-x64-NativeAOT-Anwendung. 1.000 wiederholte native Text-/Geometrie-Draws erzeugen im JIT-Lauf 0 verwaltete Bytes. NativeAOT/DesktopGL verbleibt bei 64 Byte je nichtleerem Flush: Die Diagnose lokalisiert je 32 Byte in MonoGames Vertex-/Index-SetData, 0 Byte in Begin, Glyphenausgabe, Bindings, Effect und Draw. Der Test hält dafür eine explizite Backend-Allokationsgrenze fest.

`Graphics/GraphicsProbe.csproj` bleibt die zusätzliche Cache-Prüfung mit 105 echten Draws. Die Fonteinbindung unterstützt dort ebenfalls den nativen Pfad.

Details: [FrameBatch](../FrameBatch.md), [Layout-Caching](../Layout-Caching.md). `Check-NoReflection.ps1` prüft die Quellen auf Reflection-/Member-Discovery-APIs. Neue native Typen und Benchmark-Quellen wurden zusätzlich auf passende Typ-/Dateinamen, einen Typ je Datei und fehlende verschachtelte/Partial-Typen geprüft.
