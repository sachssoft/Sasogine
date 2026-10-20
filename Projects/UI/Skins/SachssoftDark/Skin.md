# Sachssoft Dark

Ein dunkler Skin mit cyanfarbenem Akzent. Originale, prozedural gezeichnete Grafiken und Symbole, ohne externe Icon-Pakete. Texte und Schriftarten werden zur Laufzeit gezeichnet und sind nicht im Atlas eingebrannt.

| Datei | Inhalt |
| --- | --- |
| `sachssoft_dark.png` | Texture-Atlas, 1024 × 512, RGBA, 139 Regionen |
| `sachssoft_dark.atlas.xml` | Frame-Namen und Pixelrechtecke im vorhandenen TextureAtlas-/sprite-Format |
| `sachssoft_dark.skin.xml` | 31 zentrale Styles, Vererbung, Zustände, Parts, Palette und Nine-Slice-Ränder |
| `Vorschau.png` | Aus denselben Atlas-Regionen zusammengesetzte Übersicht |
| `Pruefung.json` | Technischer Prüfstand |

## Abgedeckte Elemente

- Button, ToggleButton, SelectorButton, CheckBox mit Zwischenzustand und RadioButton.
- TextBox mit Fokus, Auswahl, Caret, ReadOnly-, Invalid- und Disabled-Darstellung.
- Slider horizontal/vertikal, getrennte Tracks, Fills, Thumbs und Fokusmarkierung.
- Eigenständige ScrollBar horizontal/vertikal mit separaten Pfeiltasten, Track und Thumb; ScrollViewer mit Eckfläche.
- ComboBox, ListBox, WrapPanel, TiledPanel, MenuBar und ContextMenu.
- Window, Dialog, OpenFileDialog, FolderSelectorDialog, ColorPickerDialog und MessageBox.
- Wiederverwendbare Panels, Fokusrahmen, Schatten, Separatoren, Tooltip und ProgressBar.

## Zustände und skalierbare Flächen

`Normal`, `Hover`, `Pressed` und `Disabled` sind die Grundzustände. Auswahlzustände heißen beispielsweise `Checked.Hover`, `Unchecked.Normal`, `Mixed.Pressed` oder `Selected.Inactive`. Fokus wird zusätzlich als Overlay gezeichnet. Ein fehlender zusammengesetzter Zustand fällt zuerst auf den passenden Auswahl-Grundzustand und dann auf `Normal` zurück. Deaktivierung hat Vorrang vor Hover/Pressed. Ungültige Textfelder behalten ihren roten Rahmen auch bei Fokus.

`Style.basedOn` vererbt Eigenschaften und Parts. Ein abgeleiteter Part ergänzt beziehungsweise überschreibt die gleichnamigen Zustände seines Basis-Parts. Lokale Control-Overrides sollen weiterhin Vorrang vor den Skin-Werten haben.

`Regions/Region` verbindet den Namen mit dem Atlas-Frame. `mode="NineSlice"` verwendet `slice="links,oben,rechts,unten"` in Pixeln der Originalregion. Ecken nicht verzerren; nur Mitte und Kanten entsprechend ihrer Achse strecken. Zielgröße mindestens so groß wie die gegenüberliegenden Slice-Ränder zusammen. `Fixed`-Symbole proportional zeichnen, `Stretch`-Flächen frei skalieren, `Tile`-Flächen innerhalb des Atlas-Rechtecks wiederholen. Nicht den gesamten Atlas mit einem Wrap-Sampler kacheln.

Grundmaße bei 1×: Control-Höhe 32, Icon-Größe 20, Check-/Radio-Glyphen 24, Scrollbar-Dicke und Pfeiltasten 20, Slider-Thumb 24, Menü-/Listenzeilen 30 und Titelleiste 36. Die neungeteilten Flächen benötigen keine eigene Textur pro Control-Größe. Atlas-Frames sind weder getrimmt noch gedreht; zwei Pixel Kantenextrusion und zwei Pixel Abstand verhindern Bleeding bei bilinearer Filterung.

## Alpha und Laden

Die PNG enthält bereits **vormultiplizierte RGB-Kanäle**. Direktes `Texture2D.FromStream` plus `BlendState.AlphaBlend`, `LinearClamp` und deaktivierte Mipmaps verwenden. Beim Laden oder im Content-Pipeline-Prozessor nicht erneut vormultiplizieren. Für Bildprogramme, die PNG grundsätzlich als Straight-Alpha interpretieren, dient die separat normal gespeicherte Vorschau als visuelle Referenz.

Das Atlas-XML wurde mit dem vorhandenen `FrameSetImporter` geprüft: 139 Frames mit unveränderten Koordinaten, korrekten Originalmaßen, ohne Trim oder Rotation. Das zusätzliche `alphaMode`-Attribut am Atlas beschreibt die Pixelkonvention; der vorhandene Frame-Importer liest nur die Sprite-Daten.

## Color Picker

Die Sättigungs-/Helligkeitsfläche funktioniert für beliebige Hue-Werte: zuerst `white` mit der voll gesättigten aktuellen Hue-Farbe einfärben, danach `color_saturation_overlay` und `color_value_overlay` darüber zeichnen. Für Alpha: Checkerboard, dann `color_alpha_ramp` mit der gewählten RGB-Farbe einfärben. Hue-Bänder, Cursor, Vorschau-Rahmen und Eyedropper-Symbol sind eigenständige Regionen.

## Laufzeit-Anbindung

`ControlSkin.Load(graphicsDevice, folder)` liest Atlas, Skin-XML und PNG. `skin.Apply(registry)` installiert die Styles. Der Renderer unterstützt Nine-Slice, feste Symbole, Stretch, gekachelte Flächen und Zustands-Parts. Die angeforderten Controls und Dialoge sind in der UI-Bibliothek implementiert; siehe `Controls.md` und `Examples/ControlsShowcase.cs`.

Tooltip und ProgressBar sind zusätzliche Atlasressourcen; eigene Controls dafür sind nicht Bestandteil dieses Stands. Geprüft: 139 Regionen, tatsächlicher Sasogine-Atlasimport, XML-Validierung und DesktopGL-Rendering der Galerie sowie der vier spezialisierten Dialoge. `Vorschau.png` bleibt die statische Ressourcenübersicht; die Grafikprüfung erzeugt zusätzlich Screenshots der laufenden Controls.

VirtualJoystick verwendet skalierbare Alias-Regionen des vorhandenen Atlas und horizontale/vertikale Tracks. Die Skin-XML enthält damit 147 Regionen für weiterhin 139 Bitmap-Frames. Der große Kreisrahmen wird scharf über DrawLine gezeichnet; ein eigener Skin kann den optionalen Part Base liefern. Die aktuelle Joystick-Galerie steht im Beginner.09-Beispiel.
