# VectorPathTool Concept

## Purpose

`VectorPathTool` is the vector-editing tool of the Sasogine Toolkit. Its purpose is to provide a clear separation between object selection, vector-path editing, node selection, object transforms, direct geometry editing, persistent definition data, and runtime/editor state.

The tool is designed to work together with the existing Selection Toolkit without coupling vector editing directly to selection transforms.

## Design Goals

- Keep `VectorPathTool` independent from concrete game objects.
- Allow compatible objects to expose editable vector geometry through interfaces.
- Keep object selection and node selection separate.
- Support multiple vector targets.
- Allow selected targets to expose editable nodes.
- Keep object-level transforms independent from direct node edits.
- Allow definitions to represent persistent transform state.
- Avoid destructive geometry transforms unless explicitly intended.
- Reuse the common Sasogine tool lifecycle.

## Target Model

`VectorPathTool` does not know terrain, scenery, facilities, or other concrete game objects directly. Instead, an object participates in vector editing by exposing a `VectorShape` through `IVectorPathTarget` or `IVectorPathTargetDefinition`.

```text
Editor Object
    ↓
IVectorPathTarget
    ↓
ActiveShape
    ↓
VectorShape
    ↓
VectorPath
    ↓
VectorNode / IVectorSegment
```

A runtime/editor target can expose vector geometry through:

```csharp
public interface IVectorPathTarget : ISelectionTarget
{
    VectorShape? ActiveShape { get; set; }
}
```

A definition-side target follows the same concept:

```csharp
public interface IVectorPathTargetDefinition :
    IDefinition,
    ISelectionTargetDefinition
{
    VectorShape? ActiveShape { get; set; }
}
```

`ActiveShape == null` means no vector geometry is currently exposed to the tool. `ActiveShape != null` means the target has vector geometry available. A shape with zero paths exists but contains no editable path data.

## Shape Ownership

Every vector target owns its own `VectorShape` instance. Sharing the same `VectorShape` between multiple targets is invalid because the shape also contains editor state such as selected nodes.

```text
Target A → Shape A
Target B → Shape B
Target C → Shape C
```

Invalid:

```text
Target A ─┐
          ├→ Shape A
Target B ─┘
```

If shared instances are detected, the editor should treat this as an invalid configuration instead of silently deduplicating them.

## Object Selection vs. Node Selection

The editor has two independent selection levels.

Object selection belongs to the Selection Toolkit. `ISelectionTarget.IsSelected` or `ISelectionTargetDefinition.IsSelected` determines whether the object itself is selected. For `VectorPathTool`, object selection determines whether the target's vector geometry is actively editable.

Node selection belongs to the vector editor. `VectorNode.IsSelected` determines whether an individual path or control node is selected.

```text
Target selected
    → VectorPathTool may edit its ActiveShape
    → zero, one, or many vector nodes may be selected

Target not selected
    → vector nodes are not actively edited
    → target may still be visually framed
```

## VectorShape

A `VectorShape` is the logical container for one or more vector paths.

```text
VectorShape
├── VectorPath
├── VectorPath
└── VectorPath
```

The shape is the correct level for operations such as total geometry bounds, mesh input generation, target visualization, and complete vector-geometry serialization.

## VectorPath

A `VectorPath` describes one continuous path.

```text
Start
  ↓
Segment 0
  ↓
Segment 1
  ↓
Segment 2
```

Each segment owns its end node. The path owns its start node. A path may be open or closed through `IsClosed`.

For a closed path, the final segment endpoint is logically connected back to `Start`. Closure is a property of the path and does not require storing the start point twice.

## Vector Nodes

`VectorNode.Position` represents an actual geometry position. It is not an object transform and not a temporary visual offset.

Changing a node directly changes the vector geometry:

```text
Node.Position = (100, 100)
        ↓ edit
Node.Position = (120, 100)
```

This direct geometry edit must not automatically modify the object's transform state.

## Target Visualization

Every object that exposes a valid vector shape may be visually framed by `VectorPathTool`.

```text
IVectorPathTarget
ActiveShape != null
ActiveShape.Paths.Count > 0
    ↓
show target border
```

Selection changes only the appearance:

```text
Target not selected → TargetBorderColor
Target selected     → TargetSelectionColor
```

The target border is separate from node-selection indicators.

## Shape Bounds and Padding

Bounds must first be calculated from the complete vector geometry. Only after true geometry bounds are known may editor-specific padding be added.

```text
VectorShape
    ↓
calculate actual geometry bounds
    ↓
Bounds
    ↓
apply TargetBorderPadding
    ↓
visual target bounds
```

Geometry-level bounds belong to `VectorShape`; editor padding belongs to `VectorPathTool`.

Example:

```text
Geometry bounds:
X = 100
Y = 100
Width = 100
Height = 100

TargetBorderPadding = 4

Displayed bounds:
X = 96
Y = 96
Width = 108
Height = 108
```

Width and height therefore use `Padding * 2`.

## Sampling Rules

Segment sampling must produce one continuous path without duplicated shared endpoints.

```text
A → B → C → D
```

should become:

```text
A, B, C, D
```

not:

```text
A, B, B, C, C, D
```

The authoritative endpoint of a segment is always `segment.Node.Position`. If a segment temporarily cannot produce samples, the logical current path position must still advance to that node so later segments start from the correct position.

## Tool Lifecycle

`VectorPathTool` participates in the standard Sasogine tool lifecycle.

```text
ToolComponentBase
    ↓
ToolBase
    ↓
VectorPathTool
```

Update flow:

```text
ToolComponentBase
    ↓
reset interactions
    ↓
ApplyInteractions(...)
    ↓
create ToolContext
    ↓
tool.ApplyContext(...)
    ↓
tool.Update(...)
```

Draw flow:

```text
ToolComponentBase.Draw(...)
    ↓
tool.Draw(...)
```

## ToolContext and Interactions

`ToolContext` provides the state required by the current tool update:

```text
CursorState
Camera
Interactions
```

`VectorPathTool.ApplyContext()` converts the cursor to world coordinates and updates the current tool state.

```text
screen cursor
    ↓ Camera
world position
    ↓
_cursorPosition
    ↓ grid snapping
_snappedCursorPosition
```

`VectorPathTool` currently needs only standard `ToolInteractions`, so a specialized `VectorPathToolInteractions` type is unnecessary.

The current interaction instance may be stored from the context:

```csharp
_interactions = context.Interactions;
```

The update logic then uses this current reference.

`ToolBase.CreateInteractions()` remains important as an extension point for tools that truly need custom interaction types, such as camera tools.

## Modes

`VectorPathTool` currently has three primary modes:

```text
Selection
Draw
Insert
```

### Selection Mode

Selection mode edits existing vector nodes. It supports single selection, modifier-based multiple selection, control-node selection, node dragging, moving multiple selected nodes together, area selection, and cancellation.

The outer target is selected by the Selection Toolkit. `VectorPathTool` only selects nodes inside selected vector targets.

### Area Selection

Pressing on empty vector space starts an area-selection rectangle.

```text
press on empty space
    ↓
store start
    ↓
drag
    ↓
update end
    ↓
release
    ↓
select nodes inside bounds
```

Without the modifier interaction, the previous node selection is replaced. With the modifier interaction, the existing selection can be extended or toggled.

### Node Movement

Moving selected nodes modifies vector geometry directly.

```text
drag start
    ↓
store original selected-node positions
    ↓
cursor delta
    ↓
original position + delta
```

This differs from moving the complete object through an object transform.

### Draw Mode

Draw mode creates a new `VectorPath` interactively.

```text
first click
    ↓
create temporary path
    ↓
create preview segment
    ↓
next click
    ↓
commit segment
    ↓
continue
```

A path may be completed by closing it at its own start node, ending it at the current endpoint, or connecting it to another compatible path endpoint.

`SegmentFactory` determines which segment implementation is created.

### Insert Mode

Insert mode creates predefined paths using `PathFactory`.

```text
drag rectangle
    ↓
Bounds2
    ↓
PathFactory(bounds)
    ↓
VectorPath
    ↓
ActiveShape.Paths.Add(path)
```

This allows predefined primitives such as rectangles, ellipses, or game-specific terrain shapes.

## Primary Shape

Some operations require a single target shape, even when multiple vector targets are selected. Examples include drawing a new path, inserting a path, adding a segment, or replacing a segment.

Multiple selected shapes may participate in node selection, but creation operations use one primary shape. This avoids unintentionally inserting the same geometry into multiple targets.

## Transform Architecture

Object transforms and vector-node editing must remain independent.

```text
Object transform
    → changes the object as a whole

Vector node edit
    → changes local vector geometry
```

These operations may affect the same visible result, but they represent different intentions and must not overwrite one another.

## Definition Transform

A definition may contain persistent transform values, for example:

```csharp
public interface ITransformMovable2Definition
{
    Point2 Position { get; set; }
}
```

These values represent saved object state.

During configuration:

```text
Definition.Position
    ↓
ConfigureFromDefinition()
    ↓
runtime/editor geometry is configured
```

A definition update may therefore cause geometry to be transformed or rebuilt.

## Runtime Transform

The runtime object may expose the corresponding transform capability without the `Definition` suffix:

```csharp
public interface ITransformMovable2
{
    Point2 Position { get; set; }
}
```

The concrete implementation may privately track whether the transform changed:

```text
_position
_positionDirty
```

Dirty state is an implementation detail and does not belong in the public interface.

When object position changes:

```text
Position changes
    ↓
_positionDirty = true
    ↓
whole runtime geometry may be transformed
```

When a vector node changes directly:

```text
Node.Position changes
    ↓
vector geometry changes
    ↓
_positionDirty remains unchanged
```

This separation is intentional.

## Example Transform Flow

Saved definition:

```text
Definition.Position = (10, 10)
```

Configuration:

```text
ConfigureFromDefinition()
    ↓
apply definition transform to geometry
```

Later the Selection Toolkit moves the complete object:

```text
Runtime Position = (20, 20)
    ↓
runtime transform becomes dirty
    ↓
whole vector geometry moves
```

Later `VectorPathTool` moves one node:

```text
Node.Position += (5, 0)
```

Result:

```text
node geometry changed
object Position unchanged
transform dirty state unchanged
```

This prevents Selection Tool transforms and Vector Path editing from fighting each other.

## Relationship to SelectionTool

Responsibilities are intentionally separated:

```text
SelectionTool
    → select editor objects
    → perform supported object-level transforms

VectorPathTool
    → edit vector geometry inside selected vector objects
    → select and move vector nodes
```

`IVectorPathTarget : ISelectionTarget` means a vector-editable object is also selectable. It does not mean vector nodes themselves are selection targets.

## Events

`VectorPathTool` exposes editing events such as:

```text
NodeSelected
NodeMoved
SegmentAdded
SegmentRemoved
PathRemoved
PathConnectionChanged
```

These allow the surrounding editor to implement undo/redo, dirty-document tracking, command updates, inspector refreshes, and application-specific synchronization without coupling those concerns into the generic tool.

## VectorPathEditorService

A game/editor-specific service may hold the current `VectorPathTool` and the current shape-creation mode.

```text
VectorPathEditorService
├── Tool
└── SelectedShape
```

This belongs to the application/editor layer, not the generic Toolkit. It can configure `PathFactory`, current primitive type, and command/UI state.

## Rendering Layers

The vector editor may render several logically separate layers:

```text
Target Bounds
Vector Lines
Nodes
Control Nodes
Sample Vertices
Area Selection
Insertion Preview
Drawing Preview
```

These layers should remain conceptually independent even if some share batching infrastructure.

Target selection color and normal target border color must not accidentally depend on shared mutable shader state within one batch. Separate render passes may be used where needed.

## Coordinate Semantics

Vector geometry positions represent exact geometry coordinates.

```text
Node.Position
    → exact vector point

PointSize
    → editor marker size only
```

Therefore path lines, sampled vertices, and geometry bounds use exact node positions. Visual node markers are centered around those positions. Marker size must never shift actual vector geometry.

## Locked Paths

`VectorPath.IsLocked` blocks geometry modifications but does not block selection.

```text
IsLocked == true
    → selection and area selection remain available
    → node/control-node dragging is blocked
    → add/remove/replace segment operations are blocked
    → remove path is blocked
    → open/close path is blocked
    → connecting to the path is blocked
```

This keeps locked geometry visible and selectable while preventing accidental edits.

## Path Connection

Draw mode may connect a newly drawn path to an existing path endpoint. Only endpoints are valid connection targets. Internal nodes do not implicitly split or merge paths.

Closed paths may optionally be excluded from connection through `AllowConnectToClosedPath`.

## Factories

`SegmentFactory` determines which segment type is created while drawing or adding segments. Possible implementations include line, Bézier, spline, or arc segments.

`PathFactory` creates predefined paths during Insert mode and keeps the generic tool independent from game-specific primitives.

## Persistence

Persistent vector data and temporary editor state must remain distinct.

Potentially transient state includes:

```text
VectorNode.IsSelected
temporary drawing path
temporary drawing segment
area-selection rectangle
moving-node cache
current cursor position
```

Persistent game data should not depend on temporary tool state.

## Responsibility Summary

### VectorPathTool

Responsible for vector editing, vector node selection, area selection, node movement, path drawing, path insertion, segment/path operations, target visualization, and interaction handling.

Not responsible for game persistence, general object-selection ownership, commands, undo/redo storage, or game-specific terrain semantics.

### SelectionTool

Responsible for selecting editor objects and performing supported object-level transforms. It does not edit internal vector nodes.

### VectorShape

Responsible for owning paths and geometry-level operations such as total bounds. It does not own editor padding or target colors.

### VectorPath

Responsible for ordered topology, segment ownership, closure, and path sampling.

### VectorNode

Responsible for vector-point position and node-local selection state. It does not represent an object transform.

## Architectural Principle

The central rule is:

> Object transforms modify the object. Vector editing modifies the geometry.

These operations may visually affect the same result, but they represent different intentions and therefore remain separate in the data model.

This separation allows `SelectionTool` and `VectorPathTool` to coexist without one tool unintentionally rewriting state owned by the other.
