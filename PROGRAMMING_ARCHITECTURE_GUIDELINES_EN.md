# Sasogine -- Programming and Architecture Guidelines

These guidelines describe Sasogine's fundamental architectural principle
for separating a `Definition` from an engine object.

The principle is comparable to architectural patterns such as MVVM: data
and its use are deliberately separated. Sasogine, however, uses its own
Definition/Engine model.

------------------------------------------------------------------------

## 1. Core Principle

Configurable data for an engine object is described by a `Definition`.

Where this data comes from is irrelevant to the architecture.

Examples:

-   Code
-   JSON
-   XML
-   Editor
-   Package
-   external data source

The important boundary is:

``` text
Any Source
    │
    ▼
Definition
    │
    ▼
Engine Object
```

Every engine object publicly exposes its associated `Definition`.

There are exactly two ways for an engine object to use definition data:

1.  **Live**
2.  **Applied**

**Live should generally be preferred.**

Applied should only be used when the engine object actually requires an
independent state that can evolve separately from its `Definition`.

------------------------------------------------------------------------

## 2. Live

With the Live model, the `Definition` is the direct and authoritative
data source for the engine object.

The engine object uses values directly from its `Definition`.

``` csharp
float zoom = camera.Definition.Zoom;
```

A change to the definition takes effect immediately:

``` csharp
camera.Definition.Zoom = 2f;
```

No additional transfer into a separate engine object state is required
for this value.

``` text
Definition.Zoom
      │
      ▼
Engine Object uses
the value directly
```

### Optional Engine Property

Because every engine object already provides public access to its
`Definition`, an additional property on the engine object is not
required.

It may, however, be provided as a convenience API:

``` csharp
public float Zoom => Definition.Zoom;
```

For example, both forms of access are then possible:

``` csharp
camera.Definition.Zoom
camera.Zoom
```

Such a property should preferably be get-only.

A writable proxy property is technically possible:

``` csharp
public float Zoom
{
    get => Definition.Zoom;
    set => Definition.Zoom = value;
}
```

However, it is not recommended from an architectural perspective.

Changes should preferably be made directly to the `Definition`:

``` csharp
camera.Definition.Zoom = 2f;
```

This keeps it clear that the `Definition` is the actual data source.

### Usage

Live should be used when the `Definition` itself can represent the
current state.

Typical examples include:

-   visual properties,
-   colors,
-   camera settings,
-   tool settings,
-   sizes,
-   parameters,
-   directly editable configuration.

### Live Rule

> The `Definition` is the current data source for the engine object.
>
> An additional get-only property on the engine object is optional.
>
> Live should be preferred over Applied as long as no independent engine
> object state is required.

------------------------------------------------------------------------

## 3. Applied

Applied is used when a definition value only describes an initial or
configuration state and the engine object subsequently owns a state that
is independent from it.

The transfer takes place inside:

``` csharp
ConfigureFromDefinition()
```

Example:

``` csharp
protected override void ConfigureFromDefinition()
{
    base.ConfigureFromDefinition();

    _body.SetTransform(
        Definition.StartPosition,
        Definition.StartRotation);
}
```

The data flow is:

``` text
Definition
    │
    │ ConfigureFromDefinition()
    ▼
Engine Object State
```

Afterwards, the two states are deliberately separated:

``` text
Definition State        Engine Object State
       │                         │
       └────── separated ────────┘
```

The engine object then works with its own state.

For that value, the `Definition` must not simultaneously be interpreted
as the current state of the engine object.

### Applied Rule

> Applied is used when the engine object requires an independent state.
>
> State is transferred from the `Definition` through
> `ConfigureFromDefinition()`.
>
> Afterwards, the engine object works with its own state until
> configuration is applied again.

------------------------------------------------------------------------

## 4. Example: Character with Physics

A character may, for example, have a start position:

``` csharp
Definition.StartPosition
```

This describes only the position where the character should begin.

When configuring the engine object, the start position is applied to a
physics body:

``` csharp
protected override void ConfigureFromDefinition()
{
    base.ConfigureFromDefinition();

    _body.SetTransform(
        Definition.StartPosition,
        Definition.StartRotation);
}
```

Afterwards, a system such as Box2D controls movement:

``` text
CharacterDefinition.StartPosition
              │
              │ ConfigureFromDefinition()
              ▼
          Box2D Body
              │
              ▼
      Physics Simulation
```

The current position now belongs to the state of the engine object or
its physics body.

`Definition.StartPosition`, in contrast, remains the configured start
position.

Therefore, the following implementation would be incorrect:

``` csharp
public Vector2 Position => Definition.StartPosition;
```

The current position must instead be read from the actual state of the
physics body.

Applied is necessary in this case because the current position evolves
independently from the original start position.

------------------------------------------------------------------------

## 5. Live vs. Applied

Live is the preferred architecture.

Applied deliberately creates an additional state that is separate from
the `Definition`. Therefore, Applied should not be used when the same
goal can be achieved with Live.

### Not Recommended

If `Zoom` does not require an independent state, the following
implementation is unnecessary:

``` csharp
private float _zoom;

protected override void ConfigureFromDefinition()
{
    base.ConfigureFromDefinition();

    _zoom = Definition.Zoom;
}

public float Zoom => _zoom;
```

This unnecessarily creates two states:

``` text
Definition.Zoom
      │
      ▼
    _zoom
```

### Preferred

Direct access:

``` csharp
camera.Definition.Zoom
```

or optionally as a get-only convenience property:

``` csharp
public float Zoom => Definition.Zoom;
```

### Decision Rule

``` text
                Definition Property
                        │
                        ▼
          Does the engine object need
           its own independent state?
                  │             │
                 No            Yes
                  │             │
                  ▼             ▼
                LIVE         APPLIED
                  │             │
            use directly       │
                                ▼
                    ConfigureFromDefinition()
                                │
                                ▼
                      Engine Object State
```

In short:

> **Live:** The `Definition` is the current data source.
>
> **Applied:** The `Definition` configures an independent state of the
> engine object.

------------------------------------------------------------------------

## 6. ConfigureFromDefinition and the Engine Lifecycle

`ConfigureFromDefinition()` is part of the general lifecycle of an
engine object.

The method may be executed in connection with the following operations:

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
   Engine Object
```

Calling `ConfigureFromDefinition()` does not mean that every property
uses Applied.

### With Live

A Live value normally does not need to be processed inside
`ConfigureFromDefinition()`:

``` csharp
public float Zoom => Definition.Zoom;
```

If `ConfigureFromDefinition()` is nevertheless executed by the engine
lifecycle, no additional action is required for `Zoom`.

### With Applied

Applied values are applied to the engine object's independent state
inside `ConfigureFromDefinition()`:

``` csharp
protected override void ConfigureFromDefinition()
{
    base.ConfigureFromDefinition();

    _body.SetTransform(
        Definition.StartPosition,
        Definition.StartRotation);
}
```

A later `Reload()` or `ReloadAsync()` can therefore apply the current
values from the `Definition` to the engine object again.

------------------------------------------------------------------------

## 7. Editor and Inspector

The distinction between Live and Applied does not need to be stored as
an additional mode or metadata on the engine object.

An editor or inspector may generally call `Reload()` after changing a
`Definition`.

``` text
Inspector
    │
    │ Change Definition
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

The change is already effective immediately through the `Definition`.

The additional reload is not necessary for that Live value, but it does
not create an architectural problem.

### Applied

The reload causes the changed definition value to be applied again to
the engine object's state through `ConfigureFromDefinition()`.

Therefore, the inspector does not need to know whether an individual
property uses Live or Applied.

``` text
Inspector changes Property
          │
          ▼
        Reload
          │
     ┌────┴────┐
     │         │
    Live    Applied
     │         │
     │         ▼
     │    Apply value again
     │
     ▼
No additional
action required
```

------------------------------------------------------------------------

## 8. Comparison with MVVM

The Sasogine principle is not MVVM, but it follows a comparable
architectural idea: data and its use are clearly separated.

Simplified:

  MVVM        Sasogine
  ----------- ----------------------------------------------
  Model       `Definition`
  ViewModel   Engine Object
  View        Presentation / Rendering
  Binding     Use of the `Definition` by the engine object

This comparison is only intended to provide a conceptual reference.
Sasogine does not implement MVVM.

MVVM typically describes a data flow such as:

``` text
Model
  │
  ▼
ViewModel
  │
  ▼
View
```

Sasogine focuses on:

``` text
Definition
    │
    ▼
Engine Object
```

There are exactly two ways to use definition data:

``` text
                    Definition
                        │
              ┌─────────┴─────────┐
              │                   │
              ▼                   ▼
             LIVE               APPLIED
              │                   │
              ▼                   ▼
        Direct Access    ConfigureFromDefinition()
                                  │
                                  ▼
                          Engine Object State
```

The key difference is:

**MVVM** primarily describes the separation and communication between
Model, ViewModel, and View.

**Sasogine** defines the boundary between a `Definition` and an engine
object and specifies how definition data is used by the engine object.

------------------------------------------------------------------------

## 9. Architecture Summary

For a new definition-based state, only a few questions are necessary:

1.  Can the `Definition` itself be the current data source?
2.  If yes: **use Live.**
3.  Does the engine object's state need to evolve independently from the
    `Definition`?
4.  Only then: **use Applied and configure the state through
    `ConfigureFromDefinition()`.**

The fundamental principle is:

> **Prefer Live.**
>
> **Use Applied only when an independent engine object state is actually
> required.**
>
> **With Applied, state is transferred from the `Definition` through
> `ConfigureFromDefinition()`.**
