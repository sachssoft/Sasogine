# Einstieg

Die API ist in [Architektur.md](Architektur.md) beschrieben. Controls werden über Properties konfiguriert. Der `Workspace` wird beim Initialisieren erstellt und anschließend über Update und Draw betrieben.

`ViewSceneBase` verbindet den Workspace mit dem Grafikgerät. Eigene Hosts, die Eingabe-Snapshots an den Workspace übergeben, rufen vor dem ersten Update `workspace.InitializeGraphics(GraphicsDevice)` auf. Der Workspace lädt dann automatisch den mitgelieferten Skin Sachssoft Dark.

```csharp
var workspace = new Workspace(new Button { Text = "Start" });
workspace.SetSkin(SkinPreset.SachssoftLight);
workspace.InitializeGraphics(GraphicsDevice);
```

Die Auswahl mit `SetSkin` ist optional. Registrierte Skins lassen sich auch per Name auswählen. `RegisterSkin("Custom", device => ControlSkin.Load(device, directory))` lädt den eigenen Skin beim ersten Einsatz. Der Workspace gibt Skin-Instanzen aus Factories frei. Direkt übergebene Instanzen bleiben Eigentum des Aufrufers.

Ausführbare Beispiele: `Examples/Beginner.09.Controls` (Galerie), `Examples/Beginner.10.Joysticks` (Touch-Steuerung), `Examples/Advanced.01.Editor` (2D-Editor mit Rotation und JSON).

Build der Bibliothek: `dotnet build Projects/UI/Sachssoft.Sasogine.Controls.csproj`. In den Beispielen erstellt `EXE erstellen.cmd` eine eigenständig startbare Windows-x64-Version. `UseBuiltEngine=true` kann bereits gebaute Engine-DLLs verwenden.

Prüfung: `dotnet run --project Projects/UI/Validation/Validation.csproj -f net10.0 -c Release -- --font <Font.ttf>`. Der Allokationsbenchmark gehört zum Prüfprogramm.
