# Direkte Zufallsaufrufe

```csharp
using Sachssoft.Engine.Gameplay.Curves;
using Sachssoft.Engine.Gameplay.Randomization;

int amount = RandomUtil.Generate(1, 10); // 1 bis einschließlich 10 (wie früher)
int index = RandomUtil.Next(0, 10);     // 0 bis einschließlich 9
float value = RandomUtil.NextSingle(); // [0, 1)
float speed = RandomUtil.NextSingle(2f, 8f);
bool success = RandomUtil.Chance(0.25); // 25 Prozent
string item = RandomUtil.Choose("A", "B", "C");

float shaped = RandomUtil.NextSingle(new CubicInCurve());
int count = RandomUtil.Next(1, 11, new CubicInCurve());

string rarity = RandomUtil.ChooseWeighted(
    new WeightedValue<string>("Common", 80),
    new WeightedValue<string>("Rare", 20));
```

Ohne Seed verwendet die Klasse einen fortlaufenden gemeinsamen Generator mit automatisch erzeugtem Startseed. Seine Zugriffe werden synchronisiert. Für wiederholbare Abläufe einmal `RandomUtil.SetSeed(1234UL)` aufrufen; das setzt den gemeinsamen Strom für alle Aufrufer zurück. Die Reihenfolge paralleler Aufrufe ist nicht festgelegt.

`RandomUtil.Generate(1, 10, 1234)` erstellt für diesen Aufruf einen separaten Strom. Wiederholte identische Aufrufe liefern denselben Wert; sie verändern den gemeinsamen Strom nicht. Negative alte `int`-Seeds werden unterstützt, die alte `System.Random`-Sequenz wird jedoch nicht wiederhergestellt.

Vorhandene Expressions: `RandomUtil.Evaluate(expression)`. Vorhandene Patterns: `RandomUtil.Next(pattern)`. Das Pattern behält seinen eigenen Generator und seine Historie; `SetSeed` setzt es nicht zurück. Für Synchronisierung dasselbe Pattern konsequent über diesen Helfer aufrufen. Noise weiterhin als Quelle behalten und mit `GetValue(...)` abfragen: Es wertet Koordinaten aus und verbraucht keinen gemeinsamen Zufallsstrom.

Für eigene unabhängige Abläufe bleiben `RandomGenerator` und `CurveRandomGenerator` verfügbar. Die früheren generischen `Generate<TGenerator>`- und `GeneratePeaked<TGenerator>`-Aufrufe werden durch diese Instanzen bzw. Curve-Aufrufe ersetzt.

Die statische Komfortschicht für Curves und gewichtete Auswahl erstellt pro Aufruf Hilfsobjekte. Für häufige Abfragen bestehende Generatoren oder Expressions wiederverwenden. Kurven und eigene Expressions werden unter der gemeinsamen Sperre ausgewertet; darin nicht auf andere Threads warten, die ebenfalls `RandomUtil` aufrufen.
