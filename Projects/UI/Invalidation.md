# Control invalidation

Controls expose separate invalidation paths. Call them on the workspace thread outside drawing; the next `Workspace.Update` processes scheduled layout work.

| Method | Effect |
|---|---|
| `InvalidateVisual()` | Marks the control and its ancestors as visually changed; preserves measurement and arrangement caches. |
| `InvalidateArrange()` | Invalidates arrangement for the affected ancestor chain, preserves measured sizes and marks visuals as changed. |
| `InvalidateLayout()` | Public, virtual entry point for full measurement and arrangement invalidation. |
| `InvalidateMeasure()` | Existing protected/internal framework entry point for full measurement and arrangement invalidation. |

```csharp
control.InvalidateVisual();  // Custom drawing data changed.
control.InvalidateArrange(); // Custom placement data changed; desired size is unchanged.
control.InvalidateLayout();  // Desired size or an external measurement resource changed.
```

Standard properties select the appropriate path automatically. Colors, opacity, hover/focus, activation state and interaction availability use visual invalidation. Control, content and text alignment use arrangement invalidation. Width, height, padding, margins, visibility and other measurement-dependent properties retain full layout invalidation. Z-index updates the parent's drawing order and invalidates visuals.

Visual changes propagate to ancestors for renderer cache tracking, without invalidating unrelated siblings. The library records an internal visual revision instead of clearing a dirty flag during Draw. Actual bounds changes also update the revision. This does not introduce bitmap compositing or skip the existing per-frame drawing pass.

Handled input invalidates the receiving control's visuals. Built-in caret blink transitions, spinner animation, chart series changes and optimized text/content notification paths also report visual changes. Custom controls should call `InvalidateVisual()` when private animation or drawing data changes without a property notification.

Arrange-only changes preserve measure caches and retain validation of alignment values. A failed arrangement stays invalid so the next successful update can retry it. Changes in constraints or font resources may still require measurement independently of an explicit arrange request.

For external font changes, use the existing font invalidation API or the affected text control's `InvalidateLayout()` override so its private text metrics are refreshed as well.
