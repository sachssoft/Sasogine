# Workspace

`Root` has been renamed to `Workspace` in `Sachssoft.Engine.Controls`.

```csharp
_workspace = new Workspace(content);
_workspace.SetSkin(_skin);

// Update before drawing, using the same layout and drawing scale.
_workspace.Update(input, layout);
_workspace.Draw(drawContext);
```

Migration:

- Replace the `Root` type with `Workspace` and rename `Root.cs` to `Workspace.cs`.
- Replace `control.Root` with `control.Workspace`.
- Replace `inputContext.Root` with `inputContext.Workspace`.
- Example views expose `Workspace`; hosts use `_workspace`.
- Registry, styles, bindings, focus, dialogs, input routing, disposal and layout caching keep their existing behavior.

This is an API rename without a compatibility alias. When updating from a source ZIP, remove the old `Projects/UI/Root.cs` before copying in the new sources. Rebuild applications using the Controls library. Existing README files remain unchanged and may still show the previous API name; use this migration note for those snippets.
