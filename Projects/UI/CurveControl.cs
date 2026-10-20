using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Gameplay.Curves;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>CurveBase preview and editable point graph with constrained dragging, view navigation and bounded history.</summary>
public class CurveControl : Control
{
    private readonly List<CurveSample> _samples = new();
    private readonly List<CurveEditState> _undo = new(), _redo = new();
    private ChangeTracker<CurveBase?> _source;
    private ChangeTracker<Vector2> _viewMinimum, _viewMaximum;
    private ChangeTracker<int> _selection;
    private ChangeTracker<bool> _readOnly;
    /// <summary>Gets or sets the optional command notified after a completed edit.</summary>
    public System.Windows.Input.ICommand? Command { get => ConfigCommand; set => ConfigCommand = value; }
    /// <summary>Gets or sets the completion parameter; null uses the displayed CurveBase.</summary>
    public object? CommandParameter { get => ConfigCommandParameter; set => ConfigCommandParameter = value; }

    private System.ComponentModel.INotifyPropertyChanged? _observableCurve;
    private void TrackCurve()
    {
        var next = Workspace is null ? null : Curve as System.ComponentModel.INotifyPropertyChanged;
        if (ReferenceEquals(next, _observableCurve))
            return;
        if (_observableCurve is not null)
            _observableCurve.PropertyChanged -= SourceChanged;
        _observableCurve = next;
        if (next is not null)
            next.PropertyChanged += SourceChanged;
    }

    private void SourceChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args) => InvalidateMeasure();
    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();
        TrackCurve();
    }

    /// <inheritdoc/>
    protected override void OnDetached()
    {
        base.OnDetached();
        if (_observableCurve is not null)
            _observableCurve.PropertyChanged -= SourceChanged;
        _observableCurve = null;
    }

    private int _observedRevision;
    private bool _dragging, _panning;
    private ControlPointer _pointer;
    private MouseButton _button;
    private Vector2 _lastPosition, _grabOffset, _backgroundClick;
    private double _time, _lastClickTime = double.NegativeInfinity;
    private EditableCurve? _editModel;
    private CurveEditState? _before;
    private Bounds2 _plot;
    /// <summary>Creates a curve preview/editor.</summary>
    public CurveControl() : base()
    {
    }

    /// <summary>Gets the displayed source; null assignment restores the control's editable default.</summary>
    public CurveBase Curve
    {
        get
        {
            return ConfigCurve ?? ConfigDefaultCurve;
        }

        set => ConfigCurve = value;
    }

    /// <summary>Gets the editable source or null when displaying an ordinary CurveBase.</summary>
    public EditableCurve? EditableCurve => Curve as EditableCurve;

    /// <summary>Gets or sets an optional interpolation override.</summary>
    public CurveInterpolation? Interpolation
    {
        get => ConfigInterpolation;
        set
        {
            if (value is not (null or CurveInterpolation.Step or CurveInterpolation.Linear or CurveInterpolation.Monotone))
                throw new ArgumentOutOfRangeException(nameof(value));
            if (_dragging || _panning)
                EndInteraction(true, true);
            if (EditableCurve is EditableCurve model && value is CurveInterpolation mode && model.Interpolation != mode && !IsReadOnly)
            {
                var before = Snapshot(model);
                ConfigInterpolation = value;
                model.Interpolation = mode;
                Commit(model, before);
            }
            else
                ConfigInterpolation = value;
        }
    }

    /// <summary>Gets effective read-only mode; ordinary CurveBase sources are previews.</summary>
    public bool IsReadOnly { get => ConfigIsReadOnly || EditableCurve is null; set => ConfigIsReadOnly = value; }

    /// <summary>Gets or sets the selected point, or -1.</summary>
    public int SelectedIndex
    {
        get => EditableCurve is EditableCurve model && ConfigSelectedIndex >= 0 && ConfigSelectedIndex < model.Points.Count ? ConfigSelectedIndex : -1;
        set
        {
            if (value < -1 || value >= (EditableCurve?.Points.Count ?? 0))
                throw new ArgumentOutOfRangeException(nameof(value));
            ConfigSelectedIndex = value;
            NotifySelection();
        }
    }

    /// <summary>Gets the plot area in root logical coordinates.</summary>
    public Bounds2 PlotBounds => _plot;
    /// <summary>Gets whether a point is being dragged.</summary>
    public bool IsEditing => _dragging;
    /// <summary>Gets whether an undo operation is available.</summary>
    public bool CanUndo => !IsReadOnly && !_dragging && _undo.Count > 0;
    /// <summary>Gets whether a redo operation is available.</summary>
    public bool CanRedo => !IsReadOnly && !_dragging && _redo.Count > 0;
    /// <summary>Gets whether the last sampling pass found nonfinite source values.</summary>
    public bool HasInvalidSamples { get; private set; }

    /// <summary>Occurs for own model edits and externally observed source changes.</summary>
    public event EventHandler? CurveChanged;
    /// <summary>Occurs when point selection changes.</summary>
    public event EventHandler? SelectionChanged;
    /// <summary>Occurs after the view changes.</summary>
    public event EventHandler? ViewChanged;
    /// <summary>Occurs before a point drag starts.</summary>
    public event EventHandler? EditStarted;
    /// <summary>Occurs after a completed drag or discrete edit; optional Command is executed here.</summary>
    public event EventHandler? EditCompleted;
    /// <summary>Occurs when a gesture is cancelled; its own point changes are reverted.</summary>
    public event EventHandler? EditCancelled;
    /// <inheritdoc/>
    protected override string DefaultStyleName => "CurveControl";

    internal override void Prepare()
    {
        base.Prepare();
        ValidateView(ConfigCurveViewMinimum, ConfigCurveViewMaximum);
        if (!float.IsFinite(ConfigSnapX) || ConfigSnapX < 0 || !float.IsFinite(ConfigSnapY) || ConfigSnapY < 0 || !float.IsFinite(ConfigPointRadius) || ConfigPointRadius <= 0 || ConfigPointRadius > 64 || !float.IsFinite(ConfigCurveLineThickness) || ConfigCurveLineThickness <= 0 || ConfigCurveLineThickness > 64)
            throw new InvalidOperationException("Invalid curve graph sizes or snapping.");
        if (ConfigInterpolation is not (null or CurveInterpolation.Step or CurveInterpolation.Linear or CurveInterpolation.Monotone))
            throw new InvalidOperationException("Invalid curve interpolation.");
        CurveBase source = Curve;
        TrackCurve();
        bool ready = _source.WasEnsured;
        bool changed = _source.EnsureChange(source);
        if (changed)
        {
            EndInteraction(true, true);
            _source.Set(source);
            _undo.Clear();
            _redo.Clear();
            if (ready)
                ConfigSelectedIndex = -1;
            _observedRevision = EditableCurve?.Revision ?? 0;
            if (ready)
                CurveChanged?.Invoke(this, EventArgs.Empty);
        }

        if (source is EditableCurve model)
        {
            if (model.Revision != _observedRevision)
            {
                EndInteraction(true, false);
                _undo.Clear();
                _redo.Clear();
                _observedRevision = model.Revision;
                CurveChanged?.Invoke(this, EventArgs.Empty);
            }

            if (ConfigInterpolation is CurveInterpolation mode && mode != model.Interpolation)
            {
                EndInteraction(true, true);
                model.Interpolation = mode;
                _observedRevision = model.Revision;
                _undo.Clear();
                _redo.Clear();
                CurveChanged?.Invoke(this, EventArgs.Empty);
            }

            if (ConfigSelectedIndex >= model.Points.Count)
                ConfigSelectedIndex = model.Points.Count - 1;
        }
        else
            ConfigSelectedIndex = -1;
        _readOnly.EnsureChange(IsReadOnly);
        if (_dragging && _readOnly.NewValue)
            EndInteraction(true, true);
        bool viewReady = _viewMinimum.WasEnsured && _viewMaximum.WasEnsured;
        bool viewChanged = _viewMinimum.EnsureChange(ConfigCurveViewMinimum) | _viewMaximum.EnsureChange(ConfigCurveViewMaximum);
        if (viewReady && viewChanged)
            ViewChanged?.Invoke(this, EventArgs.Empty);
        NotifySelection();
    }

    private void NotifySelection()
    {
        int index = SelectedIndex;
        bool ready = _selection.WasEnsured;
        if (_selection.EnsureChange(index) && ready)
            SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private static void ValidateView(Vector2 minimum, Vector2 maximum)
    {
        double width = (double)maximum.X - minimum.X, height = (double)maximum.Y - minimum.Y;
        if (!float.IsFinite(minimum.X) || !float.IsFinite(minimum.Y) || !float.IsFinite(maximum.X) || !float.IsFinite(maximum.Y) || width < .000099 || height < .000099 || width > 1000.001 || height > 1000.001 || minimum.X < -10000 || minimum.Y < -10000 || maximum.X > 10000 || maximum.Y > 10000)
            throw new InvalidOperationException("Curve view must be finite, increasing, within ±10000, with spans 0.0001–1000.");
    }

    /// <summary>Sets a validated visible range without modifying curve data.</summary>
    public void SetView(Vector2 minimum, Vector2 maximum)
    {
        ValidateView(minimum, maximum);
        bool changed = ConfigCurveViewMinimum != minimum || ConfigCurveViewMaximum != maximum;
        ConfigCurveViewMinimum = minimum;
        ConfigCurveViewMaximum = maximum;
        _viewMinimum.Set(minimum);
        _viewMaximum.Set(maximum);
        if (changed)
            ViewChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Resets the graph to the unit square.</summary>
    public void ResetView() => SetView(Vector2.Zero, Vector2.One);
    /// <summary>Fits finite sampled values and editable points with a small margin.</summary>
    public void FitToCurve()
    {
        double low = double.PositiveInfinity, high = double.NegativeInfinity;
        for (int i = 0; i <= 256; i++)
        {
            float value = Curve.GetValue(i / 256f);
            if (float.IsFinite(value))
            {
                low = Math.Min(low, value);
                high = Math.Max(high, value);
            }
        }

        if (EditableCurve is EditableCurve model)
            foreach (var point in model.Points)
            {
                low = Math.Min(low, point.Y);
                high = Math.Max(high, point.Y);
            }

        if (!double.IsFinite(low))
            return;
        double span = Math.Max(.1, high - low), height = Math.Clamp(span * 1.2, .0001, 1000);
        double min = Math.Clamp(low - span * .1, -10000, 10000 - height);
        SetView(new Vector2(-.05f, (float)min), new Vector2(1.05f, (float)(min + height)));
    }

    /// <summary>Converts a data point to root coordinates. Call after layout.</summary>
    public Vector2 ToScreen(CurvePoint point)
    {
        var min = ConfigCurveViewMinimum;
        var max = ConfigCurveViewMaximum;
        return new Vector2((float)(_plot.X + ((double)point.X - min.X) / (max.X - min.X) * _plot.Width), (float)(_plot.Bottom - ((double)point.Y - min.Y) / (max.Y - min.Y) * _plot.Height));
    }

    /// <summary>Converts root coordinates to graph coordinates. Call after layout.</summary>
    public CurvePoint FromScreen(Vector2 position)
    {
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
            throw new ArgumentOutOfRangeException(nameof(position));
        var min = ConfigCurveViewMinimum;
        var max = ConfigCurveViewMaximum;
        if (_plot.Width <= 0 || _plot.Height <= 0)
            return new(min.X, min.Y);
        return new CurvePoint((float)(min.X + ((double)position.X - _plot.X) / _plot.Width * (max.X - min.X)), (float)(min.Y + ((double)_plot.Bottom - position.Y) / _plot.Height * (max.Y - min.Y)));
    }

    private CurveEditState Snapshot(EditableCurve model) => new(model.Points.ToArray(), model.Interpolation, ConfigInterpolation, IsStyleLocal("Interpolation"));
    private void Remember(CurveEditState state)
    {
        if (_undo.Count == 64)
            _undo.RemoveAt(0);
        _undo.Add(state);
        _redo.Clear();
    }

    private void Changed(EditableCurve model)
    {
        _source.Set(model);
        _observedRevision = model.Revision;
        CurveChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Completed()
    {
        EditCompleted?.Invoke(this, EventArgs.Empty);
        var command = Command;
        object? parameter = CommandParameter ?? Curve;
        if (command?.CanExecute(parameter) == true)
            command.Execute(parameter);
    }

    private void Commit(EditableCurve model, CurveEditState before)
    {
        Remember(before);
        Changed(model);
        Completed();
    }

    /// <summary>Adds an interior point as one undoable edit.</summary>
    public int AddPoint(CurvePoint point)
    {
        if (IsReadOnly)
            throw new InvalidOperationException("This curve is read-only.");
        if (_dragging || _panning)
            EndInteraction(true, true);
        var model = EditableCurve!;
        var before = Snapshot(model);
        int index = model.AddPoint(point);
        SelectedIndex = index;
        Commit(model, before);
        return index;
    }

    private CurvePoint ConstrainPoint(EditableCurve model, int index, CurvePoint point, bool snap)
    {
        if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
            throw new ArgumentOutOfRangeException(nameof(point));
        float x = point.X, y = point.Y;
        if (snap)
        {
            if (ConfigSnapX > 0)
                x = (float)(Math.Round((double)x / ConfigSnapX) * ConfigSnapX);
            if (ConfigSnapY > 0)
                y = (float)(Math.Round((double)y / ConfigSnapY) * ConfigSnapY);
        }

        if (index == 0)
            x = 0;
        else if (index == model.Points.Count - 1)
            x = 1;
        else
        {
            float low = model.Points[index - 1].X + EditableCurve.MinimumSpacing, high = model.Points[index + 1].X - EditableCurve.MinimumSpacing;
            x = low > high ? model.Points[index].X : Math.Clamp(x, low, high);
        }

        y = Math.Clamp(y, ConfigCurveViewMinimum.Y, ConfigCurveViewMaximum.Y);
        return new(x, y);
    }

    /// <summary>Moves the selected point as one undoable edit, preserving point order.</summary>
    public bool MoveSelectedPoint(CurvePoint point, bool snap = false)
    {
        if (IsReadOnly || SelectedIndex < 0)
            return false;
        if (_dragging || _panning)
            EndInteraction(true, true);
        var model = EditableCurve!;
        var next = ConstrainPoint(model, SelectedIndex, point, snap);
        if (next == model.Points[SelectedIndex])
            return false;
        var before = Snapshot(model);
        model.SetPoint(SelectedIndex, next);
        Commit(model, before);
        return true;
    }

    /// <summary>Removes the selected interior point as one undoable edit.</summary>
    public bool RemoveSelectedPoint()
    {
        if (IsReadOnly || SelectedIndex <= 0 || SelectedIndex >= EditableCurve!.Points.Count - 1)
            return false;
        if (_dragging || _panning)
            EndInteraction(true, true);
        var model = EditableCurve!;
        var before = Snapshot(model);
        int index = SelectedIndex;
        model.RemovePoint(index);
        SelectedIndex = Math.Min(index, model.Points.Count - 1);
        Commit(model, before);
        return true;
    }

    private void Restore(EditableCurve model, CurveEditState state)
    {
        model.ReplacePoints(state.Points);
        model.Interpolation = state.Interpolation;
        ConfigInterpolation = state.Override;
        if (!state.HasLocalOverride)
            ResetStyleProperty("Interpolation");
        _observedRevision = model.Revision;
        ConfigSelectedIndex = Math.Clamp(ConfigSelectedIndex, -1, model.Points.Count - 1);
    }

    /// <summary>Undoes the most recent completed editor change.</summary>
    public void Undo()
    {
        if (!CanUndo)
            return;
        var model = EditableCurve!;
        var state = _undo[^1];
        _undo.RemoveAt(_undo.Count - 1);
        _redo.Add(Snapshot(model));
        Restore(model, state);
        Changed(model);
        NotifySelection();
        Completed();
    }

    /// <summary>Reapplies an undone editor change.</summary>
    public void Redo()
    {
        if (!CanRedo)
            return;
        var model = EditableCurve!;
        var state = _redo[^1];
        _redo.RemoveAt(_redo.Count - 1);
        _undo.Add(Snapshot(model));
        Restore(model, state);
        Changed(model);
        NotifySelection();
        Completed();
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available) => new(MathF.Min(340, available.Width), MathF.Min(240, available.Height));
    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float left = MathF.Min(44, b.Width), bottom = MathF.Min(28, b.Height);
        _plot = new Bounds2(b.X + left, b.Y + 8, MathF.Max(0, b.Width - left - 8), MathF.Max(0, b.Height - bottom - 8));
        BuildSamples();
    }

    private void BuildSamples()
    {
        _samples.Clear();
        HasInvalidSamples = false;
        if (_plot.Width <= 0 || _plot.Height <= 0)
            return;
        double left = Math.Max(0, ConfigCurveViewMinimum.X), right = Math.Min(1, ConfigCurveViewMaximum.X);
        if (left > right)
            return;
        var source = Curve;
        if (source is EditableCurve model && model.Interpolation is CurveInterpolation.Step or CurveInterpolation.Linear)
        {
            AddSample(left, source.GetValue((float)left));
            for (int i = 1; i < model.Points.Count; i++)
            {
                var point = model.Points[i];
                if (point.X <= left || point.X > right)
                    continue;
                if (model.Interpolation == CurveInterpolation.Step)
                    AddSample(point.X, model.Points[i - 1].Y);
                AddSample(point.X, point.Y);
            }

            AddSample(right, source.GetValue((float)right));
            return;
        }

        int count = (int)Math.Clamp(Math.Ceiling(_plot.Width), 32, 1024);
        for (int i = 0; i <= count; i++)
        {
            double x = left + (right - left) * i / count;
            AddSample(x, source.GetValue((float)x));
        }
    }

    private void AddSample(double x, float y)
    {
        bool valid = float.IsFinite(y);
        HasInvalidSamples |= !valid;
        _samples.Add(new CurveSample(x, valid ? y : 0, valid));
    }

    private int HitPoint(Vector2 position)
    {
        if (!ConfigShowPoints || EditableCurve is not EditableCurve model)
            return -1;
        float limit = ConfigPointRadius + 5, best = limit * limit;
        int result = -1;
        for (int i = 0; i < model.Points.Count; i++)
        {
            Vector2 p = ToScreen(model.Points[i]);
            if (!Contains(_plot, p.X, p.Y) && p.X != _plot.Right && p.Y != _plot.Bottom)
                continue;
            float distance = Vector2.DistanceSquared(p, position);
            if (distance <= best)
            {
                best = distance;
                result = i;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var e = args.Input;
        if (e.Type == ControlInputEventType.KeyDown && e.Key == Keys.Escape && (_dragging || _panning))
        {
            EndInteraction(true, true);
            args.Handled = true;
            return;
        }

        if ((_dragging || _panning) && e.Pointer == _pointer)
        {
            if (e.Type == ControlInputEventType.PointerMove)
            {
                if (_panning)
                {
                    Pan(e.Position - _lastPosition);
                    _lastPosition = e.Position;
                }
                else if (_editModel is EditableCurve model && SelectedIndex >= 0)
                {
                    var next = ConstrainPoint(model, SelectedIndex, FromScreen(e.Position - _grabOffset), e.Shift);
                    int revision = model.Revision;
                    model.SetPoint(SelectedIndex, next);
                    if (revision != model.Revision)
                        Changed(model);
                }

                args.Handled = true;
                return;
            }

            if (e.Type == ControlInputEventType.PointerUp && e.Button == _button)
            {
                if (_dragging && _editModel is EditableCurve model && SelectedIndex >= 0)
                {
                    int revision = model.Revision;
                    model.SetPoint(SelectedIndex, ConstrainPoint(model, SelectedIndex, FromScreen(e.Position - _grabOffset), e.Shift));
                    if (revision != model.Revision)
                        Changed(model);
                }

                EndInteraction(false, true);
                args.Handled = true;
                return;
            }
        }

        if (_dragging || _panning)
            return;
        if (e.Type == ControlInputEventType.PointerWheel && Contains(_plot, e.Position.X, e.Position.Y))
        {
            Zoom(e.Position, e.WheelDelta);
            args.Handled = true;
            return;
        }

        if (e.Type == ControlInputEventType.PointerDown && Contains(new Bounds2(_plot.X - ConfigPointRadius - 5, _plot.Y - ConfigPointRadius - 5, _plot.Width + ConfigPointRadius * 2 + 10, _plot.Height + ConfigPointRadius * 2 + 10), e.Position.X, e.Position.Y))
        {
            if (e.Button == MouseButton.Middle || e.Button == MouseButton.Left && e.Alt)
            {
                _panning = true;
                _pointer = e.Pointer;
                _button = e.Button;
                _lastPosition = e.Position;
                context.CapturePointer(this);
                args.Handled = true;
                return;
            }

            int hit = HitPoint(e.Position);
            if (hit < 0 && !Contains(_plot, e.Position.X, e.Position.Y))
                return;
            if (e.Button == MouseButton.Right && hit >= 0)
            {
                SelectedIndex = hit;
                if (!IsReadOnly)
                    RemoveSelectedPoint();
                args.Handled = true;
                return;
            }

            if (e.Button != MouseButton.Left)
                return;
            if (hit >= 0)
            {
                SelectedIndex = hit;
                if (!IsReadOnly)
                {
                    _editModel = EditableCurve!;
                    _before = Snapshot(_editModel);
                    _grabOffset = e.Position - ToScreen(_editModel.Points[hit]);
                    _pointer = e.Pointer;
                    _button = e.Button;
                    _dragging = true;
                    context.CapturePointer(this);
                    EditStarted?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                SelectedIndex = -1;
                bool doubleClick = _time - _lastClickTime <= .35 && Vector2.DistanceSquared(e.Position, _backgroundClick) <= 64;
                if (!IsReadOnly && (e.Control || doubleClick))
                {
                    var point = FromScreen(e.Position);
                    if (point.X > 0 && point.X < 1 && EditableCurve!.Points.All(p => Math.Abs((double)p.X - point.X) >= EditableCurve.MinimumSpacing))
                        AddPoint(point);
                    _lastClickTime = double.NegativeInfinity;
                }
                else
                {
                    _backgroundClick = e.Position;
                    _lastClickTime = _time;
                }
            }

            args.Handled = true;
            return;
        }

        if (e.Type != ControlInputEventType.KeyDown)
            return;
        if (e.Key == Keys.Home)
        {
            ResetView();
            args.Handled = true;
            return;
        }

        if (e.Key == Keys.F)
        {
            FitToCurve();
            args.Handled = true;
            return;
        }

        if (IsReadOnly)
            return;
        if (e.Control && e.Key == Keys.Z)
        {
            Undo();
            args.Handled = true;
            return;
        }

        if (e.Control && e.Key == Keys.Y)
        {
            Redo();
            args.Handled = true;
            return;
        }

        if (e.Key is Keys.Delete or Keys.Back)
        {
            RemoveSelectedPoint();
            args.Handled = true;
            return;
        }

        if (e.Key == Keys.Insert)
        {
            float x = Math.Clamp((ConfigCurveViewMinimum.X + ConfigCurveViewMaximum.X) / 2, .0001f, .9999f);
            if (EditableCurve!.Points.All(p => Math.Abs((double)p.X - x) >= EditableCurve.MinimumSpacing))
                AddPoint(new(x, Curve.GetValue(x)));
            args.Handled = true;
            return;
        }

        if (SelectedIndex >= 0 && e.Key is Keys.Left or Keys.Right or Keys.Up or Keys.Down)
        {
            float step = e.Shift ? .1f : .01f;
            var point = EditableCurve!.Points[SelectedIndex];
            MoveSelectedPoint(new(point.X + (e.Key == Keys.Left ? -step : e.Key == Keys.Right ? step : 0), point.Y + (e.Key == Keys.Down ? -step : e.Key == Keys.Up ? step : 0)));
            args.Handled = true;
        }
    }

    private void EndInteraction(bool cancelled, bool restore)
    {
        if (!_dragging && !_panning)
            return;
        bool editing = _dragging;
        _dragging = false;
        _panning = false;
        Workspace?.ReleasePointer(this, _pointer);
        if (editing && _editModel is EditableCurve model && _before is CurveEditState before)
        {
            bool changed = model.Revision != _observedRevision || !model.Points.SequenceEqual(before.Points) || model.Interpolation != before.Interpolation;
            if (cancelled)
            {
                if (restore && model.Revision == _observedRevision && changed)
                {
                    model.ReplacePoints(before.Points);
                    model.Interpolation = before.Interpolation;
                    _observedRevision = model.Revision;
                    if (ReferenceEquals(Curve, model))
                        Changed(model);
                }

                EditCancelled?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (changed)
                    Remember(before);
                Completed();
            }
        }

        _editModel = null;
        _before = null;
    }

    private void Pan(Vector2 pixels)
    {
        if (_plot.Width <= 0 || _plot.Height <= 0)
            return;
        var min = ConfigCurveViewMinimum;
        var max = ConfigCurveViewMaximum;
        ApplyNavigation(min.X - (double)pixels.X / _plot.Width * (max.X - min.X), min.Y + (double)pixels.Y / _plot.Height * (max.Y - min.Y), max.X - min.X, max.Y - min.Y);
    }

    private void Zoom(Vector2 position, int wheel)
    {
        var min = ConfigCurveViewMinimum;
        var max = ConfigCurveViewMaximum;
        var anchor = FromScreen(position);
        double factor = Math.Pow(1.2, -Math.Clamp(wheel / 120.0, -20, 20));
        double width = Math.Clamp((max.X - min.X) * factor, .001, 1000), height = Math.Clamp((max.Y - min.Y) * factor, .001, 1000);
        double x = anchor.X - (position.X - _plot.X) / _plot.Width * width, y = anchor.Y - (_plot.Bottom - position.Y) / _plot.Height * height;
        ApplyNavigation(x, y, width, height);
    }

    private void ApplyNavigation(double x, double y, double width, double height)
    {
        x = Math.Clamp(x, -10000, 10000 - width);
        y = Math.Clamp(y, -10000, 10000 - height);
        SetView(new((float)x, (float)y), new((float)(x + width), (float)(y + height)));
    }

    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed) => _time += elapsed.TotalSeconds;
    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => EndInteraction(true, true);
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        CurveControlRenderer.Draw(this, context, _samples);
    }

    private StyleProperty<bool> _styleIsReadOnly = new(false);
    private static readonly System.ComponentModel.PropertyChangedEventArgs IsReadOnlyChangedArgs = new(nameof(IsReadOnly));
    internal bool ConfigIsReadOnly
    {
        get => _styleIsReadOnly.Value;
        set
        {
            if (_styleIsReadOnly.SetLocal(value))
                NotifyPropertyChanged(IsReadOnlyChangedArgs);
        }
    }

    private StyleProperty<int> _styleSelectedIndex = new(-1);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SelectedIndexChangedArgs = new(nameof(SelectedIndex));
    internal int ConfigSelectedIndex
    {
        get => _styleSelectedIndex.Value;
        set
        {
            if (_styleSelectedIndex.SetLocal(value))
                NotifyPropertyChanged(SelectedIndexChangedArgs);
        }
    }

    private StyleProperty<System.Windows.Input.ICommand?> _styleCommand = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CommandChangedArgs = new(nameof(Command));
    internal System.Windows.Input.ICommand? ConfigCommand
    {
        get => _styleCommand.Value;
        set
        {
            if (_styleCommand.SetLocal(value))
                NotifyPropertyChanged(CommandChangedArgs);
        }
    }

    private StyleProperty<object?> _styleCommandParameter = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CommandParameterChangedArgs = new(nameof(CommandParameter));
    internal object? ConfigCommandParameter
    {
        get => _styleCommandParameter.Value;
        set
        {
            if (_styleCommandParameter.SetLocal(value))
                NotifyPropertyChanged(CommandParameterChangedArgs);
        }
    }

    private StyleProperty<Sachssoft.Engine.Gameplay.Curves.CurveBase?> _styleCurve = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CurveChangedArgs = new(nameof(Curve));
    internal Sachssoft.Engine.Gameplay.Curves.CurveBase? ConfigCurve
    {
        get => _styleCurve.Value;
        set
        {
            if (_styleCurve.SetLocal(value))
                NotifyPropertyChanged(CurveChangedArgs);
        }
    }

    private StyleProperty<CurveInterpolation?> _styleInterpolation = new(null);
    private static readonly System.ComponentModel.PropertyChangedEventArgs InterpolationChangedArgs = new(nameof(Interpolation));
    internal CurveInterpolation? ConfigInterpolation
    {
        get => _styleInterpolation.Value;
        set
        {
            if (_styleInterpolation.SetLocal(value))
                NotifyPropertyChanged(InterpolationChangedArgs);
        }
    }

    private StyleProperty<bool> _styleShowGrid = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShowGridChangedArgs = new(nameof(ShowGrid));
    internal bool ConfigShowGrid
    {
        get => _styleShowGrid.Value;
        set
        {
            if (_styleShowGrid.SetLocal(value))
                NotifyPropertyChanged(ShowGridChangedArgs);
        }
    }

    /// <summary>Gets or sets ShowGrid; a local assignment overrides the style.</summary>
    public bool ShowGrid { get => ConfigShowGrid; set => ConfigShowGrid = value; }

    private StyleProperty<bool> _styleShowPoints = new(true);
    private static readonly System.ComponentModel.PropertyChangedEventArgs ShowPointsChangedArgs = new(nameof(ShowPoints));
    internal bool ConfigShowPoints
    {
        get => _styleShowPoints.Value;
        set
        {
            if (_styleShowPoints.SetLocal(value))
                NotifyPropertyChanged(ShowPointsChangedArgs);
        }
    }

    /// <summary>Gets or sets ShowPoints; a local assignment overrides the style.</summary>
    public bool ShowPoints { get => ConfigShowPoints; set => ConfigShowPoints = value; }

    private StyleProperty<float> _styleSnapX = new(0.05f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SnapXChangedArgs = new(nameof(SnapX));
    internal float ConfigSnapX
    {
        get => _styleSnapX.Value;
        set
        {
            if (_styleSnapX.SetLocal(value))
                NotifyPropertyChanged(SnapXChangedArgs);
        }
    }

    /// <summary>Gets or sets SnapX; a local assignment overrides the style.</summary>
    public float SnapX { get => ConfigSnapX; set => ConfigSnapX = value; }

    private StyleProperty<float> _styleSnapY = new(0.05f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs SnapYChangedArgs = new(nameof(SnapY));
    internal float ConfigSnapY
    {
        get => _styleSnapY.Value;
        set
        {
            if (_styleSnapY.SetLocal(value))
                NotifyPropertyChanged(SnapYChangedArgs);
        }
    }

    /// <summary>Gets or sets SnapY; a local assignment overrides the style.</summary>
    public float SnapY { get => ConfigSnapY; set => ConfigSnapY = value; }

    private StyleProperty<float> _styleCurveLineThickness = new(2f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CurveLineThicknessChangedArgs = new(nameof(CurveLineThickness));
    internal float ConfigCurveLineThickness
    {
        get => _styleCurveLineThickness.Value;
        set
        {
            if (_styleCurveLineThickness.SetLocal(value))
                NotifyPropertyChanged(CurveLineThicknessChangedArgs);
        }
    }

    /// <summary>Gets or sets CurveLineThickness; a local assignment overrides the style.</summary>
    public float CurveLineThickness { get => ConfigCurveLineThickness; set => ConfigCurveLineThickness = value; }

    private StyleProperty<float> _stylePointRadius = new(5f);
    private static readonly System.ComponentModel.PropertyChangedEventArgs PointRadiusChangedArgs = new(nameof(PointRadius));
    internal float ConfigPointRadius
    {
        get => _stylePointRadius.Value;
        set
        {
            if (_stylePointRadius.SetLocal(value))
                NotifyPropertyChanged(PointRadiusChangedArgs);
        }
    }

    /// <summary>Gets or sets PointRadius; a local assignment overrides the style.</summary>
    public float PointRadius { get => ConfigPointRadius; set => ConfigPointRadius = value; }

    private StyleProperty<Color> _styleCurveColor = new(new Color(65, 199, 223));
    private static readonly System.ComponentModel.PropertyChangedEventArgs CurveColorChangedArgs = new(nameof(CurveColor));
    internal Color ConfigCurveColor
    {
        get => _styleCurveColor.Value;
        set
        {
            if (_styleCurveColor.SetLocal(value))
                NotifyPropertyChanged(CurveColorChangedArgs);
        }
    }

    /// <summary>Gets or sets CurveColor; a local assignment overrides the style.</summary>
    public Color CurveColor { get => ConfigCurveColor; set => ConfigCurveColor = value; }

    private StyleProperty<Color> _stylePointColor = new(new Color(227, 234, 244));
    private static readonly System.ComponentModel.PropertyChangedEventArgs PointColorChangedArgs = new(nameof(PointColor));
    internal Color ConfigPointColor
    {
        get => _stylePointColor.Value;
        set
        {
            if (_stylePointColor.SetLocal(value))
                NotifyPropertyChanged(PointColorChangedArgs);
        }
    }

    /// <summary>Gets or sets PointColor; a local assignment overrides the style.</summary>
    public Color PointColor { get => ConfigPointColor; set => ConfigPointColor = value; }

    private StyleProperty<Vector2> _styleCurveViewMinimum = new(Vector2.Zero);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CurveViewMinimumChangedArgs = new(nameof(CurveViewMinimum));
    internal Vector2 ConfigCurveViewMinimum
    {
        get => _styleCurveViewMinimum.Value;
        set
        {
            if (_styleCurveViewMinimum.SetLocal(value))
                NotifyPropertyChanged(CurveViewMinimumChangedArgs);
        }
    }

    /// <summary>Gets or sets CurveViewMinimum; a local assignment overrides the style.</summary>
    public Vector2 CurveViewMinimum { get => ConfigCurveViewMinimum; set => ConfigCurveViewMinimum = value; }

    private StyleProperty<Vector2> _styleCurveViewMaximum = new(Vector2.One);
    private static readonly System.ComponentModel.PropertyChangedEventArgs CurveViewMaximumChangedArgs = new(nameof(CurveViewMaximum));
    internal Vector2 ConfigCurveViewMaximum
    {
        get => _styleCurveViewMaximum.Value;
        set
        {
            if (_styleCurveViewMaximum.SetLocal(value))
                NotifyPropertyChanged(CurveViewMaximumChangedArgs);
        }
    }

    /// <summary>Gets or sets CurveViewMaximum; a local assignment overrides the style.</summary>
    public Vector2 CurveViewMaximum { get => ConfigCurveViewMaximum; set => ConfigCurveViewMaximum = value; }
    internal EditableCurve ConfigDefaultCurve { get; } = new();
    /// <summary>Gets or sets the value for DefaultCurve.</summary>
    public EditableCurve DefaultCurve { get => ConfigDefaultCurve; }

    /// <summary>Applies this control's explicit style values without replacing local assignments.</summary>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        foreach (var entry in style.Values)
        {
            switch (entry.Key)
            {
                case "GridColor":
                    if (_gridColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(GridColorChangedArgs);
                    break;
                case "IsReadOnly":
                    if (_styleIsReadOnly.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(IsReadOnlyChangedArgs);
                    break;
                case "SelectedIndex":
                    if (_styleSelectedIndex.Apply(Converters.Convert<int>("Int32", entry.Value)))
                        NotifyPropertyChanged(SelectedIndexChangedArgs);
                    break;
                case "Command":
                    if (_styleCommand.Apply(Converters.Convert<System.Windows.Input.ICommand?>("Command", entry.Value)))
                        NotifyPropertyChanged(CommandChangedArgs);
                    break;
                case "CommandParameter":
                    if (_styleCommandParameter.Apply(Converters.Convert<object?>("CommandParameter", entry.Value)))
                        NotifyPropertyChanged(CommandParameterChangedArgs);
                    break;
                case "Curve":
                    if (_styleCurve.Apply(Converters.Convert<Sachssoft.Engine.Gameplay.Curves.CurveBase?>("Curve", entry.Value)))
                        NotifyPropertyChanged(CurveChangedArgs);
                    break;
                case "Interpolation":
                    if (_styleInterpolation.Apply(Converters.Convert<CurveInterpolation?>("NullableCurveInterpolation", entry.Value)))
                        NotifyPropertyChanged(InterpolationChangedArgs);
                    break;
                case "ShowGrid":
                    if (_styleShowGrid.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(ShowGridChangedArgs);
                    break;
                case "ShowPoints":
                    if (_styleShowPoints.Apply(Converters.Convert<bool>("Boolean", entry.Value)))
                        NotifyPropertyChanged(ShowPointsChangedArgs);
                    break;
                case "SnapX":
                    if (_styleSnapX.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(SnapXChangedArgs);
                    break;
                case "SnapY":
                    if (_styleSnapY.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(SnapYChangedArgs);
                    break;
                case "CurveLineThickness":
                    if (_styleCurveLineThickness.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(CurveLineThicknessChangedArgs);
                    break;
                case "PointRadius":
                    if (_stylePointRadius.Apply(Converters.Convert<float>("Single", entry.Value)))
                        NotifyPropertyChanged(PointRadiusChangedArgs);
                    break;
                case "CurveColor":
                    if (_styleCurveColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(CurveColorChangedArgs);
                    break;
                case "PointColor":
                    if (_stylePointColor.Apply(Converters.Convert<Color>("Color", entry.Value)))
                        NotifyPropertyChanged(PointColorChangedArgs);
                    break;
                case "CurveViewMinimum":
                    if (_styleCurveViewMinimum.Apply(Converters.Convert<Vector2>("Vector2", entry.Value)))
                        NotifyPropertyChanged(CurveViewMinimumChangedArgs);
                    break;
                case "CurveViewMaximum":
                    if (_styleCurveViewMaximum.Apply(Converters.Convert<Vector2>("Vector2", entry.Value)))
                        NotifyPropertyChanged(CurveViewMaximumChangedArgs);
                    break;
            }
        }
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            case "IsReadOnly":
                return _styleIsReadOnly.HasLocalValue;
            case "SelectedIndex":
                return _styleSelectedIndex.HasLocalValue;
            case "Command":
                return _styleCommand.HasLocalValue;
            case "CommandParameter":
                return _styleCommandParameter.HasLocalValue;
            case "Curve":
                return _styleCurve.HasLocalValue;
            case "Interpolation":
                return _styleInterpolation.HasLocalValue;
            case "ShowGrid":
                return _styleShowGrid.HasLocalValue;
            case "ShowPoints":
                return _styleShowPoints.HasLocalValue;
            case "SnapX":
                return _styleSnapX.HasLocalValue;
            case "SnapY":
                return _styleSnapY.HasLocalValue;
            case "CurveLineThickness":
                return _styleCurveLineThickness.HasLocalValue;
            case "PointRadius":
                return _stylePointRadius.HasLocalValue;
            case "CurveColor":
                return _styleCurveColor.HasLocalValue;
            case "PointColor":
                return _stylePointColor.HasLocalValue;
            case "CurveViewMinimum":
                return _styleCurveViewMinimum.HasLocalValue;
            case "CurveViewMaximum":
                return _styleCurveViewMaximum.HasLocalValue;
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            case "IsReadOnly":
                if (_styleIsReadOnly.ClearLocal())
                    NotifyPropertyChanged(IsReadOnlyChangedArgs);
                return;
            case "SelectedIndex":
                if (_styleSelectedIndex.ClearLocal())
                    NotifyPropertyChanged(SelectedIndexChangedArgs);
                return;
            case "Command":
                if (_styleCommand.ClearLocal())
                    NotifyPropertyChanged(CommandChangedArgs);
                return;
            case "CommandParameter":
                if (_styleCommandParameter.ClearLocal())
                    NotifyPropertyChanged(CommandParameterChangedArgs);
                return;
            case "Curve":
                if (_styleCurve.ClearLocal())
                    NotifyPropertyChanged(CurveChangedArgs);
                return;
            case "Interpolation":
                if (_styleInterpolation.ClearLocal())
                    NotifyPropertyChanged(InterpolationChangedArgs);
                return;
            case "ShowGrid":
                if (_styleShowGrid.ClearLocal())
                    NotifyPropertyChanged(ShowGridChangedArgs);
                return;
            case "ShowPoints":
                if (_styleShowPoints.ClearLocal())
                    NotifyPropertyChanged(ShowPointsChangedArgs);
                return;
            case "SnapX":
                if (_styleSnapX.ClearLocal())
                    NotifyPropertyChanged(SnapXChangedArgs);
                return;
            case "SnapY":
                if (_styleSnapY.ClearLocal())
                    NotifyPropertyChanged(SnapYChangedArgs);
                return;
            case "CurveLineThickness":
                if (_styleCurveLineThickness.ClearLocal())
                    NotifyPropertyChanged(CurveLineThicknessChangedArgs);
                return;
            case "PointRadius":
                if (_stylePointRadius.ClearLocal())
                    NotifyPropertyChanged(PointRadiusChangedArgs);
                return;
            case "CurveColor":
                if (_styleCurveColor.ClearLocal())
                    NotifyPropertyChanged(CurveColorChangedArgs);
                return;
            case "PointColor":
                if (_stylePointColor.ClearLocal())
                    NotifyPropertyChanged(PointColorChangedArgs);
                return;
            case "CurveViewMinimum":
                if (_styleCurveViewMinimum.ClearLocal())
                    NotifyPropertyChanged(CurveViewMinimumChangedArgs);
                return;
            case "CurveViewMaximum":
                if (_styleCurveViewMaximum.ClearLocal())
                    NotifyPropertyChanged(CurveViewMaximumChangedArgs);
                return;
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _gridColor.BeginStyle(new Color(43, 57, 76));
        if (_styleIsReadOnly.BeginStyle(false))
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleSelectedIndex.BeginStyle(-1))
            NotifyPropertyChanged(SelectedIndexChangedArgs);
        if (_styleCommand.BeginStyle(null))
            NotifyPropertyChanged(CommandChangedArgs);
        if (_styleCommandParameter.BeginStyle(null))
            NotifyPropertyChanged(CommandParameterChangedArgs);
        if (_styleCurve.BeginStyle(null))
            NotifyPropertyChanged(CurveChangedArgs);
        if (_styleInterpolation.BeginStyle(null))
            NotifyPropertyChanged(InterpolationChangedArgs);
        if (_styleShowGrid.BeginStyle(true))
            NotifyPropertyChanged(ShowGridChangedArgs);
        if (_styleShowPoints.BeginStyle(true))
            NotifyPropertyChanged(ShowPointsChangedArgs);
        if (_styleSnapX.BeginStyle(0.05f))
            NotifyPropertyChanged(SnapXChangedArgs);
        if (_styleSnapY.BeginStyle(0.05f))
            NotifyPropertyChanged(SnapYChangedArgs);
        if (_styleCurveLineThickness.BeginStyle(2f))
            NotifyPropertyChanged(CurveLineThicknessChangedArgs);
        if (_stylePointRadius.BeginStyle(5f))
            NotifyPropertyChanged(PointRadiusChangedArgs);
        if (_styleCurveColor.BeginStyle(new Color(65, 199, 223)))
            NotifyPropertyChanged(CurveColorChangedArgs);
        if (_stylePointColor.BeginStyle(new Color(227, 234, 244)))
            NotifyPropertyChanged(PointColorChangedArgs);
        if (_styleCurveViewMinimum.BeginStyle(Vector2.Zero))
            NotifyPropertyChanged(CurveViewMinimumChangedArgs);
        if (_styleCurveViewMaximum.BeginStyle(Vector2.One))
            NotifyPropertyChanged(CurveViewMaximumChangedArgs);
    }

    private StyleProperty<Color> _gridColor = new(new Color(43, 57, 76));
    private static readonly System.ComponentModel.PropertyChangedEventArgs GridColorChangedArgs = new(nameof(GridColor));
    internal Color ConfigGridColor
    {
        get => _gridColor.Value;
        set
        {
            if (_gridColor.SetLocal(value))
                NotifyPropertyChanged(GridColorChangedArgs);
        }
    }

    /// <summary>Gets or sets the curve grid color.</summary>
    public Color GridColor { get => ConfigGridColor; set => ConfigGridColor = value; }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_styleIsReadOnly.CommitStyle())
            NotifyPropertyChanged(IsReadOnlyChangedArgs);
        if (_styleSelectedIndex.CommitStyle())
            NotifyPropertyChanged(SelectedIndexChangedArgs);
        if (_styleCommand.CommitStyle())
            NotifyPropertyChanged(CommandChangedArgs);
        if (_styleCommandParameter.CommitStyle())
            NotifyPropertyChanged(CommandParameterChangedArgs);
        if (_styleCurve.CommitStyle())
            NotifyPropertyChanged(CurveChangedArgs);
        if (_styleInterpolation.CommitStyle())
            NotifyPropertyChanged(InterpolationChangedArgs);
        if (_styleShowGrid.CommitStyle())
            NotifyPropertyChanged(ShowGridChangedArgs);
        if (_styleShowPoints.CommitStyle())
            NotifyPropertyChanged(ShowPointsChangedArgs);
        if (_styleSnapX.CommitStyle())
            NotifyPropertyChanged(SnapXChangedArgs);
        if (_styleSnapY.CommitStyle())
            NotifyPropertyChanged(SnapYChangedArgs);
        if (_styleCurveLineThickness.CommitStyle())
            NotifyPropertyChanged(CurveLineThicknessChangedArgs);
        if (_stylePointRadius.CommitStyle())
            NotifyPropertyChanged(PointRadiusChangedArgs);
        if (_styleCurveColor.CommitStyle())
            NotifyPropertyChanged(CurveColorChangedArgs);
        if (_stylePointColor.CommitStyle())
            NotifyPropertyChanged(PointColorChangedArgs);
        if (_styleCurveViewMinimum.CommitStyle())
            NotifyPropertyChanged(CurveViewMinimumChangedArgs);
        if (_styleCurveViewMaximum.CommitStyle())
            NotifyPropertyChanged(CurveViewMaximumChangedArgs);
        if (_gridColor.CommitStyle())
            NotifyPropertyChanged(GridColorChangedArgs);
    }
}
