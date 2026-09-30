# Sasogine – NuGet Release

Kurzanleitung zum Erstellen und Veröffentlichen der Sasogine-Pakete.

> Alle PowerShell-Befehle im Sasogine-Hauptordner ausführen.
> Android und iOS sind aktuell vom Release ausgesetzt.

## 1. Version prüfen

Die Version steht in `Sachssoft.Sasogine.Package.props`:

```xml
<VersionPrefix>0.11.0</VersionPrefix>
<VersionSuffix>alpha</VersionSuffix>
```

Ergebnis: `0.11.0-alpha`

Vor einem neuen Release die Version anpassen und `CHANGELOG.md` aktualisieren.

## 2. Alte Pakete löschen

```powershell
Remove-Item .\Packages\*.nupkg, .\Packages\*.snupkg -ErrorAction SilentlyContinue
```

Damit enthält `Packages` keine Dateien eines alten Releases.

## 3. Restore und Build

```powershell
dotnet restore .\Sachssoft.Sasogine.slnx
dotnet build .\Sachssoft.Sasogine.slnx -c Release
```

Der Release-Build erzeugt die NuGet-Pakete automatisch im Ordner `Packages`.

Der Build muss erfolgreich sein.

## 4. Pakete prüfen

```powershell
Get-ChildItem .\Packages\*.nupkg
```

Aktuell werden veröffentlicht:

- `Sachssoft.Sasogine`
- `Sachssoft.Sasogine.Documents`
- `Sachssoft.Sasogine.Toolkit`
- `Sachssoft.Sasogine.Platform.Windows`
- `Sachssoft.Sasogine.Platform.Linux`
- `Sachssoft.Sasogine.Platform.MacOs`
- `Sachssoft.Sasogine.DesktopGL`
- `Sachssoft.Sasogine.DesktopVK`
- `Sachssoft.Sasogine.WindowsDX`
- `Sachssoft.Sasogine.WindowsDX12`

Prüfen, ob Version und Paketnamen stimmen.

## 5. NuGet API-Key setzen

```powershell
$env:NUGET_API_KEY = "YOUR_NUGET_API_KEY"
```

Den API-Key niemals im Repository speichern.

## 6. Alle Pakete veröffentlichen

```powershell
Get-ChildItem .\Packages\*.nupkg | % { dotnet nuget push $_ --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate }
```

Diesen Befehl nur ausführen, wenn im Ordner `Packages` wirklich nur die Pakete des aktuellen Releases liegen.

## 7. Release prüfen

Nach dem Upload auf NuGet.org kurz prüfen:

- richtige Version
- richtige Pakete
- richtige Abhängigkeiten

## 8. Git-Tag erstellen

Beispiel für Version `0.11.0-alpha`:

```powershell
git tag v0.11.0-alpha
git push origin v0.11.0-alpha
```

## Bei Restore-Problemen

Nur bei Problemen mit NuGet ausführen:

```powershell
dotnet nuget locals all --clear
dotnet restore .\Sachssoft.Sasogine.slnx --no-cache
```

## Kurzfassung

```powershell
Remove-Item .\Packages\*.nupkg, .\Packages\*.snupkg -ErrorAction SilentlyContinue
dotnet restore .\Sachssoft.Sasogine.slnx
dotnet build .\Sachssoft.Sasogine.slnx -c Release
Get-ChildItem .\Packages\*.nupkg
$env:NUGET_API_KEY = "YOUR_NUGET_API_KEY"
Get-ChildItem .\Packages\*.nupkg | % { dotnet nuget push $_ --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json --skip-duplicate }
```
