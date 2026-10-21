# Workspace DPI and automatic resizing

`Workspace.Dpi` is the physical pixel density used by the UI. It defaults to **96 DPI** (100%). `DpiScale` returns `Dpi / 96`; 144 DPI means 150%, 192 DPI means 200%. The host may assign the platform's reported DPI or its own UI setting. Resizing a window does not change this value.

```csharp
workspace.Dpi = 144;

// Update: sample physical device states once and pass the current physical viewport.
workspace.Update(inputSampler, mouse, keyboard, touches,
    graphicsDevice.Viewport.Bounds, elapsed, layout, isActive);

// Draw: viewport and scale are synchronized from the processed frame automatically.
workspace.Draw(drawContext);
```

The logical layout size is physical viewport size divided by DpiScale. A resized viewport invalidates layout; unchanged frames keep cached measurements. Fixed logical sizes stay fixed during window resizing. Star columns, wrapping, scrolling and popups use the newly available logical area. DPI changes raise cached PropertyChanged notifications and apply on the next processed frame. Input and drawing use the same captured DPI even when a UI callback changes Dpi during a frame.

Frames produced by ControlInputSampler.Sample and SampleReusable retain PhysicalViewport and SourceScale. Existing workspace.Update(sampledFrame, layout) calls therefore also use the workspace DPI and automatically convert recorded pointer coordinates exactly once. The recorded frame itself is not changed. Use Workspace.Dpi as the scale authority rather than calculating a scale from window dimensions.

Explicitly constructed ControlInputFrame values without viewport metadata remain the advanced logical-coordinate API. Their sizes/events are already logical; their draw context must still match those dimensions and its explicit scale. This preserves custom render passes and deterministic logical replays. The application owns fonts and render targets; DPI does not recreate the logical font or load resources each frame.

The four UI examples use the physical-viewport update path. LogoInspector supports --dpi 144 and --verify-resize; the latter checks actual window sizes and rendering at fixed and changed DPI. Zero-sized minimized viewports are skipped by the examples before updating or drawing.
