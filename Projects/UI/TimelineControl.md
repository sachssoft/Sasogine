# TimelineControl

`TimelineControl` edits timed blocks on rows sharing a time ruler. It does not execute animation definitions: use `TimelineItem.Tag` to associate application data and `TimeChanged` to evaluate a preview.

```csharp
var timeline = new TimelineControl
{
    Duration = 12,
    PixelsPerSecond = 100,
    SnapInterval = 0.1,
    Height = 320
};

var position = new TimelineTrack { Title = "Position" };
position.Items.Add(new TimelineItem { Title = "Move right", Start = 0, Duration = 2 });
position.Items.Add(new TimelineItem { Title = "Return", Start = 4, Duration = 2 });

var rotation = new TimelineTrack { Title = "Rotation" };
rotation.Items.Add(new TimelineItem { Title = "Rotate", Start = 1, Duration = 3 });

timeline.Tracks.Add(position);
timeline.Tracks.Add(rotation);

timeline.TimeChanged += (_, _) => PreviewAnimation(timeline.Time);
timeline.EditCompleted += (_, edit) => SaveAnimationRange(
    edit.Item.Tag, edit.OriginalStart, edit.OriginalDuration, edit.Start, edit.Duration);
```

`PreviewAnimation` and `SaveAnimationRange` are application callbacks. A completed edit supplies an immutable snapshot suitable for one caller-managed undo action. An optional `Command` receives this snapshot, unless `CommandParameter` supplies a custom parameter. There is no automatic history stack.

## Interaction

- Click a block to select it. Drag its body to move it within its row.
- Drag a visible start/end edge to resize. Very narrow blocks can be moved and become resizable after zooming in.
- Hold Shift during a block gesture to bypass time snapping.
- Drag the ruler to seek. `Time` is also directly assignable.
- Ctrl+wheel or +/- zoom around the cursor or viewport center. `ZoomToFit()` fits the configured duration.
- Wheel scrolls rows; Shift+wheel scrolls time. Middle-button or Alt+left-button dragging pans both axes.
- Touch dragging on empty timeline space pans. Touch dragging a block edits it; touching the ruler seeks.
- Left/right arrows move the selected editable block by the snap interval, or move the playhead when no editable block is selected. Shift multiplies the increment by ten.
- Up/down arrows scroll rows. Home/End seek to the start/end. Space toggles preview playback.
- Escape, pointer cancellation, detachment, and loss of availability cancel a gesture. Cancellation restores the gesture's own range changes, while preserving external range replacements and removed models.

`IsPlaying`, `PlaybackRate`, and `Loop` advance only the preview playhead using the elapsed time supplied to `Workspace.Update`. The control does not own an animation controller.

## Data and styling

Rows and blocks provide `INotifyPropertyChanged`. `Tracks` uses Sasogine's `TrackableCollection`; each track observes its block collection. A block can update both range values atomically with `SetRange(start, duration)`.

The editing range is `[0, Duration]`. Existing blocks outside the range are retained and clipped; increase `Duration` to edit them. Overlaps are allowed. Later blocks in a row draw above earlier blocks. `IsReadOnly`, `TimelineTrack.IsLocked`, and `TimelineItem.IsLocked` prevent interactive range changes.

The control registers automatically with `UIRegistry`. It uses the existing horizontal and vertical `ScrollBar` controls. `TrackHeight`, `RulerHeight`, `TrackHeaderWidth`, `ResizeHandleWidth`, `ItemBackground`, `SelectedItemBackground`, `GridColor`, and `PlayheadColor` support local values and central string-keyed styles. For example:

```csharp
var style = new Style { BasedOn = nameof(TimelineControl) };
style[nameof(TimelineControl.TrackHeight)] = "40";
style[nameof(TimelineControl.PlayheadColor)] = "#FFB737";
```

Visible row/block geometry and ruler labels are rebuilt outside Draw when their inputs change. Model subscriptions are released on detachment and restored on reattachment. No reflection, runtime activation, or animation-library dependency is required.
