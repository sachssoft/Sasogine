# VectorPathTool fixes

- Removed `ShowFill`, fill batch/shader/color, and unused fill rendering.
- Removed insert debug quad and `Console.WriteLine` diagnostics.
- Removed `VectorPathTool.bak.cs` and obsolete `VectorPathToolInteractions.cs`.
- `VectorPath.IsLocked` now blocks editing operations while leaving selection available.
- Locked selected nodes/control nodes cannot start dragging; mixed selections no longer move when drag starts from a locked node.
- Locked paths cannot be removed, changed open/closed, have segments added/removed/replaced, or be used as connection targets.
- Added `VectorPath.Reverse()` for path-connection direction handling.
- Cubic Bézier controls, B-spline/Catmull-Rom control order, and elliptical-arc sweep are reversed correctly.
- Fixed drawing-path connection to the end of an existing path by reversing the temporary path before merging.
- When `AllowConnectToClosedPath` is enabled, the target path is opened at its existing seam before connection.
- Added several index/null guards and preserved endpoint selection when replacing a segment.

## Transform synchronization

- Completed `VectorShape.ApplyTransform()` using an old-to-new delta matrix.
- Position, size, scale, rotation, rotation pivot, and skew capabilities are included when available.
- Path anchor nodes and all segment control nodes are transformed exactly once.
- `VectorShape.ApplyTransform()` is public so application code controls when transform-source changes are synchronized.
- Removed automatic transform synchronization from `VectorPathTool.Update()`; SelectionTool and VectorPathTool remain independent.
- Added `VectorShape.IsChanged`, `Changed`, `NotifyChanged()`, and `UpdateState()` for geometry change tracking.
- The previous transform state is updated only after geometry synchronization.
## Parent hierarchy update
- Replaced `IVectorSegment` with abstract `VectorSegment`.
- Added `VectorSegment.Path` parent reference.
- Added `VectorNode.Segment` parent reference for segment endpoint and control nodes.
- Added `VectorSegmentCollection` to manage `VectorPath.Segments` ownership automatically.
- Added `VectorNodeCollection` for variable control-node ownership.
* [Change] Replaced the shared `ControlNodes` property on `VectorSegment` with `GetControlNodes()`, while `VectorVariableSegment` exposes its modifiable `ControlNodes` collection directly.
