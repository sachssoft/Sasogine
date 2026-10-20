# Einstieg

Die neue API ist in [Architektur.md](Architektur.md) beschrieben. Controls werden direkt über Properties aufgebaut; `Workspace` wird in Initialize erstellt und anschließend über Update/Draw betrieben. Skin und Font werden vor dem ersten Update geladen.

Ausführbare Beispiele: `Examples/Beginner.09.Controls` (Galerie), `Examples/Beginner.10.Joysticks` (Touch-Steuerung), `Examples/Advanced.01.Editor` (2D-Editor mit Rotation und JSON).

Build der Bibliothek: `dotnet build Projects/UI/Sachssoft.Sasogine.Controls.csproj`. In den Beispielen erstellt `EXE erstellen.cmd` eine eigenständig startbare Windows-x64-Version. `UseBuiltEngine=true` kann bereits gebaute Engine-DLLs verwenden.

Prüfung: `dotnet run --project Projects/UI/Validation/Validation.csproj -f net10.0 -c Release -- --font <Font.ttf>`. Der Allokationsbenchmark gehört zum Prüfprogramm.
