# Native UI-FrameBatch

`ControlDrawContext` verwendet standardmÃ¤ÃŸig den eigenen **ControlFrameBatch**. Bestehende Initialisierung bleibt gÃ¼ltig:

```csharp
_draw = new ControlDrawContext(this, font, viewport);
_workspace.Update(frame, layout);
_workspace.Draw(_draw);
```

## Aufbau

- Wiederverwendete Vertex-/Indexpuffer mit 16-Bit-Indizes und geordneten Flushes an KapazitÃ¤tsgrenzen.
- Standard- und Half-Pixel-Adressierung sowie Sprite-Layerdepth werden berücksichtigt. Lokale Koordinaten und DPI werden auf der CPU transformiert. Dreiecke werden am aktuellen Pixel-Clip geschnitten; UVs und Farben bleiben interpoliert.
- ZusammenhÃ¤ngende oder sicher verschiebbare Texturgruppen werden gemeinsam ausgegeben. Ein Element darf nur an fremden Gruppen vorbeiziehen, deren konservative Rechtecke es nicht Ã¼berlappt. Ãœberlappende Inhalte behalten die Zeichenreihenfolge.
- Bilder, Atlas-Skins, Nine-Slice, Linien und die beiden eingebauten Fontadapter nutzen den nativen Pfad. FontStashSharp liefert Glyph-Quads Ã¼ber seine vorhandene Renderer-Schnittstelle; keine neue PaketabhÃ¤ngigkeit.
- Layout, TextmaÃŸe und Textposition bleiben separat gecacht. Die Frame-Geometrie wird fÃ¼r das aktuelle Bild gesammelt; es gibt keine automatische RenderTexture-Kopie ganzer Controls.

## Eigene Controls und Fonts

Eigene Controls kÃ¶nnen die vorhandenen Methoden `FillRectangle`, `DrawTexture`, `DrawLine` und `DrawText` weiterverwenden. Direkte native Zeichenaufrufe gehen Ã¼ber `context.FrameBatch`; Begin/End und GerÃ¤teÃ¤nderungen gehÃ¶ren dem Context.

FÃ¼r neue Fonts zusÃ¤tzlich `IControlBatchFont` implementieren. Der bisherige `IControlFont.Draw(SpriteBatch, ...)`-Vertrag bleibt erhalten. `FontFaceControlFont` und `SpriteFontControlFont` unterstÃ¼tzen beide Pfade.

`context.SpriteBatch` ist weiterhin verfÃ¼gbar. Zugriff leert zuerst die native Warteschlange und aktiviert fÃ¼r den aktuellen Zeichenabschnitt den KompatibilitÃ¤tspfad. Das gilt auch beim Mischen mit Context-Aufrufen und verschachtelten Clips. Expliziter Zugriff auf `FrameBatch` wechselt wieder zum nativen Pfad; danach eine zuvor gespeicherte SpriteBatch-Referenz nicht weiterverwenden.

FÃ¼r neue Clip-Bereiche `using (context.PushClipScope(bounds))` verwenden. Der konkrete Struct-Scope vermeidet Boxing; nicht kopieren und in umgekehrter Reihenfolge schlieÃŸen. Das bisherige `PushClip` mit `IDisposable` bleibt binÃ¤r erreichbar und kann dabei weiterhin boxen.

Der Vergleichspfad lÃ¤sst sich ausdrÃ¼cklich auswÃ¤hlen:

```csharp
var reference = new ControlDrawContext(
    this, font, viewport, 1, ControlRenderer.SpriteBatch);
```

`FrameDrawCalls` zeigt die vom nativen Batcher ausgegebenen Draw-Calls des letzten Frames. KompatibilitÃ¤ts-SpriteBatch-Aufrufe zÃ¤hlen dort nicht mit; fÃ¼r Gesamtzahlen `GraphicsDevice.Metrics` um den UI-Aufruf erfassen.

## PrÃ¼fung und Grenzen

Die Headless-Suite besteht unter .NET 8/9/10 und NativeAOT: 18 Gruppen, 2.074 Assertions einschlieÃŸlich Clipping/UV-Interpolation. Die RendererprÃ¼fung vergleicht vier Galerie-Seiten mit Skin, Text, DPI, Alpha, Modal-Overlays, alten Custom-Fonts und gemischten SpriteBatch-/Context-Aufrufen. 12.000 Quads prÃ¼fen Pufferwechsel. Bei normaler Skalierung sind die Galerie-Seiten pixelgleich; ein Geometrie-Sonderfall bei DPI 1,3 zeigt einen Randpixel mit anderer Rasterisierungsdeckung, daneben minimale Farb-Rundungen. Die PrÃ¼fung erlaubt nur eng begrenzte Nachbarpixel-Abweichungen an Kanten, keine Ã„nderungen im Bildinneren.

1.000 wiederholte native Text-/Geometrie-Draws: **0 verwaltete Bytes im JIT-Lauf**. Im getesteten NativeAOT-/DesktopGL-Lauf entstehen **64 Byte je nichtleerem Flush** innerhalb der beiden MonoGame-SetData-Aufrufe, 32 Byte pro Upload. Die Diagnose misst 0 Byte fÃ¼r Begin, Glyphenausgabe, Bindings, Effect und Draw. Das ist eine verbleibende Backend-Kostenstelle, keine Aussage Ã¼ber andere Grafikbackends.

```cmd
dotnet run --project Validation/Validation.csproj -c Release -f net10.0
dotnet run --project Validation/Renderer/RenderValidation.csproj -c Release
dotnet publish Validation/Renderer/RenderValidation.csproj -c Release -r win-x64 -p:PublishAot=true
```

Die GrafikprÃ¼fung benÃ¶tigt die mitgelieferten Galerie-Quellen unter `Examples/Beginner.09.Controls`, eine bereits gebaute Basic-DLL sowie unter Windows Segoe UI. Captures landen neben der PrÃ¼fanwendung. `Validation/Check-NoReflection.ps1` prÃ¼ft den Controls-Code; Factories und Fontadapter verwenden explizite Typen und Delegates.

Der Performance-Vergleich unter `Benchmarks/Controls.FrameBatch` verwendet fÃ¼r beide Sasogine-Varianten denselben aktuellen Layout-/Text-Cache und dieselbe Fontinstanz. Nur der Renderer unterscheidet sich. Myra 1.6.8 ist unverÃ¤ndert. Reale Framezeit mit Update/Draw/Present, ohne Readback wÃ¤hrend der Messung; keine allgemeine Geschwindigkeitsgarantie fÃ¼r komplette Spiele.

API-Referenzen: [FontStashSharp Renderer-Interface](https://github.com/FontStashSharp/FontStashSharp/blob/master/src/FontStashSharp/Interfaces/IFontStashRenderer2.cs), [MonoGame VertexBuffer-Upload 3.8.6](https://github.com/MonoGame/MonoGame/blob/v3.8.6/MonoGame.Framework/Platform/Graphics/Vertices/VertexBuffer.OpenGL.cs). Keine Reflection im Controls-Code.
