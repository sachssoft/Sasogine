using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Collections;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace Sachssoft.Engine.Controls;

/// <summary>Edits timed blocks on multiple rows with a shared ruler, playhead, zoom, and independent scrollbars.</summary>
/// <remarks>
/// Models remain caller-owned. Overlapping blocks are allowed; moving a block preserves its row.
/// Block geometry and ruler labels are cached outside Draw. Editing events expose before/after
/// ranges for caller-managed undo history; the control does not evaluate animation definitions.
/// </remarks>
public sealed class TimelineControl : ContainerControl
{
    /// <summary>Identifies changes to <see cref="Command"/>.</summary>
    private static readonly PropertyChangedEventArgs CommandArgs = new(nameof(Command));

    /// <summary>Identifies changes to <see cref="CommandParameter"/>.</summary>
    private static readonly PropertyChangedEventArgs CommandParameterArgs = new(nameof(CommandParameter));

    /// <summary>Identifies changes to <see cref="Duration"/>.</summary>
    private static readonly PropertyChangedEventArgs DurationArgs = new(nameof(Duration));

    /// <summary>Identifies changes to <see cref="GridColor"/>.</summary>
    private static readonly PropertyChangedEventArgs GridColorArgs = new(nameof(GridColor));

    /// <summary>Identifies changes to <see cref="IsEditing"/>.</summary>
    private static readonly PropertyChangedEventArgs IsEditingArgs = new(nameof(IsEditing));

    /// <summary>Identifies changes to <see cref="IsPlaying"/>.</summary>
    private static readonly PropertyChangedEventArgs IsPlayingArgs = new(nameof(IsPlaying));

    /// <summary>Identifies changes to <see cref="IsReadOnly"/>.</summary>
    private static readonly PropertyChangedEventArgs IsReadOnlyArgs = new(nameof(IsReadOnly));

    /// <summary>Identifies changes to <see cref="ItemBackground"/>.</summary>
    private static readonly PropertyChangedEventArgs ItemBackgroundArgs = new(nameof(ItemBackground));

    /// <summary>Identifies changes to <see cref="Loop"/>.</summary>
    private static readonly PropertyChangedEventArgs LoopArgs = new(nameof(Loop));

    /// <summary>Identifies changes to <see cref="MinimumItemDuration"/>.</summary>
    private static readonly PropertyChangedEventArgs MinimumItemDurationArgs = new(nameof(MinimumItemDuration));

    /// <summary>Identifies changes to <see cref="PixelsPerSecond"/>.</summary>
    private static readonly PropertyChangedEventArgs PixelsPerSecondArgs = new(nameof(PixelsPerSecond));

    /// <summary>Identifies changes to <see cref="PlaybackRate"/>.</summary>
    private static readonly PropertyChangedEventArgs PlaybackRateArgs = new(nameof(PlaybackRate));

    /// <summary>Identifies changes to <see cref="PlayheadColor"/>.</summary>
    private static readonly PropertyChangedEventArgs PlayheadColorArgs = new(nameof(PlayheadColor));

    /// <summary>Identifies changes to <see cref="ResizeHandleWidth"/>.</summary>
    private static readonly PropertyChangedEventArgs ResizeHandleWidthArgs = new(nameof(ResizeHandleWidth));

    /// <summary>Identifies changes to <see cref="RulerHeight"/>.</summary>
    private static readonly PropertyChangedEventArgs RulerHeightArgs = new(nameof(RulerHeight));

    /// <summary>Identifies changes to <see cref="SelectedItem"/>.</summary>
    private static readonly PropertyChangedEventArgs SelectedItemArgs = new(nameof(SelectedItem));

    /// <summary>Identifies changes to <see cref="SelectedItemBackground"/>.</summary>
    private static readonly PropertyChangedEventArgs SelectedItemBackgroundArgs = new(nameof(SelectedItemBackground));

    /// <summary>Identifies changes to <see cref="SelectedTrack"/>.</summary>
    private static readonly PropertyChangedEventArgs SelectedTrackArgs = new(nameof(SelectedTrack));

    /// <summary>Identifies changes to <see cref="SnapInterval"/>.</summary>
    private static readonly PropertyChangedEventArgs SnapIntervalArgs = new(nameof(SnapInterval));

    /// <summary>Identifies changes to <see cref="Time"/>.</summary>
    private static readonly PropertyChangedEventArgs TimeArgs = new(nameof(Time));

    /// <summary>Identifies changes to <see cref="TrackHeaderWidth"/>.</summary>
    private static readonly PropertyChangedEventArgs TrackHeaderWidthArgs = new(nameof(TrackHeaderWidth));

    /// <summary>Identifies changes to <see cref="TrackHeight"/>.</summary>
    private static readonly PropertyChangedEventArgs TrackHeightArgs = new(nameof(TrackHeight));

    /// <summary>Identifies changes to <see cref="Tracks"/>.</summary>
    private static readonly PropertyChangedEventArgs TracksArgs = new(nameof(Tracks));

    /// <summary>Identifies changes to <see cref="VerticalOffset"/>.</summary>
    private static readonly PropertyChangedEventArgs VerticalOffsetArgs = new(nameof(VerticalOffset));

    /// <summary>Identifies changes to <see cref="ViewStart"/>.</summary>
    private static readonly PropertyChangedEventArgs ViewStartArgs = new(nameof(ViewStart));

    /// <summary>Identifies changes to <see cref="WasEditCompleted"/>.</summary>
    private static readonly PropertyChangedEventArgs WasEditCompletedArgs = new(nameof(WasEditCompleted));

    /// <summary>Defines the maximum supported zoom scale.</summary>
    public const float MaximumPixelsPerSecond = 10000f;

    /// <summary>Defines the minimum supported zoom scale.</summary>
    public const float MinimumPixelsPerSecond = 0.01f;

    /// <summary>Stores the model subscriptions currently owned by this attached control.</summary>
    private readonly HashSet<TimelineTrack> _observedTracks = new(ReferenceEqualityComparer.Instance);

    /// <summary>Caches only blocks intersecting the visible time and row ranges.</summary>
    private readonly List<TimelineItemLayout> _visibleItems = new();

    /// <summary>Caches ruler text until its time-to-screen mapping changes.</summary>
    private readonly List<TimelineTick> _ticks = new();

    /// <summary>Caches the visible row headers.</summary>
    private readonly List<TimelineTrackLayout> _visibleTracks = new();

    /// <summary>Provides the independent horizontal scrollbar.</summary>
    private readonly ScrollBar _horizontalScrollBar = new() { Orientation = ControlOrientation.Horizontal };

    /// <summary>Provides the independent vertical scrollbar.</summary>
    private readonly ScrollBar _verticalScrollBar = new() { Orientation = ControlOrientation.Vertical };

    /// <summary>Stores explicitly supplied observable timeline rows.</summary>
    private readonly TrackableCollection<TimelineTrack> _tracks = new();

    /// <summary>Stores the absolute timeline body rectangle.</summary>
    private Bounds2 _plot;

    /// <summary>Stores the single pointer owning the current gesture.</summary>
    private ControlPointer? _pointer;

    /// <summary>Stores the optional completed-edit command.</summary>
    private ICommand? _command;

    /// <summary>Stores the button that started the captured gesture.</summary>
    private MouseButton _button;

    /// <summary>Stores the style value and local override for GridColor.</summary>
    private StyleProperty<Color> _styleGridColor = new(Color.Gray * 0.5f);

    /// <summary>Stores the style value and local override for ItemBackground.</summary>
    private StyleProperty<Color> _styleItemBackground = new(new Color(57, 112, 165));

    /// <summary>Stores the style value and local override for PlayheadColor.</summary>
    private StyleProperty<Color> _stylePlayheadColor = new(new Color(255, 183, 55));

    /// <summary>Stores the style value and local override for SelectedItemBackground.</summary>
    private StyleProperty<Color> _styleSelectedItemBackground = new(new Color(48, 148, 210));

    /// <summary>Stores the style value and local override for ResizeHandleWidth.</summary>
    private StyleProperty<float> _styleResizeHandleWidth = new(6f);

    /// <summary>Stores the style value and local override for RulerHeight.</summary>
    private StyleProperty<float> _styleRulerHeight = new(28f);

    /// <summary>Stores the style value and local override for TrackHeaderWidth.</summary>
    private StyleProperty<float> _styleTrackHeaderWidth = new(140f);

    /// <summary>Stores the style value and local override for TrackHeight.</summary>
    private StyleProperty<float> _styleTrackHeight = new(36f);

    /// <summary>Stores the active block operation.</summary>
    private TimelineEditKind _editKind;

    /// <summary>Stores the block being edited.</summary>
    private TimelineItem? _editItem;

    /// <summary>Stores the selected block.</summary>
    private TimelineItem? _selectedItem;

    /// <summary>Stores the row containing the active edit.</summary>
    private TimelineTrack? _editTrack;

    /// <summary>Stores the selected row.</summary>
    private TimelineTrack? _selectedTrack;

    /// <summary>Stores the pointer position at gesture start.</summary>
    private Vector2 _origin;

    /// <summary>Tracks whether cached visible geometry needs rebuilding.</summary>
    private bool _dirty = true;

    /// <summary>Stores whether the playhead advances during Update.</summary>
    private bool _isPlaying;

    /// <summary>Stores whether block editing is disabled.</summary>
    private bool _isReadOnly;

    /// <summary>Stores whether preview playback wraps at the timeline end.</summary>
    private bool _loop;

    /// <summary>Tracks an active background pan.</summary>
    private bool _panning;

    /// <summary>Tracks an active ruler gesture.</summary>
    private bool _scrubbing;

    /// <summary>Prevents recursive scrollbar value synchronization.</summary>
    private bool _syncingBars;

    /// <summary>Tracks whether the time mapping requires new ruler labels.</summary>
    private bool _ticksDirty = true;

    /// <summary>Tracks whether model event subscriptions are active.</summary>
    private bool _trackingModels;

    /// <summary>Stores the per-frame completed-edit flag.</summary>
    private bool _wasEditCompleted;

    /// <summary>Distinguishes control-owned range changes from external model edits.</summary>
    private bool _writing;

    /// <summary>Stores the editable end time in seconds.</summary>
    private double _duration = 10;

    /// <summary>Stores the duration most recently written by this gesture.</summary>
    private double _lastDuration;

    /// <summary>Stores the start most recently written by this gesture.</summary>
    private double _lastStart;

    /// <summary>Stores the shortest interactively resized block.</summary>
    private double _minimumItemDuration = 0.01;

    /// <summary>Stores the duration before the edit.</summary>
    private double _originalDuration;

    /// <summary>Stores the start time before the edit.</summary>
    private double _originalStart;

    /// <summary>Stores the horizontal time offset before panning.</summary>
    private double _panStart;

    /// <summary>Stores the preview playback speed multiplier.</summary>
    private double _playbackRate = 1;

    /// <summary>Stores the edit snap interval; zero disables snapping.</summary>
    private double _snapInterval = 0.1;

    /// <summary>Stores the current playhead time.</summary>
    private double _time;

    /// <summary>Stores the leftmost visible time.</summary>
    private double _viewStart;

    /// <summary>Caches the resolved font line height.</summary>
    private float _lineHeight = 16;

    /// <summary>Stores the vertical offset before panning.</summary>
    private float _panVertical;

    /// <summary>Stores the zoom scale in logical units per second.</summary>
    private float _pixelsPerSecond = 100;

    /// <summary>Stores the vertical scroll offset in logical units.</summary>
    private float _verticalOffset;

    /// <summary>Stores the row count used for the last scrollbar arrangement.</summary>
    private int _arrangedTrackCount = -1;

    /// <summary>Stores an optional caller-defined completion parameter.</summary>
    private object? _commandParameter;

    /// <summary>Occurs when the selected row or block changes.</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>Occurs when the playhead time changes, including preview playback.</summary>
    public event EventHandler? TimeChanged;

    /// <summary>Occurs after a block gesture starts, before its first range change.</summary>
    public event EventHandler<TimelineEditEventArgs>? EditStarted;

    /// <summary>Occurs after an interactive block range change.</summary>
    public event EventHandler<TimelineEditEventArgs>? EditChanged;

    /// <summary>Occurs once after a changed block gesture completes successfully.</summary>
    public event EventHandler<TimelineEditEventArgs>? EditCompleted;

    /// <summary>Occurs when a block gesture is cancelled, including loss of pointer capture.</summary>
    public event EventHandler<TimelineEditEventArgs>? EditCancelled;

    /// <summary>Creates an empty timeline with two independently styled scrollbars.</summary>
    public TimelineControl()
    {
        _tracks.CollectionChanged += OnTracksChanged;
        _horizontalScrollBar.ValueChanged += OnHorizontalScrollChanged;
        _verticalScrollBar.ValueChanged += OnVerticalScrollChanged;
        base.Add(_horizontalScrollBar);
        base.Add(_verticalScrollBar);
    }

    /// <summary>Gets or sets the logical height of each track row. Local assignments override styling.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The logical size is outside its supported range.</exception>
    public float TrackHeight
    {
        get => _styleTrackHeight.Value;
        set
        {
            ValidateSizeValue(value, 12, 1024);
            if (_styleTrackHeight.SetLocal(value))
                NotifyPropertyChanged(TrackHeightArgs);
        }
    }

    /// <summary>Gets or sets the logical height of the time ruler. Local assignments override styling.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The logical size is outside its supported range.</exception>
    public float RulerHeight
    {
        get => _styleRulerHeight.Value;
        set
        {
            ValidateSizeValue(value, 12, 256);
            if (_styleRulerHeight.SetLocal(value))
                NotifyPropertyChanged(RulerHeightArgs);
        }
    }

    /// <summary>Gets or sets the logical width reserved for row captions; zero hides captions. Local assignments override styling.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The logical size is outside its supported range.</exception>
    public float TrackHeaderWidth
    {
        get => _styleTrackHeaderWidth.Value;
        set
        {
            ValidateSizeValue(value, 0, 4096);
            if (_styleTrackHeaderWidth.SetLocal(value))
                NotifyPropertyChanged(TrackHeaderWidthArgs);
        }
    }

    /// <summary>Gets or sets the logical hit width of a visible resize handle. Local assignments override styling.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The logical size is outside its supported range.</exception>
    public float ResizeHandleWidth
    {
        get => _styleResizeHandleWidth.Value;
        set
        {
            ValidateSizeValue(value, 1, 24);
            if (_styleResizeHandleWidth.SetLocal(value))
                NotifyPropertyChanged(ResizeHandleWidthArgs);
        }
    }

    /// <summary>Gets or sets the default block fill color. Local assignments override styling.</summary>
    public Color ItemBackground
    {
        get => _styleItemBackground.Value;
        set
        {
            if (_styleItemBackground.SetLocal(value))
                NotifyPropertyChanged(ItemBackgroundArgs);
        }
    }

    /// <summary>Gets or sets the fill color of the selected block. Local assignments override styling.</summary>
    public Color SelectedItemBackground
    {
        get => _styleSelectedItemBackground.Value;
        set
        {
            if (_styleSelectedItemBackground.SetLocal(value))
                NotifyPropertyChanged(SelectedItemBackgroundArgs);
        }
    }

    /// <summary>Gets or sets the row separator and time grid color. Local assignments override styling.</summary>
    public Color GridColor
    {
        get => _styleGridColor.Value;
        set
        {
            if (_styleGridColor.SetLocal(value))
                NotifyPropertyChanged(GridColorArgs);
        }
    }

    /// <summary>Gets or sets the playhead and ruler indicator color. Local assignments override styling.</summary>
    public Color PlayheadColor
    {
        get => _stylePlayheadColor.Value;
        set
        {
            if (_stylePlayheadColor.SetLocal(value))
                NotifyPropertyChanged(PlayheadColorArgs);
        }
    }

    /// <summary>Gets the observable rows. Block changes propagate through their owning row.</summary>
    public TrackableCollection<TimelineTrack> Tracks => _tracks;

    /// <summary>Gets the independent scrollbar controlling horizontal time navigation.</summary>
    public ScrollBar HorizontalScrollBar => _horizontalScrollBar;

    /// <summary>Gets the independent scrollbar controlling vertical row navigation.</summary>
    public ScrollBar VerticalScrollBar => _verticalScrollBar;

    /// <summary>Gets the timeline body in absolute logical coordinates.</summary>
    public Bounds2 PlotBounds => _plot;

    /// <summary>Gets whether a block is currently being moved or resized.</summary>
    public bool IsEditing => _editItem is not null;

    /// <summary>Gets whether an edit completed during the current workspace frame.</summary>
    public bool WasEditCompleted => _wasEditCompleted;

    /// <summary>Gets the currently selected row, including row-header-only selection.</summary>
    public TimelineTrack? SelectedTrack => _selectedTrack;

    /// <summary>Gets or sets the selected block; null clears selection.</summary>
    /// <exception cref="ArgumentException">The block is not contained in this timeline.</exception>
    public TimelineItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            TimelineTrack? track = null;
            if (value is not null)
            {
                foreach (var candidate in Tracks)
                    if (candidate.Items.Contains(value)) { track = candidate; break; }
                if (track is null) throw new ArgumentException("The item must belong to this timeline.", nameof(value));
            }
            if (!ReferenceEquals(value, _selectedItem)) FinishInteraction(true);
            Select(track, value);
        }
    }

    /// <summary>Gets or sets the positive finite editing range end in seconds. Existing blocks are not truncated.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The end is nonfinite or shorter than MinimumItemDuration.</exception>
    public double Duration
    {
        get => _duration;
        set
        {
            if (!double.IsFinite(value) || value < MinimumItemDuration) throw new ArgumentOutOfRangeException(nameof(value));
            if (_duration == value) return;
            FinishInteraction(true);
            SetNotify(ref _duration, value, DurationArgs);
            Time = Math.Min(Time, value);
            ViewStart = _viewStart;
        }
    }

    /// <summary>Gets or sets the playhead time in seconds, clamped to the editing range.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The supplied time is nonfinite.</exception>
    public double Time
    {
        get => _time;
        set
        {
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            if (SetNotify(ref _time, Math.Clamp(value, 0, Duration), TimeArgs)) TimeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Gets or sets logical units per second, constrained to the supported zoom limits.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The zoom is nonfinite or outside the supported range.</exception>
    public float PixelsPerSecond
    {
        get => _pixelsPerSecond;
        set
        {
            ValidateSizeValue(value, MinimumPixelsPerSecond, MaximumPixelsPerSecond);
            if (_pixelsPerSecond == value) return;
            FinishInteraction(true);
            SetNotify(ref _pixelsPerSecond, value, PixelsPerSecondArgs);
            ViewStart = _viewStart;
        }
    }

    /// <summary>Gets or sets the leftmost visible time, clamped to the scrollable range.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The supplied time is nonfinite.</exception>
    public double ViewStart
    {
        get => _viewStart;
        set
        {
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _viewStart, Math.Clamp(value, 0, MaximumViewStart), ViewStartArgs);
        }
    }

    /// <summary>Gets or sets the vertical logical scroll offset, clamped to the row extent.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The supplied offset is nonfinite.</exception>
    public float VerticalOffset
    {
        get => _verticalOffset;
        set
        {
            if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _verticalOffset, Math.Clamp(value, 0, MaximumVerticalOffset), VerticalOffsetArgs);
        }
    }

    /// <summary>Gets or sets the finite nonnegative edit snap interval in seconds; zero disables snapping.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The interval is negative or nonfinite.</exception>
    public double SnapInterval
    {
        get => _snapInterval;
        set
        {
            if (!double.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _snapInterval, value, SnapIntervalArgs);
        }
    }

    /// <summary>Gets or sets the shortest interactively resized duration, between zero and Duration.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The minimum is nonpositive, nonfinite, or larger than Duration.</exception>
    public double MinimumItemDuration
    {
        get => _minimumItemDuration;
        set
        {
            if (!double.IsFinite(value) || value <= 0 || value > Duration) throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _minimumItemDuration, value, MinimumItemDurationArgs);
        }
    }

    /// <summary>Gets or sets whether blocks are read-only; navigation, seeking, and playback remain available.</summary>
    public bool IsReadOnly
    {
        get => _isReadOnly;
        set
        {
            if (value) FinishInteraction(true);
            SetNotify(ref _isReadOnly, value, IsReadOnlyArgs);
        }
    }

    /// <summary>Gets or sets whether Update advances the preview playhead using the supplied elapsed time.</summary>
    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetNotify(ref _isPlaying, value, IsPlayingArgs);
    }

    /// <summary>Gets or sets whether preview playback wraps to the start at Duration.</summary>
    public bool Loop
    {
        get => _loop;
        set => SetNotify(ref _loop, value, LoopArgs);
    }

    /// <summary>Gets or sets the finite positive preview speed, up to 1000 times normal playback.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed is nonpositive, nonfinite, or exceeds 1000.</exception>
    public double PlaybackRate
    {
        get => _playbackRate;
        set
        {
            if (!double.IsFinite(value) || value <= 0 || value > 1000) throw new ArgumentOutOfRangeException(nameof(value));
            SetNotify(ref _playbackRate, value, PlaybackRateArgs);
        }
    }

    /// <summary>Gets or sets the optional command executed once after a changed edit completes.</summary>
    public ICommand? Command
    {
        get => _command;
        set => SetNotify(ref _command, value, CommandArgs);
    }

    /// <summary>Gets or sets the command parameter; null supplies the completed TimelineEditEventArgs snapshot.</summary>
    public object? CommandParameter
    {
        get => _commandParameter;
        set => SetNotify(ref _commandParameter, value, CommandParameterArgs);
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => nameof(TimelineControl);

    /// <summary>Gets the cached visible block geometry.</summary>
    internal IReadOnlyList<TimelineItemLayout> VisibleItems => _visibleItems;

    /// <summary>Gets the cached visible row headers.</summary>
    internal IReadOnlyList<TimelineTrackLayout> VisibleTracks => _visibleTracks;

    /// <summary>Gets the cached ruler tick positions and text.</summary>
    internal IReadOnlyList<TimelineTick> Ticks => _ticks;

    /// <summary>Gets the prepared font line height.</summary>
    internal float LineHeight => _lineHeight;

    /// <summary>Gets the visible duration corresponding to the body width.</summary>
    private double VisibleDuration => _plot.Width / (double)PixelsPerSecond;

    /// <summary>Gets the largest horizontal time offset.</summary>
    private double MaximumViewStart => Math.Max(0, Duration - VisibleDuration);

    /// <summary>Gets the largest vertical logical scroll offset.</summary>
    private float MaximumVerticalOffset => (float)Math.Max(0, (double)Tracks.Count * TrackHeight - _plot.Height);

    /// <summary>Fits the editing range into the current timeline body, subject to zoom limits.</summary>
    public void ZoomToFit()
    {
        if (_plot.Width <= 0) return;
        PixelsPerSecond = (float)Math.Clamp(_plot.Width / Duration, MinimumPixelsPerSecond, MaximumPixelsPerSecond);
        ViewStart = 0;
    }

    /// <summary>Changes zoom while keeping the time beneath an absolute logical point fixed where bounds permit.</summary>
    /// <param name="anchor">The absolute logical point anchoring the zoom.</param>
    /// <param name="factor">The finite positive zoom multiplier.</param>
    /// <exception cref="ArgumentOutOfRangeException">The point or multiplier is nonfinite, or the multiplier is nonpositive.</exception>
    public void ZoomAt(Vector2 anchor, float factor)
    {
        if (!float.IsFinite(anchor.X) || !float.IsFinite(anchor.Y) || !float.IsFinite(factor) || factor <= 0)
            throw new ArgumentOutOfRangeException(nameof(factor));
        float x = Math.Clamp(anchor.X - _plot.X, 0, _plot.Width);
        double time = ViewStart + x / (double)PixelsPerSecond;
        PixelsPerSecond = (float)Math.Clamp((double)PixelsPerSecond * factor, MinimumPixelsPerSecond, MaximumPixelsPerSecond);
        ViewStart = time - x / (double)PixelsPerSecond;
    }

    /// <summary>Converts an absolute logical horizontal position into a time clamped to the editing range.</summary>
    /// <param name="position">The absolute logical position.</param>
    /// <returns>The corresponding time in seconds.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The position is nonfinite.</exception>
    public double PositionToTime(Vector2 position)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y)) throw new ArgumentOutOfRangeException(nameof(position));
        return Math.Clamp(ViewStart + (position.X - _plot.X) / (double)PixelsPerSecond, 0, Duration);
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();
        _trackingModels = true;
        foreach (var track in Tracks) ObserveTrack(track);
        _dirty = true;
        _ticksDirty = true;
    }

    /// <inheritdoc/>
    protected override void OnDetached()
    {
        FinishInteraction(true);
        _trackingModels = false;
        foreach (var track in _observedTracks) track.PropertyChanged -= OnTrackChanged;
        _observedTracks.Clear();
        base.OnDetached();
    }

    /// <inheritdoc/>
    internal override void BeginFrame()
    {
        base.BeginFrame();
        if (_wasEditCompleted) SetNotify(ref _wasEditCompleted, false, WasEditCompletedArgs);
    }

    /// <summary>Invalidates only the visual or layout state affected by a direct property change.</summary>
    /// <param name="notification">The cached property notification.</param>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        string? name = notification.PropertyName;
        if (name == nameof(Tracks))
        {
            if (!_writing) FinishInteraction(true);
            if (_selectedTrack is not null && (!Tracks.Contains(_selectedTrack) ||
                _selectedItem is not null && !_selectedTrack.Items.Contains(_selectedItem))) Select(null, null);
            _dirty = true;
            InvalidateVisual();
            if (_arrangedTrackCount != Tracks.Count) InvalidateArrange();
            return;
        }

        if (name is nameof(TrackHeight) or nameof(RulerHeight) or nameof(TrackHeaderWidth))
        {
            FinishInteraction(true);
            _dirty = true;
            InvalidateArrange();
            InvalidateVisual();
            return;
        }

        if (name is nameof(ItemBackground) or nameof(SelectedItemBackground) or nameof(GridColor) or nameof(PlayheadColor) or
            nameof(ResizeHandleWidth) or nameof(Time) or nameof(IsEditing) or nameof(WasEditCompleted) or nameof(SelectedItem) or
            nameof(SelectedTrack) or nameof(IsReadOnly) or nameof(IsPlaying) or nameof(Loop) or nameof(PlaybackRate) or
            nameof(Command) or nameof(CommandParameter) or nameof(SnapInterval) or nameof(MinimumItemDuration))
        {
            InvalidateVisual();
            return;
        }

        _dirty = true;
        if (name is nameof(ViewStart) or nameof(PixelsPerSecond) or nameof(Duration)) _ticksDirty = true;
        if (name is nameof(ViewStart) or nameof(VerticalOffset) or nameof(PixelsPerSecond) or nameof(Duration))
            InvalidateVisual();
        else
            base.OnPropertyChanged(notification);
    }

    /// <summary>Measures the preferred viewport, scrollbars, and resolved font metrics.</summary>
    /// <param name="context">The current layout and font context.</param>
    /// <param name="available">The available logical space.</param>
    /// <returns>The preferred timeline size before the base control applies size constraints.</returns>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _horizontalScrollBar.Measure(context, available);
        _verticalScrollBar.Measure(context, available);
        float lineHeight = GetFont(context).Measure("M").Height;
        if (_lineHeight != lineHeight) _ticksDirty = true;
        _lineHeight = lineHeight;
        return new Size2(600, 280);
    }

    /// <summary>Arranges the fixed row captions, ruler, body viewport, and two scrollbars.</summary>
    /// <param name="contentBounds">The absolute logical area inside the control padding.</param>
    protected override void ArrangeOverride(Bounds2 contentBounds)
    {
        float horizontal = MathF.Min(contentBounds.Height, _horizontalScrollBar.DesiredSize.Height);
        float vertical = MathF.Min(contentBounds.Width, _verticalScrollBar.DesiredSize.Width);
        float header = MathF.Min(TrackHeaderWidth, MathF.Max(0, contentBounds.Width - vertical));
        float ruler = MathF.Min(RulerHeight, MathF.Max(0, contentBounds.Height - horizontal));
        var plot = new Bounds2(contentBounds.X + header, contentBounds.Y + ruler,
            MathF.Max(0, contentBounds.Width - header - vertical), MathF.Max(0, contentBounds.Height - ruler - horizontal));
        if (_plot.X != plot.X || _plot.Width != plot.Width) _ticksDirty = true;
        _plot = plot;
        _horizontalScrollBar.Arrange(new Bounds2(_plot.X, _plot.Bottom, _plot.Width, horizontal), true);
        _verticalScrollBar.Arrange(new Bounds2(_plot.Right, _plot.Y, vertical, _plot.Height), true);
        _arrangedTrackCount = Tracks.Count;
        ViewStart = _viewStart;
        VerticalOffset = _verticalOffset;
        _dirty = true;
        RebuildView();
    }

    /// <summary>Advances optional preview playback and prepares invalidated visible geometry.</summary>
    /// <param name="elapsed">The deterministic elapsed duration supplied by the workspace.</param>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        if (IsPlaying && !IsEditing && !_scrubbing && elapsed > TimeSpan.Zero)
        {
            double next = Time + elapsed.TotalSeconds * PlaybackRate;
            if (!double.IsFinite(next)) next = Duration;
            if (Loop) Time = next % Duration;
            else
            {
                Time = Math.Min(next, Duration);
                if (next >= Duration) IsPlaying = false;
            }
        }
        RebuildView();
    }

    /// <summary>Draws the prepared timeline without modifying models, cached layout, or interaction.</summary>
    /// <param name="context">The active ordered drawing context.</param>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        TimelineControlRenderer.Draw(this, context);
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled()
    {
        FinishInteraction(true);
        base.OnInteractionCancelled();
    }

    /// <summary>Subscribes to a row once while this control is attached.</summary>
    /// <param name="track">The row whose blocks and settings are observed.</param>
    private void ObserveTrack(TimelineTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        if (_trackingModels && _observedTracks.Add(track)) track.PropertyChanged += OnTrackChanged;
    }

    /// <summary>Maintains row subscriptions for structural collection changes.</summary>
    /// <param name="sender">The row collection.</param>
    /// <param name="args">The structural change description.</param>
    private void OnTracksChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var track in _observedTracks) track.PropertyChanged -= OnTrackChanged;
            _observedTracks.Clear();
            if (_trackingModels) foreach (var track in Tracks) ObserveTrack(track);
        }
        else
        {
            if (args.OldItems is not null)
                foreach (var value in args.OldItems)
                    if (value is TimelineTrack track && !Tracks.Contains(track) && _observedTracks.Remove(track))
                        track.PropertyChanged -= OnTrackChanged;
            if (args.NewItems is not null)
                foreach (var value in args.NewItems)
                    if (value is TimelineTrack track) ObserveTrack(track);
        }
        NotifyPropertyChanged(TracksArgs);
    }

    /// <summary>Invalidates cached presentation after an observed row or block changes.</summary>
    /// <param name="sender">The observed row.</param>
    /// <param name="args">The forwarded model property notification.</param>
    private void OnTrackChanged(object? sender, PropertyChangedEventArgs args) => NotifyPropertyChanged(TracksArgs);

    /// <summary>Synchronizes the horizontal normalized scrollbar with time navigation.</summary>
    /// <param name="sender">The horizontal scrollbar.</param>
    /// <param name="args">The value change notification.</param>
    private void OnHorizontalScrollChanged(object? sender, EventArgs args)
    {
        if (!_syncingBars) ViewStart = _horizontalScrollBar.Value * MaximumViewStart;
    }

    /// <summary>Synchronizes vertical scrollbar input with the visible row offset.</summary>
    /// <param name="sender">The vertical scrollbar.</param>
    /// <param name="args">The value change notification.</param>
    private void OnVerticalScrollChanged(object? sender, EventArgs args)
    {
        if (!_syncingBars) VerticalOffset = _verticalScrollBar.Value;
    }

    /// <summary>Rebuilds visible rectangles, ruler labels, and scrollbar values only after invalidation.</summary>
    private void RebuildView()
    {
        if (!_dirty) return;
        _dirty = false;
        _visibleItems.Clear();
        _visibleTracks.Clear();
        _syncingBars = true;
        try
        {
            double range = MaximumViewStart;
            _horizontalScrollBar.Maximum = range > 0 ? 1 : 0;
            _horizontalScrollBar.ViewportSize = range > 0 ? (float)Math.Min(float.MaxValue, VisibleDuration / range) : 1;
            _horizontalScrollBar.SmallChange = range > 0 ? (float)Math.Min(1, 24 / PixelsPerSecond / range) : 0;
            _horizontalScrollBar.LargeChange = range > 0 ? (float)Math.Min(1, VisibleDuration / range) : 0;
            _horizontalScrollBar.Value = range > 0 ? (float)(ViewStart / range) : 0;
            _verticalScrollBar.Maximum = MaximumVerticalOffset;
            _verticalScrollBar.ViewportSize = _plot.Height;
            _verticalScrollBar.SmallChange = TrackHeight;
            _verticalScrollBar.LargeChange = _plot.Height;
            _verticalScrollBar.Value = VerticalOffset;
        }
        finally { _syncingBars = false; }

        if (_plot.Width <= 0 || _plot.Height <= 0) return;
        float x = _plot.X - Bounds.X;
        float y = _plot.Y - Bounds.Y;
        double visibleEnd = Math.Min(Duration, ViewStart + VisibleDuration);
        int first = Math.Min(Tracks.Count, (int)Math.Min(int.MaxValue, Math.Floor(VerticalOffset / TrackHeight)));
        int last = (int)Math.Min(Tracks.Count, (double)first + Math.Ceiling(_plot.Height / TrackHeight) + 1);
        for (int index = first; index < last; index++)
        {
            var track = Tracks[index] ?? throw new InvalidOperationException("Timeline rows cannot be null.");
            float top = y + index * TrackHeight - VerticalOffset;
            _visibleTracks.Add(new TimelineTrackLayout(track, index, new Bounds2(ContentBounds.X - Bounds.X, top, _plot.X - ContentBounds.X, TrackHeight)));
            foreach (var item in track.Items)
            {
                if (item is null) throw new InvalidOperationException("Timeline blocks cannot be null.");
                if (item.End <= ViewStart || item.Start >= visibleEnd) continue;
                float left = x + (float)((Math.Max(item.Start, ViewStart) - ViewStart) * PixelsPerSecond);
                float right = x + (float)((Math.Min(item.End, visibleEnd) - ViewStart) * PixelsPerSecond);
                _visibleItems.Add(new TimelineItemLayout(track, item,
                    new Bounds2(left, top + 3, MathF.Max(0, right - left), MathF.Max(0, TrackHeight - 6)),
                    item.Start >= ViewStart, item.End <= visibleEnd));
            }
        }

        if (!_ticksDirty) return;
        _ticksDirty = false;
        _ticks.Clear();
        double desiredStep = Math.Max(1e-9, Math.Max(Math.Max(80, _lineHeight * 4), _plot.Width / 128d) / PixelsPerSecond);
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(desiredStep)));
        double fraction = desiredStep / magnitude;
        double step = magnitude * (fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10);
        double firstTick = Math.Ceiling(ViewStart / step) * step;
        for (int i = 0; i < 160; i++)
        {
            double time = firstTick + i * step;
            if (!double.IsFinite(time) || time > visibleEnd) break;
            float position = x + (float)((time - ViewStart) * PixelsPerSecond);
            _ticks.Add(new TimelineTick(position, time.ToString(step < 0.001 ? "0.######" : "0.###", CultureInfo.InvariantCulture) + "s"));
        }
    }

    /// <summary>Updates both selection properties as one logical selection operation.</summary>
    /// <param name="track">The selected row, or null.</param>
    /// <param name="item">The selected block, or null.</param>
    private void Select(TimelineTrack? track, TimelineItem? item)
    {
        bool trackChanged = !ReferenceEquals(_selectedTrack, track);
        bool itemChanged = !ReferenceEquals(_selectedItem, item);
        _selectedTrack = track;
        _selectedItem = item;
        if (trackChanged) NotifyPropertyChanged(SelectedTrackArgs);
        if (itemChanged) NotifyPropertyChanged(SelectedItemArgs);
        if (trackChanged || itemChanged) SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Checks whether a block can be edited within the configured time range.</summary>
    /// <param name="track">The owning row.</param>
    /// <param name="item">The block being inspected.</param>
    /// <returns>Whether the block is unlocked and fully inside the editing range.</returns>
    internal bool CanEdit(TimelineTrack track, TimelineItem item) =>
        !IsReadOnly && !track.IsLocked && !item.IsLocked && item.End <= Duration;

    /// <summary>Begins a block operation without modifying its time range.</summary>
    /// <param name="track">The containing row.</param>
    /// <param name="item">The edited block.</param>
    /// <param name="kind">The time range operation.</param>
    private void BeginEdit(TimelineTrack track, TimelineItem item, TimelineEditKind kind)
    {
        _editTrack = track;
        _editItem = item;
        _editKind = kind;
        _originalStart = _lastStart = item.Start;
        _originalDuration = _lastDuration = item.Duration;
        IsPlaying = false;
        if (!ReferenceEquals(_editItem, item) || !ReferenceEquals(_editTrack, track)) return;
        NotifyPropertyChanged(IsEditingArgs);
        if (ReferenceEquals(_editItem, item) && ReferenceEquals(_editTrack, track) && EditStarted is { } started)
            started(this, EditSnapshot(track, item));
    }

    /// <summary>Writes a changed time range while distinguishing its notifications from external edits.</summary>
    /// <param name="start">The constrained start time.</param>
    /// <param name="duration">The constrained duration.</param>
    private void WriteRange(double start, double duration)
    {
        if (_editItem is not { } item || _editTrack is not { } track || item.Start == start && item.Duration == duration) return;
        _lastStart = start;
        _lastDuration = duration;
        _writing = true;
        try { item.SetRange(start, duration); }
        finally { _writing = false; }
        _dirty = true;
        InvalidateVisual();
        if (!Tracks.Contains(track) || !track.Items.Contains(item) || item.Start != start || item.Duration != duration)
        {
            FinishInteraction(true);
            return;
        }
        if (EditChanged is { } changed) changed(this, EditSnapshot(track, item));
    }

    /// <summary>Creates an immutable snapshot of the active gesture.</summary>
    /// <param name="track">The row at gesture start.</param>
    /// <param name="item">The edited block.</param>
    /// <returns>The captured original and current range.</returns>
    private TimelineEditEventArgs EditSnapshot(TimelineTrack track, TimelineItem item) =>
        new(track, item, _editKind, _originalStart, _originalDuration, item.Start, item.Duration);

    /// <summary>Ends navigation or editing and restores only range values still owned by a cancelled gesture.</summary>
    /// <param name="cancel">Whether to cancel instead of committing the gesture.</param>
    private void FinishInteraction(bool cancel)
    {
        var pointer = _pointer;
        var item = _editItem;
        var track = _editTrack;
        _pointer = null;
        _panning = false;
        _scrubbing = false;
        _editItem = null;
        _editTrack = null;
        if (pointer is ControlPointer captured) Workspace?.ReleasePointer(this, captured);
        if (item is null || track is null) return;

        bool changed = item.Start != _originalStart || item.Duration != _originalDuration;
        if (cancel && changed && Tracks.Contains(track) && track.Items.Contains(item) && item.Start == _lastStart && item.Duration == _lastDuration)
        {
            _writing = true;
            try { item.SetRange(_originalStart, _originalDuration); }
            finally { _writing = false; }
            _dirty = true;
        }
        var snapshot = EditSnapshot(track, item);
        NotifyPropertyChanged(IsEditingArgs);
        if (cancel)
        {
            EditCancelled?.Invoke(this, snapshot);
            return;
        }
        if (!changed) return;
        SetNotify(ref _wasEditCompleted, true, WasEditCompletedArgs);
        EditCompleted?.Invoke(this, snapshot);
        object? parameter = CommandParameter ?? snapshot;
        var command = Command;
        if (command?.CanExecute(parameter) == true) command.Execute(parameter);
    }

    /// <summary>Rounds time to the edit grid without introducing nonfinite arithmetic results.</summary>
    /// <param name="time">The proposed time.</param>
    /// <param name="free">Whether the gesture bypasses snapping.</param>
    /// <returns>The snapped time, or the unsnapped value when snapping is disabled.</returns>
    private double Snap(double time, bool free)
    {
        if (free || SnapInterval <= 0) return time;
        double result = Math.Round(time / SnapInterval, MidpointRounding.AwayFromZero) * SnapInterval;
        return double.IsFinite(result) ? result : time;
    }

    /// <summary>Applies pointer displacement to the original range, preserving the opposite edge during resizing.</summary>
    /// <param name="input">The current captured pointer event.</param>
    private void DragRange(ControlInputEvent input)
    {
        double delta = (input.Position.X - _origin.X) / (double)PixelsPerSecond;
        if (delta == 0) { WriteRange(_originalStart, _originalDuration); return; }
        if (_editKind == TimelineEditKind.Move)
        {
            WriteRange(Math.Clamp(Snap(_originalStart + delta, input.Shift), 0, Duration - _originalDuration), _originalDuration);
        }
        else if (_editKind == TimelineEditKind.ResizeStart)
        {
            double end = _originalStart + _originalDuration;
            double minimum = Math.Min(MinimumItemDuration, _originalDuration);
            double start = Math.Clamp(Snap(_originalStart + delta, input.Shift), 0, end - minimum);
            WriteRange(start, end - start);
        }
        else
        {
            double minimum = Math.Min(MinimumItemDuration, _originalDuration);
            double end = Math.Clamp(Snap(_originalStart + _originalDuration + delta, input.Shift), _originalStart + minimum, Duration);
            WriteRange(_originalStart, end - _originalStart);
        }
    }

    /// <summary>Processes pointer navigation, range editing, ruler seeking, and keyboard shortcuts.</summary>
    /// <param name="context">The workspace focus and pointer capture context.</param>
    /// <param name="args">The routed event state; handled timeline input stops bubbling.</param>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        RebuildView();
        var input = args.Input;
        if (input.Type == ControlInputEventType.KeyDown && input.Key == Keys.Escape)
        {
            if (_pointer is not null || IsEditing) FinishInteraction(true);
            else if (_selectedTrack is not null || _selectedItem is not null) Select(null, null);
            else return;
            args.Handled = true;
            return;
        }
        if (_pointer is ControlPointer pointer)
        {
            if (input.Pointer != pointer) return;
            if (input.Type == ControlInputEventType.PointerCancel)
            {
                FinishInteraction(true);
                args.Handled = true;
                return;
            }
            if (input.Type == ControlInputEventType.PointerMove || input.Type == ControlInputEventType.PointerUp && input.Button == _button)
            {
                if (_panning)
                {
                    ViewStart = _panStart - (input.Position.X - _origin.X) / (double)PixelsPerSecond;
                    VerticalOffset = _panVertical - (input.Position.Y - _origin.Y);
                }
                else if (_scrubbing) Time = PositionToTime(input.Position);
                else if (IsEditing) DragRange(input);
                if (input.Type == ControlInputEventType.PointerUp) FinishInteraction(false);
                args.Handled = true;
                return;
            }
        }
        if (_pointer is not null || IsEditing) return;

        if (input.Type == ControlInputEventType.PointerWheel && Contains(ContentBounds, input.Position.X, input.Position.Y))
        {
            if (input.Control) ZoomAt(input.Position, MathF.Pow(1.15f, Math.Clamp(input.WheelDelta / 120f, -16, 16)));
            else if (input.Shift) ViewStart -= input.WheelDelta / 120d * 60 / PixelsPerSecond;
            else VerticalOffset -= input.WheelDelta / 120f * TrackHeight * 3;
            args.Handled = true;
            return;
        }
        if (input.Type == ControlInputEventType.PointerDown && Contains(ContentBounds, input.Position.X, input.Position.Y))
        {
            if (input.Button is not (MouseButton.Left or MouseButton.Middle)) return;
            context.Focus(this);
            _origin = input.Position;
            _button = input.Button;
            if (input.Button == MouseButton.Middle || input.Alt)
            {
                BeginPan(context, input);
            }
            else if (input.Position.Y < _plot.Y && input.Position.X >= _plot.X && input.Position.X < _plot.Right)
            {
                IsPlaying = false;
                _scrubbing = true;
                _pointer = input.Pointer;
                context.CapturePointer(this);
                Time = PositionToTime(input.Position);
            }
            else
            {
                TimelineItemLayout? hit = null;
                Vector2 local = input.Position - new Vector2(Bounds.X, Bounds.Y);
                if (Contains(_plot, input.Position.X, input.Position.Y))
                    for (int i = _visibleItems.Count - 1; i >= 0; i--)
                        if (Contains(_visibleItems[i].Bounds, local.X, local.Y)) { hit = _visibleItems[i]; break; }
                if (hit is TimelineItemLayout block)
                {
                    Select(block.Track, block.Item);
                    if (CanEdit(block.Track, block.Item) && Tracks.Contains(block.Track) && block.Track.Items.Contains(block.Item))
                    {
                        TimelineEditKind kind = TimelineEditKind.Move;
                        if (block.Bounds.Width >= ResizeHandleWidth * 2 + 4)
                        {
                            if (block.HasStart && local.X <= block.Bounds.X + ResizeHandleWidth) kind = TimelineEditKind.ResizeStart;
                            else if (block.HasEnd && local.X >= block.Bounds.Right - ResizeHandleWidth) kind = TimelineEditKind.ResizeEnd;
                        }
                        _pointer = input.Pointer;
                        context.CapturePointer(this);
                        BeginEdit(block.Track, block.Item, kind);
                    }
                }
                else
                {
                    TimelineTrack? track = null;
                    foreach (var row in _visibleTracks)
                        if (local.Y >= row.Bounds.Y && local.Y < row.Bounds.Bottom) { track = row.Track; break; }
                    Select(track, null);
                    if (input.PointerKind == ControlPointerKind.Touch && Contains(_plot, input.Position.X, input.Position.Y)) BeginPan(context, input);
                    else if (Contains(_plot, input.Position.X, input.Position.Y)) Time = PositionToTime(input.Position);
                }
            }
            args.Handled = true;
            return;
        }
        if (input.Type != ControlInputEventType.KeyDown) return;
        if (input.Key == Keys.Space) IsPlaying = !IsPlaying;
        else if (input.Key == Keys.Home) Time = 0;
        else if (input.Key == Keys.End) Time = Duration;
        else if (input.Key is Keys.Add or Keys.OemPlus) ZoomAt(new Vector2(_plot.X + _plot.Width / 2, _plot.Y), 1.25f);
        else if (input.Key is Keys.Subtract or Keys.OemMinus) ZoomAt(new Vector2(_plot.X + _plot.Width / 2, _plot.Y), 0.8f);
        else if (input.Key is Keys.Left or Keys.Right)
        {
            double delta = (SnapInterval > 0 ? SnapInterval : 1d / PixelsPerSecond) * (input.Shift ? 10 : 1) * (input.Key == Keys.Left ? -1 : 1);
            if (_selectedTrack is { } track && _selectedItem is { } item && CanEdit(track, item))
            {
                BeginEdit(track, item, TimelineEditKind.Move);
                if (IsEditing) WriteRange(Math.Clamp(item.Start + delta, 0, Duration - item.Duration), item.Duration);
                FinishInteraction(false);
            }
            else Time = Math.Clamp(Time + delta, 0, Duration);
        }
        else if (input.Key is Keys.Up or Keys.Down) VerticalOffset += (input.Key == Keys.Up ? -1 : 1) * TrackHeight;
        else return;
        args.Handled = true;
    }

    /// <summary>Captures a background navigation gesture.</summary>
    /// <param name="context">The pointer capture context.</param>
    /// <param name="input">The starting pointer event.</param>
    private void BeginPan(ControlInputContext context, ControlInputEvent input)
    {
        _panning = true;
        _panStart = ViewStart;
        _panVertical = VerticalOffset;
        _pointer = input.Pointer;
        context.CapturePointer(this);
    }

    /// <summary>Validates bounded logical style dimensions.</summary>
    /// <param name="value">The proposed dimension.</param>
    /// <param name="minimum">The inclusive lower limit.</param>
    /// <param name="maximum">The inclusive upper limit.</param>
    /// <exception cref="ArgumentOutOfRangeException">The value is nonfinite or outside the limits.</exception>
    private static void ValidateSizeValue(float value, float minimum, float maximum)
    {
        if (!float.IsFinite(value) || value < minimum || value > maximum) throw new ArgumentOutOfRangeException(nameof(value));
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case nameof(TrackHeight):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateSizeValue(value, 12, 1024);
                    if (_styleTrackHeight.Apply(value)) NotifyPropertyChanged(TrackHeightArgs);
                    break;
                }
                case nameof(RulerHeight):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateSizeValue(value, 12, 256);
                    if (_styleRulerHeight.Apply(value)) NotifyPropertyChanged(RulerHeightArgs);
                    break;
                }
                case nameof(TrackHeaderWidth):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateSizeValue(value, 0, 4096);
                    if (_styleTrackHeaderWidth.Apply(value)) NotifyPropertyChanged(TrackHeaderWidthArgs);
                    break;
                }
                case nameof(ResizeHandleWidth):
                {
                    float value = Converters.Convert<float>("Single", entry.Value);
                    ValidateSizeValue(value, 1, 24);
                    if (_styleResizeHandleWidth.Apply(value)) NotifyPropertyChanged(ResizeHandleWidthArgs);
                    break;
                }
                case nameof(ItemBackground):
                {
                    Color value = Converters.Convert<Color>("Color", entry.Value);
                    if (_styleItemBackground.Apply(value)) NotifyPropertyChanged(ItemBackgroundArgs);
                    break;
                }
                case nameof(SelectedItemBackground):
                {
                    Color value = Converters.Convert<Color>("Color", entry.Value);
                    if (_styleSelectedItemBackground.Apply(value)) NotifyPropertyChanged(SelectedItemBackgroundArgs);
                    break;
                }
                case nameof(GridColor):
                {
                    Color value = Converters.Convert<Color>("Color", entry.Value);
                    if (_styleGridColor.Apply(value)) NotifyPropertyChanged(GridColorArgs);
                    break;
                }
                case nameof(PlayheadColor):
                {
                    Color value = Converters.Convert<Color>("Color", entry.Value);
                    if (_stylePlayheadColor.Apply(value)) NotifyPropertyChanged(PlayheadColorArgs);
                    break;
                }
            }
        }
    }

    /// <inheritdoc/>
    internal override bool IsStyleLocal(string property) => property switch
    {
        nameof(TrackHeight) => _styleTrackHeight.HasLocalValue,
        nameof(RulerHeight) => _styleRulerHeight.HasLocalValue,
        nameof(TrackHeaderWidth) => _styleTrackHeaderWidth.HasLocalValue,
        nameof(ResizeHandleWidth) => _styleResizeHandleWidth.HasLocalValue,
        nameof(ItemBackground) => _styleItemBackground.HasLocalValue,
        nameof(SelectedItemBackground) => _styleSelectedItemBackground.HasLocalValue,
        nameof(GridColor) => _styleGridColor.HasLocalValue,
        nameof(PlayheadColor) => _stylePlayheadColor.HasLocalValue,
        _ => base.IsStyleLocal(property)
    };

    /// <inheritdoc/>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case nameof(TrackHeight):
                if (_styleTrackHeight.ClearLocal()) NotifyPropertyChanged(TrackHeightArgs);
                break;
            case nameof(RulerHeight):
                if (_styleRulerHeight.ClearLocal()) NotifyPropertyChanged(RulerHeightArgs);
                break;
            case nameof(TrackHeaderWidth):
                if (_styleTrackHeaderWidth.ClearLocal()) NotifyPropertyChanged(TrackHeaderWidthArgs);
                break;
            case nameof(ResizeHandleWidth):
                if (_styleResizeHandleWidth.ClearLocal()) NotifyPropertyChanged(ResizeHandleWidthArgs);
                break;
            case nameof(ItemBackground):
                if (_styleItemBackground.ClearLocal()) NotifyPropertyChanged(ItemBackgroundArgs);
                break;
            case nameof(SelectedItemBackground):
                if (_styleSelectedItemBackground.ClearLocal()) NotifyPropertyChanged(SelectedItemBackgroundArgs);
                break;
            case nameof(GridColor):
                if (_styleGridColor.ClearLocal()) NotifyPropertyChanged(GridColorArgs);
                break;
            case nameof(PlayheadColor):
                if (_stylePlayheadColor.ClearLocal()) NotifyPropertyChanged(PlayheadColorArgs);
                break;
            default: base.ResetStyleProperty(property); break;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _styleTrackHeight.BeginStyle(36f);
        _styleRulerHeight.BeginStyle(28f);
        _styleTrackHeaderWidth.BeginStyle(140f);
        _styleResizeHandleWidth.BeginStyle(6f);
        _styleItemBackground.BeginStyle(new Color(57, 112, 165));
        _styleSelectedItemBackground.BeginStyle(new Color(48, 148, 210));
        _styleGridColor.BeginStyle(Color.Gray * 0.5f);
        _stylePlayheadColor.BeginStyle(new Color(255, 183, 55));
    }

    /// <inheritdoc/>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleTrackHeight.CommitStyle()) NotifyPropertyChanged(TrackHeightArgs);
        if (_styleRulerHeight.CommitStyle()) NotifyPropertyChanged(RulerHeightArgs);
        if (_styleTrackHeaderWidth.CommitStyle()) NotifyPropertyChanged(TrackHeaderWidthArgs);
        if (_styleResizeHandleWidth.CommitStyle()) NotifyPropertyChanged(ResizeHandleWidthArgs);
        if (_styleItemBackground.CommitStyle()) NotifyPropertyChanged(ItemBackgroundArgs);
        if (_styleSelectedItemBackground.CommitStyle()) NotifyPropertyChanged(SelectedItemBackgroundArgs);
        if (_styleGridColor.CommitStyle()) NotifyPropertyChanged(GridColorArgs);
        if (_stylePlayheadColor.CommitStyle()) NotifyPropertyChanged(PlayheadColorArgs);
    }
}
