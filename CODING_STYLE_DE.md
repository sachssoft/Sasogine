# Sasogine Coding Style

Dieses Dokument definiert die verbindlichen Coding-, Formatierungs- und
Benennungsregeln für Sasogine.

Grundsätzlich orientiert sich Sasogine an den etablierten .NET- und
C#-Konventionen. Eigene Regeln ergänzen diese dort, wo die Architektur,
Serialisierung oder öffentliche API von Sasogine eine eindeutigere
Benennung benötigt.

## 1. Naming-Regeln

### .NET-Konventionen

Benennungen müssen grundsätzlich den etablierten .NET-Namenskonventionen
folgen.

`PascalCase` wird verwendet für:

-   Klassen
-   Structs
-   Interfaces
-   Enums
-   Delegates
-   Records
-   Methoden
-   Properties
-   Events
-   öffentliche Member

``` csharp
public class AssetManager
{
    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
    }
}
```

### Interfaces

Interfaces verwenden verpflichtend das Präfix `I`.

``` csharp
public interface IRenderable
{
}

public interface IDefinition
{
}
```

### Parameter und lokale Variablen

Parameter und lokale Variablen verwenden `camelCase`.

``` csharp
public void SetPosition(float positionX, float positionY)
{
    float previousPosition = positionX;
}
```

### Private Felder

Private Instanzfelder verwenden `_camelCase`.

``` csharp
private bool _isInitialized;
private AssetManager _assetManager;
```

### Konstanten

Für nichtöffentliche Konstanten mit `private` oder `internal` ist
`UPPER_CASE` zulässig und empfohlen.

Mehrere Wörter werden durch Unterstriche getrennt.

``` csharp
private const int MAXIMUM_COUNT = 100;
internal const string DEFAULT_NAME = "Sasogine";
```

Diese Schreibweise ist insbesondere für interne Implementierungsdetails
vorgesehen.

Für Konstanten, die Bestandteil der öffentlichen API sind, sollen die
etablierten .NET-Namenskonventionen verwendet werden.

``` csharp
public const int MaximumCount = 100;
```

Damit bleibt die öffentliche API mit dem .NET-Ökosystem konsistent,
während interne Konstanten optisch eindeutig erkennbar bleiben.

### Boolesche Member

Boolesche Member sollen ihren Zustand oder ihre Fähigkeit eindeutig
ausdrücken.

Bevorzugt:

``` csharp
IsVisible
IsEnabled
IsInitialized
HasFocus
CanRender
SupportsInput
```

Unspezifische oder von üblichen .NET-Konventionen abweichende
Bezeichnungen sollen vermieden werden.

### Abkürzungen

Abkürzungen werden möglichst wie normale Wörter behandelt.

Bevorzugt:

``` csharp
XmlReader
HttpClient
UiElement
```

Abweichungen sind nur sinnvoll, wenn ein etablierter technischer
Eigenname oder eine bestehende .NET-API dies erfordert.

------------------------------------------------------------------------

## 2. Scope- und Kontrollfluss-Regeln

### Geschweifte Klammern und kurze `if`-Anweisungen

Bei einem einfachen `if` mit genau einer kurzen Anweisung dürfen geschweifte Klammern weggelassen werden. Diese kompakte Schreibweise ist innerhalb von Sasogine ausdrücklich erlaubt und gern gesehen.

```csharp
if (isVisible)
    Render();
```

Geschweifte Klammern bleiben ebenfalls zulässig:

```csharp
if (isVisible)
{
    Render();
}
```

Sobald `if` und `else` gemeinsam verwendet werden, sollen geschweifte Klammern bevorzugt werden.

Nicht empfohlen:

```csharp
if (isVisible)
    Render();
else
    Test();
```

Bevorzugt:

```csharp
if (isVisible)
{
    Render();
}
else
{
    Test();
}
```

Bei mehreren Anweisungen innerhalb eines Zweigs sind geschweifte Klammern erforderlich.

`else` ist vollständig zulässig und wird nicht gegenüber Early Returns benachteiligt oder vermieden. Die jeweils verständlichere Kontrollflussstruktur kann verwendet werden.

### Einfaches `if` bevorzugen

Wenn eine Bedingung ohne `else` verständlich ausgedrückt werden kann,
ist ein einfaches `if` zu bevorzugen.

``` csharp
if (!isInitialized)
{
    Initialize();
}
```

### Boolean-Bedingungen

Boolesche Werte werden direkt als Bedingung verwendet.

Bevorzugt:

```csharp
if (isLoaded)
    Load();

if (!isLoaded)
    Load();
```

Explizite Vergleiche mit `true` oder `false` sind nicht zulässig beziehungsweise nicht zu verwenden.

Nicht verwenden:

```csharp
if (isLoaded == true)
    Load();

if (isLoaded == false)
    Load();
```

Eine Zuweisung innerhalb einer Boolean-Bedingung ist ebenfalls nicht zulässig.

Nicht verwenden:

```csharp
if (isLoaded = true)
    Load();

if (isLoaded = false)
    Load();
```

Die Bedingung selbst soll bereits durch einen aussagekräftigen Boolean-Namen verständlich sein.

### Scope möglichst klein halten

Variablen sollen möglichst nahe an ihrer tatsächlichen Verwendung
deklariert werden.

Member sollen nur die Sichtbarkeit erhalten, die tatsächlich benötigt
wird.

Bevorzugt wird grundsätzlich die kleinste sinnvolle Sichtbarkeit:

`private` → `protected` / `internal` → `public`

`public` darf nicht allein aus Bequemlichkeit verwendet werden.

Die öffentliche API von Sasogine soll bewusst definiert werden.

------------------------------------------------------------------------

## 3. Formatierungsregeln

Sasogine verwendet einen kompakten, aber gut lesbaren Formatierungsstil.

Unnötige Leerzeilen und übermäßige vertikale Formatierung sollen
vermieden werden.

### Methodendeklarationen

Kurze Methodendeklarationen können kompakt bleiben.

``` csharp
public void Move(float x, float y)
{
}
```

Bei mehreren oder komplexeren Parametern wird empfohlen, jeden Parameter
in einer eigenen Zeile zu schreiben.

``` csharp
public void CreateAsset(
    AssetDefinition definition,
    AssetContext context,
    bool initializeImmediately)
{
}
```

Dies gilt insbesondere für:

-   Konstruktoren mit mehreren Abhängigkeiten
-   Factory-Methoden
-   lange Parametertypen
-   generische Methoden
-   Delegates
-   Parameter mit Attributen oder Modifizierern

### Methodenaufrufe

Dasselbe Prinzip gilt für Methodenaufrufe.

Kurze Aufrufe bleiben kompakt:

``` csharp
Move(x, y);
```

Längere Aufrufe werden aufgeteilt:

``` csharp
CreateAsset(
    definition,
    context,
    initializeImmediately);
```

### Generische Typen

Generische Typen dürfen auf mehrere Zeilen verteilt werden, wenn die
Deklaration ansonsten schwer lesbar wird.

``` csharp
DefinitionBindingCollection<
    CourseEntityDefinitionBase,
    ICourseEntity,
    CourseEntityContext>
```

Eine kurze und übersichtliche generische Deklaration soll dagegen nicht
unnötig aufgeteilt werden.

### Leerzeilen

Leerzeilen dienen der logischen Gliederung.

Zusammengehörende Anweisungen sollen optisch zusammenbleiben.

Unnötige Leerzeilen zwischen einzelnen Anweisungen sollen vermieden
werden.

------------------------------------------------------------------------


## 4. Member-Reihenfolge in Klassen und Structs

Für `class` und `struct` gilt innerhalb von Sasogine eine feste Member-Reihenfolge.

Diese Reihenfolge ist verbindlich und soll unabhängig von der Größe des Typs eingehalten werden.

1. Konstanten (`const`)
2. Readonly-Felder (`readonly`) – zuerst `static`, danach Instanzfelder
3. Felder
4. Events
5. Konstruktoren
6. Properties
7. Instanz-Member
8. Statische Member

Beispiel:

```csharp
public class Example
{
    private const int MAXIMUM_COUNT = 100;

    private static readonly object _staticLock = new();
    private readonly object _instanceLock = new();

    private int _count;

    public event EventHandler? Changed;

    public Example()
    {
    }

    public int Count => _count;

    public void Update()
    {
    }

    public static Example Create()
    {
        return new Example();
    }
}
```

### Konstanten

Konstanten stehen immer zuerst.

```csharp
private const int MAXIMUM_COUNT = 100;
private const string DEFAULT_NAME = "Sasogine";
```

### Readonly-Felder

Nach den Konstanten folgen `readonly`-Felder.

Statische `readonly`-Felder stehen dabei vor nichtstatischen `readonly`-Feldern.

```csharp
private static readonly object _staticLock = new();

private readonly object _instanceLock = new();
```

### Felder

Normale Felder folgen nach den `readonly`-Feldern.

```csharp
private int _count;
private bool _isInitialized;
```

### Events

Events stehen nach den Feldern und vor den Konstruktoren.

```csharp
public event EventHandler? Changed;
```

### Konstruktoren

Konstruktoren stehen nach den Events und vor den Properties.

```csharp
public Example()
{
}
```

### Properties

Properties stehen nach den Konstruktoren.

```csharp
public int Count => _count;

public bool IsInitialized => _isInitialized;
```

### Instanz-Member

Nichtstatische Methoden und andere Instanz-Member folgen nach den Properties.

```csharp
public void Initialize()
{
}

public void Update()
{
}
```

### Statische Member

Statische Methoden und sonstige statische Member stehen am Ende des Typs.

```csharp
public static Example Create()
{
    return new Example();
}
```

Die Reihenfolge soll nicht nach `public`, `protected`, `internal` oder `private` gruppiert werden. Die Art und architektonische Rolle des Members bestimmt seine Position.

---

## 5. Sasogine-spezifische Benennungsregeln

Neben den allgemeinen .NET-Konventionen gelten innerhalb von Sasogine
zusätzliche Regeln.

Diese Regeln dienen dazu, die architektonische Funktion eines Typs
bereits anhand seines Namens erkennen zu können.

### `Definition`-Suffix

Das Suffix `Definition` ist verpflichtend, wenn ein Typ eine
Sasogine-Definition darstellt.

Dies gilt insbesondere für Typen, die:

-   `IDefinition` direkt implementieren,
-   von einem `IDefinition`-Typ erben,
-   als Definition innerhalb der Sasogine-Architektur verwendet werden,
-   oder als serialisierbare Definition beziehungsweise serialisierbare
    Beschreibung eines Objekts vorgesehen sind.

``` csharp
public class TextureDefinition : IDefinition
{
}

public abstract class EntityDefinitionBase : IDefinition
{
}

public class CourseEntityDefinition : EntityDefinitionBase
{
}
```

Nicht:

``` csharp
public class Texture : IDefinition
{
}
```

Das `Definition`-Suffix beschreibt eine konkrete architektonische Rolle
und darf in diesen Fällen nicht zur Verkürzung des Namens weggelassen
werden.

### Serialisierung und `Serialization`-Suffix

Serialisierungstypen sind von Definitionstypen zu unterscheiden.

Ein Typ, dessen eigentliche Aufgabe die Serialisierung oder Deserialisierung einer Definition beziehungsweise eines anderen Sasogine-Typs ist, muss das Suffix `Serialization` verwenden.

Beispiel:

```csharp
public class TreeDefinition : IDefinition
{
}

public class TreeSerialization : SerializationBase<TreeDefinition>
{
}
```

`TreeDefinition` beschreibt die Definition des Objekts.

`TreeSerialization` beschreibt die dafür zuständige Serialisierungslogik.

Das Suffix `Definition` darf deshalb nicht als allgemeines Suffix für Serialisierungstypen verwendet werden.

Bevorzugt:

```csharp
TextureDefinition
TextureSerialization

TreeDefinition
TreeSerialization
```

Nicht:

```csharp
TreeDefinitionSerializationDefinition
TreeSerializer
```

wenn der Typ innerhalb der Sasogine-Architektur ausdrücklich die Rolle eines `Serialization`-Typs besitzt.

Für Definitionstypen bleibt `Definition` verpflichtend. Für entsprechende Serialisierungstypen ist `Serialization` verpflichtend.

### `Base`-Suffix

Bei abstrakten Klassen, die ausdrücklich als Vererbungsbasis vorgesehen
sind, wird das Suffix `Base` empfohlen.

``` csharp
public abstract class ShapeDefinitionBase
{
}

public abstract class ComponentBase
{
}
```

Das Suffix `Base` ist jedoch nicht verpflichtend.

Eine abstrakte Klasse darf ohne `Base` benannt werden, wenn ihr Name
bereits eindeutig ist oder `Base` keinen zusätzlichen semantischen
Nutzen bietet.

``` csharp
public abstract class Renderer
{
}
```

`Base` soll nicht mechanisch an jede abstrakte Klasse angehängt werden.

### Weitere architektonische Suffixe

Bereits etablierte Sasogine-Suffixe sollen entsprechend ihrer
tatsächlichen architektonischen Rolle konsistent verwendet werden.

Typische Suffixe sind:

-   `Definition`
-   `Context`
-   `Registry`
-   `Factory`
-   `Builder`
-   `Provider`
-   `Manager`
-   `Collection`
-   `Attribute`
-   `Exception`
-   `EventArgs`
-   `Base`

`Definition` kennzeichnet Definitionen und entsprechende serialisierbare
Beschreibungen und ist in den oben beschriebenen Fällen verpflichtend.

`Context` kennzeichnet einen klar abgegrenzten Laufzeit- oder
Verarbeitungskontext.

`Registry` kennzeichnet eine zentrale Registrierung oder Zuordnung
bestimmter Typen beziehungsweise Werte.

`Factory` kennzeichnet einen Typ, dessen wesentliche Aufgabe die
Erzeugung anderer Objekte ist.

`Builder` kennzeichnet den schrittweisen Aufbau komplexerer Objekte oder
Konfigurationen.

`Provider` kennzeichnet einen Typ, der einen bestimmten Dienst oder
bestimmte Daten bereitstellt.

`Collection` kennzeichnet einen spezialisierten Collection-Typ.

`Attribute`, `Exception` und `EventArgs` folgen den entsprechenden
.NET-Konventionen.

`Base` kann für ausdrücklich als Vererbungsbasis entwickelte abstrakte
Klassen verwendet werden, ist aber nicht verpflichtend.

### Keine künstlichen Bezeichnungen

Existiert bereits eine etablierte Bezeichnung für ein architektonisches
Konzept, soll keine alternative Bezeichnung für dieselbe Rolle erfunden
werden.

Bevorzugt:

``` csharp
DefinitionRegistry
AssetFactory
RenderContext
DefinitionBindingCollection
```

Unspezifische Namen sollen vermieden werden:

``` csharp
DefinitionStuff
AssetHelper
RenderData
CommonUtil
```

Insbesondere `Helper`, `Util`, `Manager` und `Data` sollen nicht als
universelle Auffangbegriffe verwendet werden.

------------------------------------------------------------------------

## 6. Namespace-Regeln

Typen sollen einem bereits vorhandenen Namespace zugeordnet werden, wenn dessen fachlicher Bereich sinnvoll zum Typ passt.

Die Namespace-Struktur soll semantisch verständlich bleiben, aber nicht unnötig aufgebläht werden.

Bevorzugt wird:

1. Zuerst prüfen, ob ein vorhandener Namespace fachlich passt.
2. Wenn mehrere Namespaces möglich sind, den semantisch passendsten und stabilsten Bereich wählen.
3. Einen neuen Namespace nur dann einführen, wenn kein vorhandener Namespace den Typ sinnvoll aufnehmen kann.
4. Neue Namespaces bewusst und ausnahmsweise anlegen, um eine unnötig große Anzahl sehr kleiner Namespaces zu vermeiden.

Ein neuer Namespace soll nicht allein deshalb entstehen, weil für wenige Typen theoretisch eine noch feinere Kategorie möglich wäre.

Die Namespace-Struktur soll die Architektur unterstützen und nicht jedes Implementierungsdetail abbilden.

---

## 7. API-Benennung

Die öffentliche API von Sasogine soll möglichst bereits über ihre
Benennung verständlich sein.

Ein Nutzer sollte aus einem Namen erkennen können:

-   was ein Typ darstellt,
-   welche architektonische Rolle er besitzt,
-   ob es sich um eine Definition oder ein Runtime-Objekt handelt,
-   ob ein Typ als Registry, Factory, Context oder Collection dient.

Bezeichnungen sollen die semantische Aufgabe beschreiben und nicht
lediglich ein internes Implementierungsdetail.

Bevorzugt:

``` csharp
AssetRegistry
DefinitionBindingCollection
RenderContext
ShapeDefinition
```

gegenüber:

``` csharp
AssetData
BindingHelper
RenderStuff
ShapeInfo
```

------------------------------------------------------------------------

## Grundprinzip

Die etablierten .NET-Konventionen bilden die Grundlage des Sasogine
Coding Styles.

Sasogine-spezifische Regeln ergänzen diese dort, wo Architektur oder
Serialisierung eine zusätzliche eindeutige Semantik benötigen.

Insbesondere gilt:

-   .NET-Naming-Konventionen sind grundsätzlich einzuhalten.
-   Interfaces verwenden das Präfix `I`.
-   Private Felder verwenden `_camelCase`.
-   Nichtöffentliche `const`-Member dürfen und sollen bevorzugt
    `UPPER_CASE` verwenden.
-   Öffentliche Konstanten folgen den .NET-Namenskonventionen.
-   Kontrollstrukturen verwenden geschweifte Klammern.
-   Unnötige `else`-Blöcke sollen vermieden werden.
-   Scopes sollen möglichst klein gehalten werden.
-   Formatierung soll kompakt, aber gut lesbar bleiben.
-   Längere Parameterlisten sollen sinnvoll auf mehrere Zeilen verteilt
    werden.
-   `Definition` ist für entsprechende Definitionstypen und
    serialisierbare Definitionen verpflichtend.
-   Serialisierbare Definitionen müssen eindeutig von Runtime-Objekten
    unterscheidbar sein.
-   `Base` wird für abstrakte Vererbungsbasen empfohlen, ist jedoch
    nicht verpflichtend.
-   Die öffentliche API soll konsistent und anhand ihrer Namen
    verständlich bleiben.
