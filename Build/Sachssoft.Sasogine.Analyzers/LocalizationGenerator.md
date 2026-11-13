# Sasogine Localization Source Generator

Deutsch · [English](LocalizationGenerator.en.md)

Der Generator erzeugt typisierte C#-Schlüssel aus `.xdlt`-Vorlagen und prüft `.xdl`-Übersetzungen beim Build. Der generierte Code verwendet die Lokalisierungstypen aus Sasogine.

## Projekt einrichten

Füge die folgenden Gruppen in das Projekt ein, das die Schlüssel verwenden soll. Das Beispiel geht von zwei benachbarten Checkouts aus. Passe `SasogineRoot` an und ergänze den Runtime-Verweis nur, falls das Projekt Sasogine noch nicht referenziert.

```xml
<PropertyGroup>
  <SasogineRoot>../Sasogine</SasogineRoot>
  <RootNamespace>Sample.Game</RootNamespace>
</PropertyGroup>

<ItemGroup>
  <ProjectReference Include="$(SasogineRoot)/Projects/Basic/Sachssoft.Sasogine.csproj" />
  <ProjectReference Include="$(SasogineRoot)/Build/Sachssoft.Sasogine.Analyzers/Sachssoft.Sasogine.Analyzers.csproj"
                    OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  <CompilerVisibleProperty Include="RootNamespace" />
  <AdditionalFiles Include="Localization/**/*.xdlt;Localization/**/*.xdl" />
</ItemGroup>
```

`AdditionalFiles` stellt die Dateien dem Generator zur Verfügung. Falls die Anwendung Übersetzungen zur Laufzeit als eingebettete Ressourcen lädt, kann sie dieselben `.xdl`-Dateien zusätzlich als `EmbeddedResource` einbinden.

## Vorlage und Übersetzung anlegen

`Localization/editor.xdlt`

```xml
<dictionary name="Editor" language="en" namespace="Sample.Game.Localization">
  <section name="Menus">
    <item name="menu.title">Main Menu</item>
    <asset name="menu.logo" source="file">Images/logo_en.png</asset>
  </section>
</dictionary>
```

`Localization/editor.de.xdl`

```xml
<dictionary name="Editor" language="de">
  <item name="menu.title">Hauptmenü</item>
</dictionary>
```

- `name` und `language` sind in beiden Dateien Pflicht. Übersetzungen verwenden denselben Wörterbuchnamen und dieselben Schlüssel wie die Vorlage. Sie dürfen Einträge auslassen und flach oder in Sections angeordnet sein.
- `namespace` ist in `.xdlt` optional. Ohne Attribut gilt `RootNamespace` des Projekts und danach der Assembly-Name.
- `section` kann verschachtelt werden und bestimmt die C#-Klassenstruktur. Der Schlüssel `menu.title` bleibt in der Laufzeit-API erhalten.
- Die erzeugten Klassen und Felder sind derzeit `public`. Eine Einstellung für die Sichtbarkeit ist noch nicht verfügbar.

## Schlüssel verwenden

Nach `dotnet build` ist `EditorKeys` im generierten Namespace verfügbar. Registrierte Übersetzungen werden zur Laufzeit über den vorhandenen `LocalizationManager` aufgelöst.

```csharp
using Sample.Game.Localization;

string title = localization.GetValue(EditorKeys.Menus.MenuTitle);
var logoSource = EditorKeys.Menus.MenuLogo.Fallback;
```

Bei einem fehlenden Text prüft der Manager zuerst die konfigurierte Fallback-Sprache und verwendet anschließend den Text aus der `.xdlt`-Vorlage. Ein Asset-Eintrag enthält die konkrete Ressourcenquelle für seinen Fallback. Die Anwendung lädt die Laufzeitübersetzungen weiterhin selbst.

## Ressourcenquellen auswählen

`source="file"` verwendet Sasogines `LocalFileSource`. Eine eigene Quelle erbt von `ResourceSourceBase`, bietet einen öffentlichen Konstruktor mit einem `string`-Parameter und kann ihren Alias mit `[ResourceSource("cloud")]` deklarieren.

```xml
<asset name="menu.banner" source="cloud" provider="Acme.Sasogine.Cloud">banner.png</asset>
```

Bei Drittanbieterquellen ist `provider` der Assembly-Name des Anbieters ohne `.dll`. Das Projekt muss diese Assembly referenzieren. Ohne `provider` sucht der Generator den Alias in Sasogine.

## Generierten Code ansehen

Der Generator nennt seine Ausgabe nach der Eingabedatei. Aus `editor.xdlt` wird `editor.xdlt.cs`. Mit `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` legt der .NET-SDK-Build sie unter `obj` im Ausgabeordner des Generators ab.

Für eine separate Datei direkt neben `editor.xdlt` kann ein eigener Build-Schritt die Ausgabe dorthin kopieren. Wenn derselbe Consumer den Generator verwendet, muss seine `Compile`-Liste diese Kopie ausschließen, damit die Klasse genau einmal kompiliert wird.

## Häufige Diagnosen

| Code | Bedeutung |
|---|---|
| `SASLOC001` | XML ist ungültig oder die Datei konnte nicht gelesen werden. |
| `SASLOC002` | Wörterbuchstruktur oder Metadaten sind ungültig. |
| `SASLOC003` | Ein Schlüssel kommt im selben Wörterbuch mehrfach vor. |
| `SASLOC004` | Zwei Elemente ergeben denselben C#-Namen im selben Gültigkeitsbereich. |
| `SASLOC005` | Mehrere `.xdlt`-Dateien deklarieren denselben Wörterbuchnamen. |
| `SASLOC006` | Ressourcenquelle oder Anbieter fehlen, sind unbekannt oder mehrdeutig. |
| `SASLOC007` | Die Vorlage fehlt, die Sprache ist doppelt, der Schlüssel ist unbekannt oder die Eintragsart weicht ab. |
| `SASLOC008` | Der generierte Typname kollidiert mit einem anderen Typ. |

