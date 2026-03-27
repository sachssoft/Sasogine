# CommandBuffer<T> - Immediate Mode ICommand Tracking

## Übersicht

`CommandBuffer<T>` ist ein leichtgewichtiger, frame-basierter Command-Tracker für Immediate Mode GUIs wie ImGui. Er basiert auf dem ValueBuffer-Prinzip und erlaubt die effiziente Nutzung von `ICommand` in einem Game-Loop oder Editor-Loop.

* Struct-basiert → keine Heap-Allocation
* Lock-frei → Single-Threaded optimiert
* Frame-basiert → Immediate Mode kompatibel
* Generisch → unterstützt Parameter jeder Art

## API

```csharp
public struct CommandBuffer<T>
{
    public void Set(ICommand command);
    public bool ExecuteIfRequested(T parameter, bool shouldExecute);
    public void ResetFrame();
    public bool IsSet { get; }
}
```

### Methoden

* `Set(ICommand command)` → setzt das ICommand, das getrackt werden soll.
* `ExecuteIfRequested(T parameter, bool shouldExecute)` → prüft `CanExecute` und führt aus, wenn `shouldExecute` true ist und noch nicht ausgeführt wurde.
* `ResetFrame()` → bereitet Buffer für den nächsten Frame vor.
* `IsSet` → prüft, ob ein Command gesetzt ist.

## Beispiel

```csharp
using ImGuiNET;
using System.Windows.Input;

ICommand saveCommand = new RelayCommand(() => SaveScene(), () => HasChanges);
CommandBuffer<object> saveBuffer = new CommandBuffer<object>();
saveBuffer.Set(saveCommand);

// Draw Loop
bool pressed = ImGui.Button("Save");
saveBuffer.ExecuteIfRequested(null, pressed);
saveBuffer.ResetFrame();
```
