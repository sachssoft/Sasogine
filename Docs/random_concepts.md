# Konzept: Randomization in Sasogine

## Ziel

Das Randomization-System von Sasogine soll mehr bieten als nur einen klassischen Zufallsgenerator.

Es soll folgende Bereiche abdecken:

- klassischer deterministischer Zufall mit `ulong`-Seed,
- normalisierte Zufallswerte im Bereich `0..1`,
- Verformung normalisierter Zufallswerte über das bestehende `Curves`-System,
- rekursive Zufallsausdrücke,
- muster- und zustandsabhängige Zufallsgeneratoren,
- verlaufsabhängige und adaptive Zufallsmodelle,
- deterministisches Noise in 1D, 2D und 3D,
- Kombination mehrerer Noise-Ebenen,
- vollständige Cross-Platform- und NativeAOT-Tauglichkeit.

Das System soll vollständig in C# implementiert werden und keine nativen Abhängigkeiten benötigen.

---

# Namespace

```csharp
Sachssoft.Engine.Gameplay.Randomization
```

Empfohlene Struktur:

```text
Gameplay
├── Curves
│   ├── CurveBase
│   ├── LinearCurve
│   ├── QuadraticInCurve
│   ├── CubicInCurve
│   └── ...
│
└── Randomization
    ├── Generators
    ├── Expressions
    ├── Patterns
    └── Noise
```

`Curves` bleibt ein eigenes Gameplay-System.

`Randomization` verwendet `Curves`, um normalisierte Zufallswerte zu gewichten oder zu verformen.

---

# 1. Klassischer Zufallsgenerator

Der klassische Zufallsgenerator ist die einfachste Form.

Er besitzt einen internen Zustand und erzeugt bei jedem Aufruf den nächsten Zufallswert.

Beispiel:

```csharp
var random = new RandomGenerator(1234UL);

float value = random.NextSingle();
int number = random.Next(0, 100);
```

## Eigenschaften

- deterministisch,
- `ulong` als Seed,
- gleiche Seed-Werte erzeugen dieselbe Zufallsfolge,
- schnell,
- keine Abhängigkeit von `System.Random`,
- geeignet für reproduzierbare Spielwelten und Simulationen.

Vorgeschlagenes Interface:

```csharp
public interface IRandomGenerator
{
    ulong Seed { get; }

    ulong NextUInt64();

    int Next(int minimum, int maximum);

    float NextSingle();

    float NextSingle(float minimum, float maximum);
}
```

Vorgeschlagene Implementierung:

```csharp
public sealed class RandomGenerator : IRandomGenerator
{
    public RandomGenerator(ulong seed)
    {
        Seed = seed;
    }

    public ulong Seed { get; }

    // Interner PRNG-Zustand.
}
```

Der klassische Generator ist bewusst **nicht rekursiv**.

Er erzeugt nur den nächsten Roh-Zufallswert.

---

# 2. Normalisierte Zufallswerte

Die zentrale Zufallsdarstellung soll sein:

```text
0..1
```

Beispiel:

```csharp
float value = random.NextSingle();
```

Dieser Wert kann anschließend beliebig weiterverarbeitet werden.

Zum Beispiel:

```text
RandomGenerator
      ↓
     0..1
      ↓
    Curve
      ↓
     0..1
      ↓
Zielbereich
```

Beispiel für einen Zielbereich von `-10` bis `30`:

```csharp
float normalized = random.NextSingle();
float result = -10f + 40f * normalized;
```

---

# 3. Curves statt eigener Distribution-Hierarchie

Für Verteilungen soll zunächst kein eigenes `Distributions`-System eingeführt werden.

Das vorhandene:

```csharp
Sachssoft.Engine.Gameplay.Curves
```

übernimmt diese Aufgabe.

Beispiele:

```text
LinearCurve
QuadraticInCurve
QuadraticOutCurve
CubicInCurve
CubicOutCurve
SineInOutCurve
```

Ein Zufallswert kann dadurch einfach verformt werden:

```csharp
CurveBase curve = new CubicInCurve();

float normalized = random.NextSingle();
float shaped = curve.GetValue(normalized);
```

Danach wird der Wert in den gewünschten Bereich übertragen:

```csharp
float result = minimum + (maximum - minimum) * shaped;
```

So bleiben Zufallserzeugung und Wertverformung sauber getrennt.

---

# 4. Nicht-rekursiver Zufall

Der normale `RandomGenerator` bleibt bewusst einfach.

Beispiel:

```csharp
float value = random.NextSingle(10f, 100f);
```

Er besitzt keine verschachtelten Regeln und kennt keine anderen Generatoren.

Diese einfache Form ist wichtig für:

- schnelle Einzelwerte,
- Würfelmechaniken,
- Positionen,
- Farben,
- Größen,
- zufällige Richtungen,
- einfache Gameplay-Entscheidungen.

---

# 5. Rekursive Zufallsausdrücke

Für verschachtelten Zufall wird kein eigener rekursiver Generator benötigt.

Stattdessen wird ein Ausdruckssystem verwendet.

Namespace:

```csharp
Sachssoft.Engine.Gameplay.Randomization.Expressions
```

Basisschnittstelle:

```csharp
public interface IRandomExpression<T>
{
    T GetValue(IRandomGenerator random);
}
```

Dadurch können Zufallswerte aus anderen Zufallswerten aufgebaut werden.

Beispiel:

```text
Rand(
    Rand(0, 10),
    Rand(10, 100))
```

Das bedeutet:

1. Die untere Grenze wird zufällig bestimmt.
2. Die obere Grenze wird zufällig bestimmt.
3. Zwischen beiden Grenzen wird erneut ein Zufallswert erzeugt.

Mögliche Ausdruckstypen:

```text
ConstantExpression<T>
RangeExpression
ChoiceExpression<T>
WeightedChoiceExpression<T>
CurveExpression
```

Beispiel:

```csharp
var expression =
    new RangeExpression(
        new RangeExpression(0f, 10f),
        new RangeExpression(10f, 100f));

float value = expression.GetValue(random);
```

Der Vorteil ist, dass beliebig tiefe Zufallsbäume möglich sind, ohne den eigentlichen `RandomGenerator` kompliziert zu machen.

---

# 6. Musterbedingter Zufall

Neben klassischem und rekursivem Zufall soll Sasogine auch Zufall unterstützen, der von früheren Ergebnissen oder aktuellen Zuständen abhängt.

Dafür wird ein eigener Bereich vorgeschlagen:

```csharp
Sachssoft.Engine.Gameplay.Randomization.Patterns
```

Das ist kein klassischer unabhängiger Zufall mehr.

Der nächste Wert kann abhängen von:

- dem vorherigen Ergebnis,
- mehreren früheren Ergebnissen,
- dem aktuellen Zustand,
- Häufigkeiten,
- Wiederholungen,
- Gewichtungen,
- Erfolgen oder Fehlschlägen.

Dieses Verhalten ähnelt einfachen KI-Entscheidungssystemen, ohne dass dafür echte KI notwendig ist.

---

# 7. Markov-Generator

Ein Markov-Generator bestimmt den nächsten Zustand anhand des aktuellen Zustands.

Beispiel Wetter:

```text
Sunny
├── Sunny   70 %
├── Cloudy  25 %
└── Rain     5 %

Rain
├── Rain    40 %
├── Cloudy  50 %
└── Sunny   10 %
```

Möglicher Typ:

```csharp
MarkovGenerator<T>
```

Beispiel:

```csharp
var weather = new MarkovGenerator<WeatherState>(seed);
```

Anwendung:

- Wetter,
- KI-Zustände,
- Musikübergänge,
- Verkehrsverhalten,
- NPC-Verhalten,
- prozedurale Ereignisketten.

---

# 8. HistoryRandomGenerator

Dieser Generator berücksichtigt eine Historie früherer Ergebnisse.

Beispiel:

```text
A
A
A
A
```

Nach mehreren gleichen Ergebnissen kann `A` automatisch unwahrscheinlicher werden.

Mögliche Regeln:

- maximal drei gleiche Ergebnisse hintereinander,
- kürzlich verwendete Werte werden seltener,
- lange nicht verwendete Werte werden wahrscheinlicher,
- bestimmte Sequenzen werden vermieden,
- Wiederholungen werden gezielt begrenzt.

Möglicher Typ:

```csharp
HistoryRandomGenerator<T>
```

Nützlich für:

- Loot,
- Animationen,
- Gegner-Spawns,
- Sounds,
- Dialogvarianten,
- Streckenelemente,
- Landschaftsobjekte,
- Wetter.

---

# 9. AdaptiveRandomGenerator

Ein adaptiver Generator verändert seine Wahrscheinlichkeiten während der Laufzeit.

Beispiel:

```text
Legendary nicht erhalten
        ↓
Chance steigt leicht
        ↓
wieder nicht erhalten
        ↓
Chance steigt weiter
        ↓
Legendary erhalten
        ↓
Chance wird zurückgesetzt
```

Möglicher Typ:

```csharp
AdaptiveRandomGenerator<T>
```

Das eignet sich unter anderem für:

- Pity-Systeme,
- dynamische Schwierigkeitsanpassung,
- Spawn-Verteilungen,
- wiederholungsarme Auswahl,
- adaptive Ereignisse.

---

# 10. WeightedPatternGenerator

Ein einfacherer musterbasierter Generator kann Gewichte abhängig von Regeln verändern.

Beispiel:

```text
Normal:
A = 50 %
B = 30 %
C = 20 %

Wenn A dreimal hintereinander kam:
A = 10 %
B = 50 %
C = 40 %
```

Möglicher Typ:

```csharp
WeightedPatternGenerator<T>
```

Das ist flexibler als ein reiner Weighted-Choice-Generator, weil die Gewichte vom Verlauf abhängen dürfen.

---

# 11. PatternRandomization als eigener Bereich

Empfohlene Struktur:

```text
Randomization
├── Generators
│   └── RandomGenerator
│
├── Expressions
│   ├── ConstantExpression<T>
│   ├── RangeExpression
│   ├── ChoiceExpression<T>
│   └── WeightedChoiceExpression<T>
│
├── Patterns
│   ├── MarkovGenerator<T>
│   ├── HistoryRandomGenerator<T>
│   ├── AdaptiveRandomGenerator<T>
│   └── WeightedPatternGenerator<T>
│
└── Noise
```

Die Bereiche haben unterschiedliche Aufgaben:

```text
Generators
→ unabhängiger klassischer Zufall

Expressions
→ rekursiv zusammengesetzter Zufall

Patterns
→ zustands- und verlaufsabhängiger Zufall

Noise
→ koordinatenabhängiger deterministischer Zufall
```

---

# 12. Noise

Noise ist ebenfalls Teil von `Randomization`, funktioniert aber anders als ein klassischer Zufallsgenerator.

Ein normaler Zufallsgenerator verändert seinen Zustand:

```text
Next()
Next()
Next()
```

Noise wird über Koordinaten abgefragt:

```text
GetValue(x)
GetValue(x, y)
GetValue(x, y, z)
```

Für dieselbe Position und denselben Seed wird immer derselbe Wert erzeugt.

---

# 13. Noise1, Noise2 und Noise3

Die Dimensionen werden explizit modelliert:

```text
Noise1
Noise2
Noise3
```

Nicht:

```text
Noise<TDimension>
```

Empfohlene Interfaces:

```csharp
public interface INoise1
{
    float GetValue(float x);
}
```

```csharp
public interface INoise2
{
    float GetValue(float x, float y);
}
```

```csharp
public interface INoise3
{
    float GetValue(float x, float y, float z);
}
```

Optional können für 2D und 3D zusätzliche Convenience-Overloads angeboten werden:

```csharp
float GetValue(Vector2 position);
```

und:

```csharp
float GetValue(Vector3 position);
```

Diese Namensgebung passt zu bestehenden Sasogine-Typen wie:

```text
Point2
Point3
PixelPoint2
PixelPoint3
Vector2
Vector3
```

---

# 14. Noise-Implementierungen

Vorgeschlagene Algorithmen:

```text
PerlinNoise1
PerlinNoise2
PerlinNoise3

ValueNoise1
ValueNoise2
ValueNoise3

WorleyNoise1
WorleyNoise2
WorleyNoise3
```

Später möglich:

```text
SimplexNoise2
SimplexNoise3

OpenSimplexNoise2
OpenSimplexNoise3
```

Nicht jeder Algorithmus muss zwangsläufig alle Dimensionen unterstützen.

---

# 15. Normalisierter Noise-Bereich

Alle öffentlichen Noise-Implementierungen sollen möglichst denselben Wertebereich liefern:

```text
0..1
```

Dadurch können Noise-Werte direkt mit `Curves` kombiniert werden.

Beispiel:

```csharp
float value = noise.GetValue(x, y);
value = curve.GetValue(value);
```

Das ist besonders nützlich für Landschaften.

Beispiel:

```text
Perlin Noise
     ↓
    0..1
     ↓
CubicInCurve
     ↓
mehr niedrige Flächen
weniger hohe Flächen
```

---

# 16. FractalNoise / Oktaven

Mehrere Noise-Ebenen können kombiniert werden.

Dafür werden Wrapper vorgeschlagen:

```text
FractalNoise1
FractalNoise2
FractalNoise3
```

Beispiel:

```csharp
INoise2 noise =
    new FractalNoise2(
        new PerlinNoise2(seed));
```

Typische Einstellungen:

```text
Octaves
Frequency
Lacunarity
Persistence
```

Prinzip:

```text
niedrige Frequenz + hohe Stärke
mittlere Frequenz + mittlere Stärke
hohe Frequenz + geringe Stärke
               ↓
        komplexes Gesamt-Noise
```

---

# 17. Gesamtstruktur

```text
Sachssoft.Engine.Gameplay
│
├── Curves
│   ├── CurveBase
│   ├── LinearCurve
│   ├── QuadraticInCurve
│   ├── CubicInCurve
│   └── ...
│
└── Randomization
    │
    ├── Generators
    │   ├── IRandomGenerator
    │   └── RandomGenerator
    │
    ├── Expressions
    │   ├── IRandomExpression<T>
    │   ├── ConstantExpression<T>
    │   ├── RangeExpression
    │   ├── ChoiceExpression<T>
    │   └── WeightedChoiceExpression<T>
    │
    ├── Patterns
    │   ├── MarkovGenerator<T>
    │   ├── HistoryRandomGenerator<T>
    │   ├── AdaptiveRandomGenerator<T>
    │   └── WeightedPatternGenerator<T>
    │
    └── Noise
        ├── INoise1
        ├── INoise2
        ├── INoise3
        │
        ├── PerlinNoise1
        ├── PerlinNoise2
        ├── PerlinNoise3
        │
        ├── ValueNoise1
        ├── ValueNoise2
        ├── ValueNoise3
        │
        ├── WorleyNoise1
        ├── WorleyNoise2
        ├── WorleyNoise3
        │
        ├── FractalNoise1
        ├── FractalNoise2
        └── FractalNoise3
```

---

# 18. Grundprinzipien

## Determinismus

Alle Generatoren und Noise-Systeme sollen reproduzierbar sein.

```text
gleicher Seed
+ gleiche Eingaben
= gleiche Ergebnisse
```

---

## Trennung der Aufgaben

```text
RandomGenerator
→ erzeugt klassischen Zufall

Expressions
→ verschachteln Zufall

Patterns
→ berücksichtigen Verlauf und Zustand

Noise
→ erzeugt koordinatenabhängigen Zufall

Curves
→ formen normalisierte Werte
```

---

## 0..1 als gemeinsame Sprache

Normalisierte Werte bilden die gemeinsame Schnittstelle.

```text
Random
Noise
Pattern-Gewicht
Curve
Interpolation
```

können dadurch leicht miteinander kombiniert werden.

---

## Composition statt komplizierter Vererbung

Beispiele:

```text
RandomGenerator
+ Curve

PerlinNoise2
+ FractalNoise2
+ CubicInCurve

RandomGenerator
+ HistoryRandomGenerator

RandomGenerator
+ RandomExpression
```

Die einzelnen Teile bleiben unabhängig und wiederverwendbar.

---

## Cross-Platform

Das komplette System soll:

- vollständig managed sein,
- keine nativen Bibliotheken verwenden,
- kein P/Invoke verwenden,
- keine Reflection voraussetzen,
- keine dynamische Codegenerierung verwenden,
- NativeAOT-kompatibel bleiben.

---

# 19. Beispielanwendungen

## Klassischer Zufall

```csharp
var random = new RandomGenerator(1234UL);

float value = random.NextSingle(-10f, 30f);
```

---

## Zufall mit Curve

```csharp
CurveBase curve = new CubicInCurve();

float value = curve.GetValue(random.NextSingle());
value = -10f + 40f * value;
```

---

## Rekursiver Zufall

```csharp
IRandomExpression<float> expression =
    new RangeExpression(
        new RangeExpression(0f, 10f),
        new RangeExpression(10f, 100f));

float value = expression.GetValue(random);
```

---

## Musterabhängiger Zufall

```csharp
var generator =
    new HistoryRandomGenerator<string>(
        random);
```

Der Generator kann frühere Ergebnisse berücksichtigen und Wiederholungen vermeiden.

---

## Markov-Wetter

```csharp
var weather =
    new MarkovGenerator<WeatherState>(
        random);
```

Der nächste Wetterzustand hängt vom aktuellen Wetter ab.

---

## 1D-Noise

```csharp
INoise1 noise = new PerlinNoise1(1234UL);

float value = noise.GetValue(time);
```

Anwendungen:

- Fackelflackern,
- Kamera-Bewegung,
- Wind,
- organische Animationen.

---

## 2D-Noise

```csharp
INoise2 noise = new PerlinNoise2(1234UL);

float height = noise.GetValue(x, y);
```

Anwendungen:

- Landschaft,
- Terrain,
- Vegetationsdichte,
- Materialverteilung.

---

## Fraktales Terrain

```csharp
INoise2 noise =
    new FractalNoise2(
        new PerlinNoise2(1234UL));

float height = noise.GetValue(x, y);
```

---

# 20. Empfohlene erste Implementierungsstufe

Zuerst:

```text
IRandomGenerator
RandomGenerator

IRandomExpression<T>
ConstantExpression<T>
RangeExpression
ChoiceExpression<T>

INoise1
INoise2
INoise3

ValueNoise1
ValueNoise2
ValueNoise3

PerlinNoise1
PerlinNoise2
PerlinNoise3

FractalNoise1
FractalNoise2
FractalNoise3
```

Danach:

```text
MarkovGenerator<T>
HistoryRandomGenerator<T>
AdaptiveRandomGenerator<T>
WeightedPatternGenerator<T>

WorleyNoise1
WorleyNoise2
WorleyNoise3
```

Damit bleibt die erste Version überschaubar, ohne die spätere Architektur einzuschränken.
