# ValueBuffer<T> - Frame-based Change Detection

## Übersicht

`ValueBuffer<T>` ist eine hochperformante, frame-basierte Struktur, die alte und neue Werte speichert und Änderungen automatisch erkennt. Sie ist ideal für Game Loops, Editor Loops oder andere Szenarien, in denen Werte pro Frame überprüft werden müssen.

* Struct-basiert → keine Heap-Allocation
* Lock-frei → extrem performant
* Frame-basiert → Immediate Mode kompatibel
* Detects changes automatically → `HasChanged` gibt sofort Rückmeldung

## API

```csharp
public struct ValueBuffer<T>
{
    public T OldValue { get; private set; }
    public T NewValue { get; private set; }
    public bool HasChanged { get; }
    public bool WasEnsured { get; }

    public void Set(T value);
    public bool EnsureChange(T value);
    public void Reset(T value);
}
```

### Methoden

* `Set(T value)` → explizit neuen Wert setzen; verschiebt OldValue.
* `EnsureChange(T value)` → prüft, ob sich Wert geändert hat, aktualisiert OldValue → NewValue, gibt true zurück, wenn sich Wert geändert hat oder erstes Mal gesetzt.
* `Reset(T value)` → setzt OldValue und NewValue auf denselben Wert.

## Beispiel

```csharp
ValueBuffer<int> scoreBuffer = new ValueBuffer<int>();

// Update Loop
scoreBuffer.EnsureChange(currentScore);

// Draw Loop
ImGui.Begin("Debug");
if (scoreBuffer.HasChanged)
{
    ImGui.Text($"Score changed: {scoreBuffer.NewValue}");
}
else
{
    ImGui.Text($"Score: {scoreBuffer.NewValue}");
}
ImGui.End();
```
