# Sasogine -- Programmier- und Architekturrichtlinie

Diese Richtlinie beschreibt das grundlegende Architekturprinzip für die
Trennung zwischen `Definition` und Engine-Objekt in Sasogine.

Das Prinzip ist mit Architekturmustern wie MVVM vergleichbar: Daten und
deren Verwendung werden bewusst voneinander getrennt. Sasogine verwendet
jedoch ein eigenes Definition-/Engine-Modell.

------------------------------------------------------------------------

## 1. Grundprinzip

Konfigurierbare Daten eines Engine-Objekts werden durch eine
`Definition` beschrieben.

Woher diese Daten stammen, ist für die Architektur nicht relevant.

Beispiele:

-   Code
-   JSON
-   XML
-   Editor
-   Package
-   externe Datenquelle

Entscheidend ist die Grenze:

``` text
Any Source
    │
    ▼
Definition
    │
    ▼
Engine-Objekt
```

Jedes Engine-Objekt stellt seine zugehörige `Definition` öffentlich
bereit.

Für die Verwendung von Definition-Daten durch ein Engine-Objekt
existieren genau zwei Arten:

1.  **Live**
2.  **Applied**

**Live ist grundsätzlich zu bevorzugen.**

Applied soll nur verwendet werden, wenn das Engine-Objekt tatsächlich
einen eigenständigen Zustand benötigt, der sich unabhängig von seiner
`Definition` entwickeln kann.

------------------------------------------------------------------------

## 2. Live

Beim Live-Modell ist die `Definition` die direkte und maßgebliche
Datenquelle des Engine-Objekts.

Das Engine-Objekt verwendet die Werte direkt aus seiner `Definition`.

``` csharp
float zoom = camera.Definition.Zoom;
```

Eine Änderung an der Definition ist unmittelbar wirksam:

``` csharp
camera.Definition.Zoom = 2f;
```

Für diesen Wert ist keine zusätzliche Übertragung in einen eigenen
Zustand des Engine-Objekts erforderlich.

``` text
Definition.Zoom
      │
      ▼
Engine-Objekt verwendet
den Wert direkt
```

### Optionale Engine-Property

Da jedes Engine-Objekt bereits öffentlichen Zugriff auf seine
`Definition` besitzt, ist eine zusätzliche Property im Engine-Objekt
nicht erforderlich.

Sie kann jedoch als Komfort-API angeboten werden:

``` csharp
public float Zoom => Definition.Zoom;
```

Damit sind beispielsweise beide Zugriffe möglich:

``` csharp
camera.Definition.Zoom
camera.Zoom
```

Eine solche Property sollte bevorzugt get-only sein.

Eine schreibbare Proxy-Property ist technisch möglich:

``` csharp
public float Zoom
{
    get => Definition.Zoom;
    set => Definition.Zoom = value;
}
```

Sie ist aus Architektur-Sicht jedoch nicht empfohlen.

Änderungen sollen bevorzugt direkt an der `Definition` vorgenommen
werden:

``` csharp
camera.Definition.Zoom = 2f;
```

Dadurch bleibt eindeutig erkennbar, dass die `Definition` die
eigentliche Datenquelle ist.

### Verwendung

Live soll verwendet werden, wenn die `Definition` selbst den aktuellen
Zustand darstellen kann.

Typische Beispiele:

-   visuelle Eigenschaften,
-   Farben,
-   Kameraeinstellungen,
-   Tool-Einstellungen,
-   Größen,
-   Parameter,
-   unmittelbar editierbare Konfigurationen.

### Live-Regel

> Die `Definition` ist die aktuelle Datenquelle des Engine-Objekts.
>
> Eine zusätzliche get-only Property im Engine-Objekt ist optional.
>
> Live ist gegenüber Applied zu bevorzugen, solange kein unabhängiger
> Zustand des Engine-Objekts erforderlich ist.

------------------------------------------------------------------------

## 3. Applied

Applied wird verwendet, wenn ein Definition-Wert lediglich einen
Ausgangs- oder Konfigurationszustand beschreibt und das Engine-Objekt
anschließend einen davon unabhängigen Zustand besitzt.

Die Übertragung erfolgt innerhalb von:

``` csharp
ConfigureFromDefinition()
```

Beispiel:

``` csharp
protected override void ConfigureFromDefinition()
{
    base.ConfigureFromDefinition();

    _body.SetTransform(
        Definition.StartPosition,
        Definition.StartRotation);
}
```

Der Datenfluss ist:

``` text
Definition
    │
    │ ConfigureFromDefinition()
    ▼
Engine-Objekt-Zustand
```

Danach sind beide Zustände bewusst voneinander getrennt:

``` text
Definition State        Engine-Objekt-Zustand
       │                         │
       └────── getrennt ─────────┘
```

Das Engine-Objekt arbeitet anschließend mit seinem eigenen Zustand.

Die `Definition` darf für diesen Wert nicht gleichzeitig als aktueller
Zustand des Engine-Objekts interpretiert werden.

### Applied-Regel

> Applied wird verwendet, wenn das Engine-Objekt einen eigenständigen
> Zustand benötigt.
>
> Die Übertragung aus der `Definition` erfolgt über
> `ConfigureFromDefinition()`.
>
> Danach arbeitet das Engine-Objekt mit seinem eigenen Zustand, bis eine
> erneute Konfiguration erfolgt.

------------------------------------------------------------------------

## 4. Beispiel: Charakter mit Physik

Ein Charakter kann beispielsweise eine Startposition besitzen:

``` csharp
Definition.StartPosition
```

Diese beschreibt ausschließlich die Position, an der der Charakter
beginnen soll.

Beim Konfigurieren des Engine-Objekts wird die Startposition auf einen
Physik-Body angewendet:

``` csharp
protected override void ConfigureFromDefinition()
{
    base.ConfigureFromDefinition();

    _body.SetTransform(
        Definition.StartPosition,
        Definition.StartRotation);
}
```

Danach übernimmt beispielsweise Box2D die Bewegung:

``` text
CharacterDefinition.StartPosition
              │
              │ ConfigureFromDefinition()
              ▼
          Box2D Body
              │
              ▼
       Physiksimulation
```

Die aktuelle Position gehört nun zum Zustand des Engine-Objekts
beziehungsweise seines Physik-Bodys.

`Definition.StartPosition` bleibt dagegen die konfigurierte
Startposition.

Deshalb wäre folgende Implementierung falsch:

``` csharp
public Vector2 Position => Definition.StartPosition;
```

Die aktuelle Position muss stattdessen aus dem tatsächlichen Zustand des
Physik-Bodys gelesen werden.

Applied ist in diesem Fall notwendig, weil sich die aktuelle Position
unabhängig von der ursprünglichen Startposition entwickelt.

------------------------------------------------------------------------

## 5. Live gegenüber Applied

Live ist die bevorzugte Architektur.

Applied erzeugt bewusst einen zusätzlichen, von der `Definition`
getrennten Zustand. Deshalb soll Applied nicht verwendet werden, wenn
derselbe Zweck mit Live erreicht werden kann.

### Nicht empfohlen

Wenn `Zoom` keinen unabhängigen Zustand benötigt, wäre folgende
Implementierung unnötig:

``` csharp
private float _zoom;

protected override void ConfigureFromDefinition()
{
    base.ConfigureFromDefinition();

    _zoom = Definition.Zoom;
}

public float Zoom => _zoom;
```

Es existieren dadurch unnötig zwei Zustände:

``` text
Definition.Zoom
      │
      ▼
    _zoom
```

### Bevorzugt

Direkter Zugriff:

``` csharp
camera.Definition.Zoom
```

oder optional als get-only Komfort-Property:

``` csharp
public float Zoom => Definition.Zoom;
```

### Entscheidungsregel

``` text
                Definition Property
                        │
                        ▼
       Benötigt das Engine-Objekt einen
        unabhängigen eigenen Zustand?
                  │             │
                Nein            Ja
                  │             │
                  ▼             ▼
                LIVE         APPLIED
                  │             │
          direkt verwenden     │
                                ▼
                    ConfigureFromDefinition()
                                │
                                ▼
                    Engine-Objekt-Zustand
```

Kurz:

> **Live:** Die `Definition` ist die aktuelle Datenquelle.
>
> **Applied:** Die `Definition` konfiguriert einen eigenständigen
> Zustand des Engine-Objekts.

------------------------------------------------------------------------

## 6. ConfigureFromDefinition und Engine-Lifecycle

`ConfigureFromDefinition()` gehört zum allgemeinen Lifecycle eines
Engine-Objekts.

Die Methode kann im Zusammenhang mit folgenden Vorgängen ausgeführt
werden:

-   `Load()`
-   `LoadAsync()`
-   `Reload()`
-   `ReloadAsync()`

``` text
Load / LoadAsync
Reload / ReloadAsync
        │
        ▼
ConfigureFromDefinition()
        │
        ▼
   Engine-Objekt
```

Der Aufruf von `ConfigureFromDefinition()` ist nicht gleichbedeutend
damit, dass alle Eigenschaften Applied verwenden.

### Bei Live

Ein Live-Wert muss innerhalb von `ConfigureFromDefinition()`
normalerweise nicht verarbeitet werden:

``` csharp
public float Zoom => Definition.Zoom;
```

Wird `ConfigureFromDefinition()` durch den Engine-Lifecycle trotzdem
ausgeführt, ist für `Zoom` keine zusätzliche Aktion erforderlich.

### Bei Applied

Applied-Werte werden innerhalb von `ConfigureFromDefinition()` auf den
eigenständigen Zustand des Engine-Objekts angewendet:

``` csharp
protected override void ConfigureFromDefinition()
{
    base.ConfigureFromDefinition();

    _body.SetTransform(
        Definition.StartPosition,
        Definition.StartRotation);
}
```

Ein späteres `Reload()` beziehungsweise `ReloadAsync()` kann dadurch die
aktuellen Werte der `Definition` erneut auf das Engine-Objekt anwenden.

------------------------------------------------------------------------

## 7. Editor und Inspektor

Der Live-/Applied-Unterschied muss nicht als zusätzlicher Modus oder
Metadatum im Engine-Objekt gespeichert werden.

Ein Editor oder Inspektor darf nach einer Änderung an der `Definition`
grundsätzlich ein `Reload()` ausführen.

``` text
Inspektor
    │
    │ Definition ändern
    ▼
Definition
    │
    ▼
Reload()
    │
    ▼
ConfigureFromDefinition()
```

### Live

Die Änderung ist bereits unmittelbar über die `Definition` wirksam.

Der zusätzliche Reload ist für diesen Live-Wert nicht notwendig,
verursacht architektonisch aber kein Problem.

### Applied

Der Reload sorgt dafür, dass der geänderte Definition-Wert über
`ConfigureFromDefinition()` erneut auf den Zustand des Engine-Objekts
angewendet wird.

Dadurch benötigt der Inspektor keine Kenntnis darüber, ob eine einzelne
Property Live oder Applied verwendet wird.

``` text
Inspektor ändert Property
          │
          ▼
        Reload
          │
     ┌────┴────┐
     │         │
    Live    Applied
     │         │
     │         ▼
     │   Wert erneut anwenden
     │
     ▼
keine zusätzliche
Aktion erforderlich
```

------------------------------------------------------------------------

## 8. Vergleich mit MVVM

Das Sasogine-Prinzip ist nicht MVVM, verfolgt aber einen vergleichbaren
architektonischen Gedanken: Daten und deren Verwendung werden klar
voneinander getrennt.

Vereinfacht:

  MVVM        Sasogine
  ----------- -----------------------------------------------------
  Model       `Definition`
  ViewModel   Engine-Objekt
  View        Darstellung / Rendering
  Binding     Verwendung der `Definition` durch das Engine-Objekt

Diese Gegenüberstellung dient nur dem konzeptionellen Verständnis.
Sasogine implementiert kein MVVM.

MVVM beschreibt typischerweise einen Datenfluss wie:

``` text
Model
  │
  ▼
ViewModel
  │
  ▼
View
```

Sasogine konzentriert sich auf:

``` text
Definition
    │
    ▼
Engine-Objekt
```

Dabei existieren genau zwei Arten der Verwendung:

``` text
                    Definition
                        │
              ┌─────────┴─────────┐
              │                   │
              ▼                   ▼
             LIVE               APPLIED
              │                   │
              ▼                   ▼
      direkter Zugriff   ConfigureFromDefinition()
                                  │
                                  ▼
                         Engine-Objekt-Zustand
```

Der wesentliche Unterschied:

**MVVM** beschreibt hauptsächlich die Trennung und Kommunikation
zwischen Model, ViewModel und View.

**Sasogine** definiert die Grenze zwischen `Definition` und
Engine-Objekt und legt fest, wie Definition-Daten vom Engine-Objekt
verwendet werden.

------------------------------------------------------------------------

## 9. Architektur-Kurzregel

Bei einem neuen Definition-basierten Zustand sind nur wenige Fragen
notwendig:

1.  Kann die `Definition` selbst die aktuelle Datenquelle sein?
2.  Falls ja: **Live verwenden.**
3.  Muss sich der Zustand des Engine-Objekts unabhängig von der
    `Definition` entwickeln?
4.  Nur dann: **Applied verwenden und den Zustand über
    `ConfigureFromDefinition()` konfigurieren.**

Das grundlegende Prinzip lautet:

> **Live bevorzugen.**
>
> **Applied nur verwenden, wenn ein eigenständiger Zustand des
> Engine-Objekts tatsächlich erforderlich ist.**
>
> **Bei Applied erfolgt die Übertragung aus der `Definition` über
> `ConfigureFromDefinition()`.**
