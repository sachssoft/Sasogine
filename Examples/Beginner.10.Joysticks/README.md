# Beginner.10.Joysticks

Eigenständiges Sasogine-Beispiel mit DesktopGL. Startet direkt mit sechs virtuellen Steuerknüppeln: frei/fest, frei/schwebend, vertikal, horizontal mit beibehaltenem Wert, eigene diagonale Achse sowie Totzone mit QuadraticInCurve. Die Werte und Bedienzustände werden live angezeigt.


Alle Controls verwenden direkte Properties. Der Host erstellt `_root` in `Initialize`, lädt Skin und Font vor dem ersten Update und ruft danach `_root.Update(...)` und `_root.Draw(...)` auf. Eingaben werden einmal mit `SampleReusable` erfasst; `Root.Dispose()` löst Bindungen und Control-Verbindungen.


## Starten

Im Sasogine-Hauptordner:

```powershell
dotnet run --project Examples/Beginner.10.Joysticks/10.JoysticksGL.csproj
```

Mit bereits gebauten Debug-Assemblies von Basic und Controls zusätzlich `-p:UseBuiltEngine=true` verwenden. Beide Beispiele sind unabhängig startbar; eine Referenz auf Beginner.09 ist nicht erforderlich.

Maus ziehen oder mehrere Touch-Kontakte gleichzeitig verwenden. Jeder Knüppel hält seinen eigenen Kontakt fest. Bei Anwendungsverlust werden alle Werte zurückgesetzt. Positive Y-Werte zeigen nach unten; InvertY ändert die Ausgabe für Spielkoordinaten. Der horizontale Knüppel behält seinen Ausschlag beim Loslassen.

## Dateien

- `ControlsJoystickPage.cs`: sechs Varianten mit direkten Properties, Kurven und Ereignissen.
- `JoystickExampleApp.cs`: Font/Skin, einmalige Geräteabfrage, Root.Update, Zeichnen und Dispose.
- `JoystickExampleScene.cs`: normale Sasogine-Szene unter der Oberfläche.
- `ExampleOptions.cs` und `ExampleFont.cs`: Startoptionen und Schriftwahl.

Eine installierte Schrift wird gesucht; alternativ `--font "Pfad/Schrift.ttf"` angeben oder eine eigene Schrift unter Content/Fonts/Default.ttf ablegen. Keine Schrift wird mitgeliefert. Der Skin wird aus Projects/UI referenziert und automatisch kopiert.

## Render-Prüfung

```powershell
dotnet run --project Examples/Beginner.10.Joysticks/10.JoysticksGL.csproj -p:UseBuiltEngine=true -- --snapshot "C:/Temp/Joysticks.png"
```

Die Anwendung speichert ein echtes Renderbild und schließt sich anschließend. `--help` zeigt die Optionen. Dieses Projekt hat keine --page-Auswahl: Es ist ausschließlich für Joysticks gedacht.

TouchPanel wird einmal im Update abgefragt. Auf mobilen Hosts denselben Finger nicht zusätzlich als emulierte Maus einspeisen; bei Bedarf einen leeren MouseState an den Sampler übergeben. Mehr zur API: Projects/UI/VirtualJoystick.md. Physische Android-/iOS-Geräte sind nicht Teil der DesktopGL-Prüfung.

## Windows-EXE erstellen

`EXE erstellen.cmd` veröffentlicht eine eigenständig startbare x64-Anwendung nach `publish/win-x64`. Optional mit bereits gebauten Debug-Engine-DLLs:

```cmd
"EXE erstellen.cmd" -p:UseBuiltEngine=true -p:SasogineBuildConfiguration=Debug
```
