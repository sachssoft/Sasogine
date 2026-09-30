# Sasogine Coding Style

This document defines the coding, formatting, and naming rules used by
Sasogine.

Sasogine generally follows established .NET and C# conventions.
Project-specific rules extend these conventions where the architecture,
serialization model, or public API requires clearer semantics.

## 1. Naming Rules

### .NET Conventions

Names should follow established .NET naming conventions wherever
applicable.

Use `PascalCase` for:

-   Classes
-   Structs
-   Interfaces
-   Enums
-   Delegates
-   Records
-   Methods
-   Properties
-   Events
-   Public members

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

Interfaces must use the `I` prefix.

``` csharp
public interface IRenderable
{
}

public interface IDefinition
{
}
```

### Parameters and Local Variables

Parameters and local variables use `camelCase`.

``` csharp
public void SetPosition(float positionX, float positionY)
{
    float previousPosition = positionX;
}
```

### Private Fields

Private instance fields use `_camelCase`.

``` csharp
private bool _isInitialized;
private AssetManager _assetManager;
```

### Constants

For non-public constants declared as `private` or `internal`,
`UPPER_CASE` is allowed and recommended.

Separate multiple words with underscores.

``` csharp
private const int MAXIMUM_COUNT = 100;
internal const string DEFAULT_NAME = "Sasogine";
```

For constants that are part of the public API, established .NET naming
conventions should be used.

``` csharp
public const int MaximumCount = 100;
```

This keeps the public API consistent with the .NET ecosystem while
making internal constants visually distinct.

### Boolean Members

Boolean members should clearly describe a state or capability.

Preferred:

``` csharp
IsVisible
IsEnabled
IsInitialized
HasFocus
CanRender
SupportsInput
```

Avoid unclear names or names that unnecessarily differ from established
.NET conventions.

### Abbreviations

Treat abbreviations as normal words where practical.

Preferred:

``` csharp
XmlReader
HttpClient
UiElement
```

Use alternative capitalization only where required by an established
technical name or existing .NET API.

------------------------------------------------------------------------

## 2. Scope and Control Flow Rules

### Braces and Short `if` Statements

For a simple `if` containing exactly one short statement, braces may be omitted. This compact form is explicitly allowed and encouraged in Sasogine.

```csharp
if (isVisible)
    Render();
```

Braces are also valid:

```csharp
if (isVisible)
{
    Render();
}
```

When `if` and `else` are used together, braces should be preferred.

Not recommended:

```csharp
if (isVisible)
    Render();
else
    Test();
```

Preferred:

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

Braces are required when a branch contains multiple statements.

`else` is fully allowed and is not discouraged in favor of early returns. Use whichever control-flow structure is clearer for the specific case.

### Prefer Simple `if`

When a condition can be expressed clearly without `else`, prefer a
simple `if`.

``` csharp
if (!isInitialized)
{
    Initialize();
}
```

### Boolean Conditions

Boolean values should be used directly as conditions.

Preferred:

```csharp
if (isLoaded)
    Load();

if (!isLoaded)
    Load();
```

Explicit comparisons with `true` or `false` must not be used.

Do not use:

```csharp
if (isLoaded == true)
    Load();

if (isLoaded == false)
    Load();
```

Assignments inside Boolean conditions are also not allowed.

Do not use:

```csharp
if (isLoaded = true)
    Load();

if (isLoaded = false)
    Load();
```

The Boolean member name itself should make the condition understandable.

### Keep Scope Small

Declare variables as close as practical to where they are used.

Members should have no broader accessibility than required.

Prefer the smallest appropriate accessibility:

`private` → `protected` / `internal` → `public`

Do not make members `public` merely for convenience. The Sasogine public
API should be designed deliberately.

------------------------------------------------------------------------

## 3. Formatting Rules

Sasogine uses a compact but readable formatting style.

Avoid unnecessary empty lines and excessive vertical formatting.

### Method Declarations

Short method declarations may remain compact.

``` csharp
public void Move(float x, float y)
{
}
```

For multiple or complex parameters, placing each parameter on its own
line is recommended.

``` csharp
public void CreateAsset(
    AssetDefinition definition,
    AssetContext context,
    bool initializeImmediately)
{
}
```

This is particularly recommended for:

-   Constructors with multiple dependencies
-   Factory methods
-   Long parameter types
-   Generic methods
-   Delegates
-   Parameters with attributes or modifiers

### Method Calls

Apply the same principle to method calls.

Keep short calls compact:

``` csharp
Move(x, y);
```

Split longer calls when this improves readability:

``` csharp
CreateAsset(
    definition,
    context,
    initializeImmediately);
```

### Generic Types

Long generic type declarations may be split across lines when necessary
for readability.

``` csharp
DefinitionBindingCollection<
    CourseEntityDefinitionBase,
    ICourseEntity,
    CourseEntityContext>
```

Do not split short and readable declarations unnecessarily.

### Empty Lines

Use empty lines to separate logical sections.

Keep related statements visually grouped and avoid unnecessary empty
lines between individual statements.

------------------------------------------------------------------------


## 4. Member Order in Classes and Structs

Sasogine uses a fixed member order for `class` and `struct` declarations.

This order is mandatory and should be followed regardless of the size of the type.

1. Constants (`const`)
2. Readonly fields (`readonly`) – static first, then instance fields
3. Fields
4. Events
5. Constructors
6. Properties
7. Instance members
8. Static members

Example:

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

### Constants

Constants always appear first.

```csharp
private const int MAXIMUM_COUNT = 100;
private const string DEFAULT_NAME = "Sasogine";
```

### Readonly Fields

`readonly` fields follow constants.

Static `readonly` fields appear before instance `readonly` fields.

```csharp
private static readonly object _staticLock = new();

private readonly object _instanceLock = new();
```

### Fields

Regular fields follow the `readonly` fields.

```csharp
private int _count;
private bool _isInitialized;
```

### Events

Events appear after fields and before constructors.

```csharp
public event EventHandler? Changed;
```

### Constructors

Constructors appear after events and before properties.

```csharp
public Example()
{
}
```

### Properties

Properties appear after constructors.

```csharp
public int Count => _count;

public bool IsInitialized => _isInitialized;
```

### Instance Members

Non-static methods and other instance members follow properties.

```csharp
public void Initialize()
{
}

public void Update()
{
}
```

### Static Members

Static methods and other static members appear at the end of the type.

```csharp
public static Example Create()
{
    return new Example();
}
```

Members should not be grouped primarily by `public`, `protected`, `internal`, or `private`. Their member kind and architectural role determine their position.

---

## 5. Sasogine-Specific Naming Rules

Sasogine defines additional rules for architectural types.

These rules make the architectural role of a type recognizable from its
name.

### `Definition` Suffix

The `Definition` suffix is mandatory when a type represents a Sasogine
definition.

This applies especially to types that:

-   directly implement `IDefinition`,
-   inherit from an `IDefinition` type,
-   are used as definitions within the Sasogine architecture,
-   or represent serializable definitions or persistent object
    descriptions.

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

Avoid:

``` csharp
public class Texture : IDefinition
{
}
```

The `Definition` suffix represents a concrete architectural role and
must not be omitted merely to shorten a name.

### Serialization and the `Serialization` Suffix

Serialization types must be distinguished from definition types.

A type whose primary responsibility is serializing or deserializing a definition or another Sasogine type must use the `Serialization` suffix.

Example:

```csharp
public class TreeDefinition : IDefinition
{
}

public class TreeSerialization : SerializationBase<TreeDefinition>
{
}
```

`TreeDefinition` describes the object definition.

`TreeSerialization` represents the serialization logic responsible for that definition.

The `Definition` suffix must therefore not be used as a generic suffix for serialization types.

Preferred:

```csharp
TextureDefinition
TextureSerialization

TreeDefinition
TreeSerialization
```

Avoid:

```csharp
TreeDefinitionSerializationDefinition
TreeSerializer
```

when the type explicitly fulfills the architectural role of a Sasogine `Serialization` type.

`Definition` remains mandatory for definition types. `Serialization` is mandatory for corresponding serialization types.

### `Base` Suffix

The `Base` suffix is recommended for abstract classes explicitly
designed as inheritance bases.

``` csharp
public abstract class ShapeDefinitionBase
{
}

public abstract class ComponentBase
{
}
```

However, `Base` is not mandatory.

An abstract class may omit the suffix when its existing name is already
clear or `Base` would add no useful semantic information.

``` csharp
public abstract class Renderer
{
}
```

Do not mechanically append `Base` to every abstract class.

### Architectural Suffixes

Established Sasogine suffixes should be used consistently when a type
fulfills the corresponding architectural role.

Common suffixes include:

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

`Definition` identifies definitions and serializable descriptions and is
mandatory in the cases described above.

`Context` identifies a clearly scoped runtime or processing context.

`Registry` identifies a central registration or mapping mechanism.

`Factory` identifies a type whose primary responsibility is creating
other objects.

`Builder` identifies a type used to construct complex objects or
configurations step by step.

`Provider` identifies a type that provides a particular service or data.

`Collection` identifies a specialized collection type.

`Attribute`, `Exception`, and `EventArgs` follow the corresponding .NET
conventions.

`Base` may identify an abstract inheritance base but is not mandatory.

### Avoid Artificial Names

Do not invent alternative terminology when an established architectural
name already exists.

Preferred:

``` csharp
DefinitionRegistry
AssetFactory
RenderContext
DefinitionBindingCollection
```

Avoid vague names such as:

``` csharp
DefinitionStuff
AssetHelper
RenderData
CommonUtil
```

In particular, `Helper`, `Util`, `Manager`, and `Data` should not be
used as universal catch-all terms.

------------------------------------------------------------------------

## 6. Namespace Rules

Types should be placed in an existing namespace whenever that namespace appropriately represents their functional area.

The namespace structure should remain semantically clear without becoming unnecessarily fragmented.

Preferred approach:

1. First determine whether an existing namespace appropriately fits the type.
2. If multiple namespaces are possible, choose the most semantically appropriate and stable area.
3. Introduce a new namespace only when no existing namespace can reasonably contain the type.
4. Create new namespaces deliberately and exceptionally to avoid an excessive number of very small namespaces.

A new namespace should not be introduced merely because a few types could theoretically form a more specific category.

The namespace structure should support the architecture rather than mirror every implementation detail.

---

## 7. API Naming

The public Sasogine API should be understandable from its naming
wherever possible.

A user should be able to determine:

-   what a type represents,
-   which architectural role it has,
-   whether it is a definition or runtime object,
-   whether it acts as a registry, factory, context, or collection.

Names should describe semantic responsibility rather than incidental
implementation details.

Preferred:

``` csharp
AssetRegistry
DefinitionBindingCollection
RenderContext
ShapeDefinition
```

instead of:

``` csharp
AssetData
BindingHelper
RenderStuff
ShapeInfo
```

------------------------------------------------------------------------

## General Principle

Established .NET conventions form the foundation of the Sasogine coding
style.

Sasogine-specific rules extend those conventions where architecture or
serialization requires additional semantic clarity.

In particular:

-   Follow established .NET naming conventions.
-   Interfaces use the `I` prefix.
-   Private fields use `_camelCase`.
-   Non-public `const` members may and preferably should use
    `UPPER_CASE`.
-   Public constants follow .NET naming conventions.
-   Control-flow statements use braces.
-   Avoid unnecessary `else` blocks.
-   Keep scopes as small as practical.
-   Keep formatting compact but readable.
-   Split longer parameter lists across lines where appropriate.
-   `Definition` is mandatory for corresponding definition types and
    serializable definitions.
-   Serializable definitions must be clearly distinguishable from
    runtime objects.
-   `Base` is recommended for abstract inheritance bases but is not
    mandatory.
-   Keep the public API consistent and understandable from its names.
