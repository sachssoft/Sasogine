# Beginner.09.Controls

Startbares Sasogine-Beispiel mit DesktopGL und `Sachssoft.Engine.Controls`. Zeigt Buttons mit Bild-/Text-Content, TextBox, Checkboxen, Radio-/Selector-/Toggle-Buttons, Slider, separate ScrollBars, ListBox/ComboBox, ScrollViewer/TiledPanel, Menüs, Fenster und Datei-/Ordner-/Farb-/Meldungsdialoge.


Alle Controls verwenden direkte Properties. Der Host erstellt `_root` in `Initialize`, lädt Skin und Font vor dem ersten Update und ruft danach `_root.Update(...)` und `_root.Draw(...)` auf. Eingaben werden einmal mit `SampleReusable` erfasst; `Root.Dispose()` löst Bindungen und Control-Verbindungen.


## Starten

Im Sasogine-Hauptverzeichnis:

```powershell
dotnet run --project Examples/Beginner.09.Controls/09.ControlsGL.csproj
```

Eine installierte Standardschrift wird gesucht. Alternativ:

```powershell
dotnet run --project Examples/Beginner.09.Controls/09.ControlsGL.csproj -- --font "C:/Windows/Fonts/segoeui.ttf"
```

Eigene Schrift alternativ unter `Content/Fonts/Default.ttf` ablegen. Fonts werden nicht mitgeliefert. PNG und XML des SachssoftDark-Skins werden automatisch aus dem Controls-Projekt in den Ausgabeordner kopiert.

Mit bereits gebauten Debug-Assemblies von Basic und Controls kann das Beispiel ohne Neubau der Engine gestartet werden:

```powershell
dotnet run --project Examples/Beginner.09.Controls/09.ControlsGL.csproj -p:UseBuiltEngine=true
```

## Wo steht was?

- `ControlsExampleView.cs`: Control-Baum, Content-Button, Styles und Dialog-Callbacks.
- `ControlsExampleApp.cs`: Font-/Skin-Laden, Gerätesnapshots, TextInput, `Root.Update`, Draw und Dispose.
- `ControlsExampleScene.cs`: normale Sasogine-Szene unter der Controls-Oberfläche.
- `ExampleClipboardService.cs`: anwendungsinterner Clipboard-Adapter; für systemweites Copy/Paste einen Plattformdienst einsetzen.

Tab/Umschalt+Tab navigieren. Enter/Leertaste aktivieren passende Controls; Escape schließt Popup/Dialog. Strg+O öffnet den Dateidialog. Das Fenster lässt sich skalieren; die Galerie wird als 1160 × 760 große logische Oberfläche angepasst. Styles verwenden `StyleId`, String-Werte und typisierte Converter. Controls besitzen direkte Properties; `Root` registriert die eingebauten Controls automatisch. Jeder Typ besitzt eine eigene Datei.

Für einen automatischen Render-Durchlauf `--snapshot` angeben. Nach dem Speichern schließt sich die Anwendung:

```powershell
dotnet run --project Examples/Beginner.09.Controls/09.ControlsGL.csproj -p:UseBuiltEngine=true -- --snapshot "C:/Temp/Controls.png"
```

## Neue Layout-/Tabellenseite

Der zweite Tab zeigt Grid mit Auto/Star-Spalten, eine sortierbare und bearbeitbare DataTable sowie Sasogine-Commands mit Zeilenparametern. F2 bearbeitet die gewählte Zelle, Enter übernimmt und Escape verwirft. Die Spaltenränder lassen sich ziehen. „Color models…“ öffnet den Farbpicker mit OKLab; RGB, HSL, HSV, lineares RGB und RGBA sind ebenfalls auswählbar.

`ControlsDataPage.cs` und `ControlsDemoRow.cs` enthalten dieses Beispiel. Die Tabellenwerte sind Beispieldaten. Strg+Tab/Strg+Umschalt+Tab wechseln die Seiten. Direkt starten beziehungsweise rendern:

```powershell
dotnet run --project Examples/Beginner.09.Controls/09.ControlsGL.csproj -p:UseBuiltEngine=true -- --page layout --snapshot "C:/Temp/Controls-Table.png"
```

## Separates Joystick-Beispiel

Die virtuellen Steuerknüppel haben ein eigenes startbares Projekt: [Beginner.10.Joysticks](../Beginner.10.Joysticks/README.md). Dieses Beispiel enthält die Controls-Galerie, Grid-/Tabellenseite und die Seite Tree, input & dates.

## Tree, input & dates

Dritter Tab: TreeView, NumericSpinner mit wiederholenden Pfeilen, RepeatButton, PasswordBox, Spinner, DatePicker und Calendar. Tooltips funktionieren auch an ScrollViewer und zusammengesetzten Controls. Auf der Tabellenseite sind Strg-/Umschalt-Mehrfachauswahl und Strg+A eingeschaltet.

```powershell
dotnet run --project Examples/Beginner.09.Controls/09.ControlsGL.csproj -p:UseBuiltEngine=true -- --page advanced
```

Aufbau: ControlsAdvancedPage.cs. Die Kalenderdaten sind absichtlich feste Beispieldaten. API-Details stehen in Projects/UI/Weitere-Controls.md.

## Curve editor

Vierter Tab: editierbare Kurve mit Step/Linear/Monotone, Koordinatenfeldern, Undo/Redo und Zoom. Rechts stehen vorhandene Sasogine-Kurven als Vorschau sowie ein Joystick, der dieselbe bearbeitete CurveBase auswertet. Aufbau: ControlsCurvePage.cs.

```powershell
dotnet run --project Examples/Beginner.09.Controls/09.ControlsGL.csproj -p:UseBuiltEngine=true -- --page curve
```

Punkte ziehen, Strg+Klick/Doppelklick einfügen, Entf löschen, Umschalt einrasten, Rad zoomen, Mitte/Alt+Ziehen verschieben. Home setzt die Ansicht zurück, F passt sie an. API: Projects/UI/CurveControl.md.

## Polling und MVVM

`ControlsExampleApp.Update` ruft nach `_root.Update(...)` die Methode `_view.Update()` auf. Dort wird der Bild-Button direkt über `IsPressed` abgefragt. `IsPressed` gilt für einen Aktivierungsframe; `IsHeld` zeigt den gehaltenen Zustand. Menüs und Dialogaktionen zeigen zusätzlich die ereignisbasierte Verwendung; die Tabellenseite verwendet Commands mit Parametern.

Das mehrzeilige Textfeld ist mit `GalleryViewModel.Notes` bidirektional verbunden. Das Modell erbt von `Element` und verwendet `SetNotify` mit einem wiederverwendeten `PropertyChangedEventArgs`. `PropertyBinding` erhält explizite Getter/Setter; `Root.Bindings` verarbeitet und entsorgt die Bindung. Es wird keine Reflection benötigt.

## Windows-EXE erstellen

`EXE erstellen.cmd` veröffentlicht eine eigenständig startbare x64-Anwendung nach `publish/win-x64`. Optional mit bereits gebauten Debug-Engine-DLLs:

```cmd
"EXE erstellen.cmd" -p:UseBuiltEngine=true -p:SasogineBuildConfiguration=Debug
```
