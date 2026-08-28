# Capability Concepts

Allgemeine Fähigkeiten von Engine- und Gameplay-Objekten.

## Core

| Capability    | Wichtigkeit | Verwendung                          |
| ------------- | :---------: | ----------------------------------- |
| `IMovable`    |    ★★★★★    | Position eines Objekts verändern    |
| `IRotatable`  |    ★★★★☆    | Rotation eines Objekts verändern    |
| `IEnableable` |    ★★★★★    | Objekt aktivieren oder deaktivieren |

## Gameplay

| Capability      | Wichtigkeit | Verwendung                                            |
| --------------- | :---------: | ----------------------------------------------------- |
| `IPickupable`   |    ★★★★★    | Objekt kann aufgenommen werden                        |
| `IUsable`       |    ★★★★★    | Objekt kann benutzt/verwendet werden                  |
| `IInteractable` |    ★★★★★    | Spieler oder andere Objekte können damit interagieren |
| `IDamageable`   |    ★★★★★    | Objekt kann Schaden erhalten                          |
| `IDestructible` |    ★★★★☆    | Objekt kann zerstört werden                           |
| `ICarryable`    |    ★★★☆☆    | Objekt kann getragen werden                           |
| `ITargetable`   |    ★★★☆☆    | Objekt kann als Ziel ausgewählt werden                |
| `IControllable` |    ★★★☆☆    | Objekt kann durch einen Controller gesteuert werden   |

## Physics

| Capability    | Wichtigkeit | Verwendung                                               |
| ------------- | :---------: | -------------------------------------------------------- |
| `ICollidable` |    ★★★★★    | Objekt kann an Kollisionen teilnehmen                    |
| `IPhysical`   |    ★★★★☆    | Objekt besitzt physikalische Eigenschaften               |
| `IPushable`   |    ★★★☆☆    | Objekt kann durch andere Objekte bewegt/geschoben werden |

## Animation

| Capability    | Wichtigkeit | Verwendung                                     |
| ------------- | :---------: | ---------------------------------------------- |
| `IAnimatable` |    ★★★☆☆    | Objekt kann Animationen abspielen oder steuern |

## Environment / Gameplay

| Capability   | Wichtigkeit | Verwendung                                        |
| ------------ | :---------: | ------------------------------------------------- |
| `IClimbable` |    ★★☆☆☆    | Objekt/Fläche kann erklommen werden               |
| `ISwimmable` |    ★★☆☆☆    | Objekt/Fläche kann zum Schwimmen verwendet werden |
| `IBurnable`  |    ★★☆☆☆    | Objekt kann brennen                               |
| `IFreezable` |    ★★☆☆☆    | Objekt kann eingefroren werden                    |

## Vorläufige Kernmenge

Die zunächst wichtigsten Kandidaten für `Common.Capabilities`:

```text
IMovable
IRotatable
IEnableable

IPickupable
IUsable
IInteractable
IDamageable
IDestructible

ICollidable
```

Weitere Capabilities erst hinzufügen, wenn sie im Engine- oder Gameplay-Modell tatsächlich benötigt werden.
