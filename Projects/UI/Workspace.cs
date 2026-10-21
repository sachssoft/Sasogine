using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.ComponentModel;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls;
/// <summary>Owns one deterministic control tree, input routing, focus and frame boundaries.</summary>
public sealed class Workspace : Element, IDisposable
{
    private float _dpi = 96f;
    private float _frameDpi = 96f;
    private Rectangle? _frameViewport;
    private static readonly PropertyChangedEventArgs DpiArgs = new(nameof(Dpi));
    private static readonly PropertyChangedEventArgs DpiScaleArgs = new(nameof(DpiScale));
    /// <summary>Gets or sets pixel density in dots per inch; 96 means 100 percent. Changes apply on the next input frame.</summary>
    public float Dpi
    {
        get => _dpi;
        set
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!float.IsFinite(value) || value <= 0 || value / 96f <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), "DPI must be finite and positive.");
            if (_drawing)
                throw new InvalidOperationException("Drawing cannot change workspace DPI.");
            if (SetNotify(ref _dpi, value, DpiArgs))
                NotifyPropertyChanged(DpiScaleArgs);
        }
    }

    /// <summary>Gets the requested physical pixels per logical UI unit.</summary>
    public float DpiScale => _dpi / 96f;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(PropertyChangedEventArgs notification)
    {
        if (notification.PropertyName == nameof(Dpi))
            _layoutDirty = true;
        base.OnPropertyChanged(notification);
    }

    /// <summary>Gets or sets the application's optional viewport color source.</summary>
    public IColorPickSource? ColorPickSource { get; set; }
    /// <summary>Gets the input-isolated color-picking session.</summary>
    public ColorPickSession ColorPicking { get; } = new();
    /// <summary>Gets the workspace-local asset drag gesture.</summary>
    public AssetDragSession AssetDragging { get; } = new();
    /// <summary>Gets explicit bindings processed before layout and after input.</summary>
    public IList<IFrameBinding> Bindings { get; } = new List<IFrameBinding>();

    private void UpdateBindings()
    {
        for (int i = 0; i < Bindings.Count; i++)
            Bindings[i].Update();
    }

    private bool _layoutDirty = true;
    private int _styleRevision = -1;
    private Size2 _previousSize;
    /// <summary>Invalidates layout after explicit property or tree changes.</summary>
    internal void InvalidateLayout() => _layoutDirty = true;
    /// <summary>Gets central styles without requiring the caller to create a registry.</summary>
    public StyleRegistry Styles => Registry.Styles;

    /// <summary>Applies a skin before updating the tree.</summary>
    public void SetSkin(ControlSkin skin)
    {
        skin.Apply(Registry);
        _layoutDirty = true;
    }

    private readonly List<Action> _pending = new();
    private readonly ControlInputContext _input;
    private bool _busy;
    private bool _drawing;
    private bool _hasFrame;
    private bool _hasPointer;
    private Vector2 _pointer;
    private ControlLayoutContext? _layout;
    /// <summary>Attaches an unowned control tree.</summary>
    public Workspace(Control content, UIRegistry? registry = null)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        if (content.Parent is not null || content.RootOwner is not null || content.Workspace is not null || content.CommittedParent is not null)
            throw new InvalidOperationException("Content already has an owner.");
        Registry = registry ?? content.FactoryRegistry ?? new UIRegistry();
        ReserveRoot(content);
        ContainerControl.Attach(content, this);
        _input = new ControlInputContext(this);
        RebuildControls();
    }

    /// <summary>Gets the explicit UI factory and style registry shared by this tree.</summary>
    public UIRegistry Registry { get; }
    /// <summary>Gets the root control.</summary>
    public Control Content { get; }
    /// <summary>Gets the last processed logical viewport size.</summary>
    public Size2 Size { get; private set; }
    /// <summary>Gets the current keyboard focus owner.</summary>
    public Control? FocusedControl { get; private set; }
    /// <summary>Gets the mouse capture owner. Use GetCapturedControl for touch identities.</summary>
    public Control? CapturedControl => GetCapturedControl(ControlPointer.Mouse);

    private readonly Dictionary<ControlPointer, Control> _captures = new();
    /// <summary>Gets the owner of an independent mouse or touch pointer.</summary>
    public Control? GetCapturedControl(ControlPointer pointer) => _captures.TryGetValue(pointer, out Control? owner) ? owner : null;
    /// <summary>Processes one application UI context.</summary>
    public void Update(ControlUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ProcessFrame(context.Input, context.Layout);
    }

    /// <summary>Samples caller-provided mouse and keyboard states using this workspace's DPI and a physical viewport.</summary>
    public void Update(ControlInputSampler sampler, MouseState mouse, KeyboardState keyboard, Rectangle viewport, TimeSpan elapsed, ControlLayoutContext layout, bool active = true) => Update(sampler, mouse, keyboard, default, viewport, elapsed, layout, active);
    /// <summary>Samples mouse/touch/keyboard once, derives logical layout size and synchronizes subsequent drawing automatically.</summary>
    public void Update(ControlInputSampler sampler, MouseState mouse, KeyboardState keyboard, TouchCollection touches, Rectangle viewport, TimeSpan elapsed, ControlLayoutContext layout, bool active = true)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(sampler);
        ArgumentNullException.ThrowIfNull(layout);
        if (_busy)
            throw new InvalidOperationException("UI processing cannot be reentered.");
        float dpi = Dpi;
        ControlInputFrame frame = sampler.SampleReusable(mouse, keyboard, touches, viewport, dpi / 96f, elapsed, active);
        ProcessFrameCore(frame, layout, viewport, dpi);
    }

    /// <summary>Updates one frame using the supplied layout context.</summary>
    public void Update(ControlInputFrame frame, ControlLayoutContext layout) => ProcessFrame(frame, layout);
    /// <summary>Processes ordered input, updates controls once, commits mutations, and finalizes layout.</summary>
    public void ProcessFrame(ControlInputFrame frame, ControlLayoutContext layout) => ProcessFrameCore(frame, layout, frame?.PhysicalViewport, Dpi);
    private void ProcessFrameCore(ControlInputFrame frame, ControlLayoutContext layout, Rectangle? viewport, float frameDpi)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(layout);
        if (_busy)
            throw new InvalidOperationException("UI processing cannot be reentered.");
        Size2 logicalSize = viewport is Rectangle physical ? new Size2(physical.Width / (frameDpi / 96f), physical.Height / (frameDpi / 96f)) : frame.Size;
        Control.ValidateSize(logicalSize, false);
        float inputScale = frame.PhysicalViewport.HasValue ? frame.SourceScale / (frameDpi / 96f) : 1;
        if (!float.IsFinite(inputScale) || inputScale <= 0)
            throw new ArgumentOutOfRangeException(nameof(frameDpi));
        _hasFrame = false;
        _frameViewport = viewport;
        _frameDpi = frameDpi;
        _busy = true;
        try
        {
            while (_posted.TryDequeue(out Action? posted))
                posted();
            Commit();
            UpdateBindings();
            if (_layoutDirty)
                RebuildControls();
            foreach (Control control in EnumerateControls())
                control.BeginFrame();
            if (!_previousSize.Equals(logicalSize))
            {
                _layoutDirty = true;
                _previousSize = logicalSize;
            }

            Size = logicalSize;
            if (!ReferenceEquals(_layout?.DefaultFont, layout.DefaultFont))
                _layoutDirty = true;
            _layout = layout;
            Layout();
            ValidateInteraction();
            for (int eventIndex = 0; eventIndex < frame.Events.Count; eventIndex++)
            {
                ControlInputEvent input = frame.Events[eventIndex];
                if (inputScale != 1 && input.Type is (ControlInputEventType.PointerMove or ControlInputEventType.PointerDown or ControlInputEventType.PointerUp or ControlInputEventType.PointerWheel or ControlInputEventType.PointerCancel))
                    input = input with
                    {
                        Position = input.Position * inputScale
                    };
                Dispatch(input);
                UpdateBindings();
                Layout();
                ValidateInteraction();
            }

            ColorPicking.Update(this);
            AssetDragging.Update(this);
            UpdateBindings();
            foreach (Control control in EnumerateControls())
                if (Visible(control))
                    control.Update(frame.Elapsed);
            Commit();
            Layout();
            ValidateInteraction();
            RefreshHover();
            UpdateToolTip(frame.Elapsed);
            _hasFrame = true;
        }
        finally
        {
            _busy = false;
        }
    }

    /// <summary>Draws the committed tree. Requires a preceding ProcessFrame with the same size and font.</summary>
    public void Draw(ControlDrawContext context, bool includePickingOverlay = true)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(context);
        if (_busy || !_hasFrame)
            throw new InvalidOperationException("Process a UI frame before drawing; drawing cannot be reentered.");
        SynchronizeDrawing(context);
        if (!Size.Equals(context.LogicalSize) || !ReferenceEquals(_layout?.DefaultFont, context.DefaultFont))
            throw new InvalidOperationException("Drawing must use the processed viewport size and default font.");
        _busy = true;
        _drawing = true;
        try
        {
            context.Bind(Registry);
            context.Begin();
            try
            {
                DrawTree(Content, new Bounds2(0, 0, Size.Width, Size.Height), 1f, context);
                DrawOverlays(context);
                if (includePickingOverlay)
                    ColorPicking.Draw(context, Size);
                AssetDragging.Draw(context, Size);
                if (_visibleToolTip is ToolTip tip)
                    DrawTree(tip, new Bounds2(0, 0, Size.Width, Size.Height), 1, context);
            }
            finally
            {
                context.End();
            }
        }
        finally
        {
            _drawing = false;
            _busy = false;
        }
    }

    private void SynchronizeDrawing(ControlDrawContext context)
    {
        if (_frameViewport is Rectangle viewport)
            context.SetViewport(viewport, _frameDpi / 96f);
    }

    /// <summary>Draws only the pipette overlay after presenting a clean sampled render target.</summary>
    public void DrawColorPickingOverlay(ControlDrawContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_busy && _hasFrame)
            SynchronizeDrawing(context);
        if (_busy || !_hasFrame || !Size.Equals(context.LogicalSize) || !ReferenceEquals(_layout?.DefaultFont, context.DefaultFont))
            throw new InvalidOperationException("Process a matching UI frame before drawing the picking overlay.");
        _busy = true;
        _drawing = true;
        try
        {
            context.Bind(Registry);
            context.Begin();
            try
            {
                ColorPicking.Draw(context, Size);
            }
            finally
            {
                context.End();
            }
        }
        finally
        {
            _drawing = false;
            _busy = false;
        }
    }

    internal bool DropAsset(AssetItem asset, Vector2 position)
    {
        Control? hit = HitTarget(position);
        while (hit is not null)
        {
            if (hit is ReferencePicker picker && picker.IsAvailable() && picker.CanSelectAsset(asset))
            {
                picker.SelectAsset(asset);
                return true;
            }

            hit = hit.Parent;
        }

        return false;
    }

    /// <summary>Assigns keyboard focus to a visible, enabled, focusable control in this root.</summary>
    public void Focus(Control? control)
    {
        if (_drawing)
            throw new InvalidOperationException("Drawing cannot change keyboard focus.");
        if (control is not null && (control.Workspace != this || !control.IsAvailable() || !control.IsFocusable || !IsInActiveScope(control)))
            throw new InvalidOperationException("The control cannot receive focus.");
        if (FocusedControl == control)
            return;
        Control? previous = FocusedControl;
        FocusedControl = control;
        previous?.LoseFocus();
        previous?.NotifyFocus();
        control?.NotifyFocus();
    }

    internal void CapturePointer(Control control) => CapturePointer(control, ControlPointer.Mouse);
    internal void CapturePointer(Control control, ControlPointer pointer)
    {
        ArgumentNullException.ThrowIfNull(control);
        if (control.Workspace != this || !control.IsAvailable(true) || !IsInActiveScope(control))
            throw new InvalidOperationException("The control cannot capture input.");
        Control? previous = GetCapturedControl(pointer);
        if (previous == control)
            return;
        _captures[pointer] = control;
        previous?.PointerCaptureLost(pointer);
    }

    internal void ReleasePointer(Control control) => ReleasePointer(control, ControlPointer.Mouse);
    internal void ReleasePointer(Control control, ControlPointer pointer)
    {
        if (GetCapturedControl(pointer) != control)
            return;
        _captures.Remove(pointer);
        control.PointerCaptureLost(pointer);
    }

    private void ReleasePointers(Func<Control, bool> predicate)
    {
        if (_captures.Count == 0)
            return;
        var pointers = _captures.Keys.OrderBy(p => p.Kind).ThenBy(p => p.Id).ToArray();
        foreach (var pointer in pointers)
            if (GetCapturedControl(pointer)is Control owner && predicate(owner))
                ReleasePointer(owner, pointer);
    }

    internal bool IsTraversing => _busy && !_committing;

    private readonly HashSet<Control> _rootReservations = new(ReferenceEqualityComparer.Instance);
    private void ReserveRoot(Control control)
    {
        VerifyTreeMutation();
        control.Workspace?.VerifyTreeMutation();
        if (control.Parent is not null || control.RootOwner is not null)
            throw new InvalidOperationException("The control already has an owner.");
        if (control.CommittedParent is not null && control.Workspace?.IsTraversing == true && control.Workspace != this)
            throw new InvalidOperationException("Wait for the previous workspace to commit this control's removal.");
        _rootReservations.Add(control);
        control.RootOwner = this;
    }

    private void ReleaseRoot(Control control)
    {
        _rootReservations.Remove(control);
        if (ReferenceEquals(control.RootOwner, this))
            control.RootOwner = null;
    }

    internal void VerifyTreeMutation()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_drawing)
            throw new InvalidOperationException("The control tree cannot change while drawing.");
    }

    internal void Mutate(Action action)
    {
        VerifyTreeMutation();
        if (_busy && !_committing)
            _pending.Add(action);
        else
            action();
    }

    private void Commit()
    {
        if (_pending.Count == 0)
            return;
        Action[] batch = _pending.ToArray();
        _pending.Clear();
        _committing = true;
        try
        {
            foreach (Action action in batch)
                action();
        }
        finally
        {
            _committing = false;
        }
    }

    private static void PrepareTree(Control control)
    {
        control.Prepare();
        if (control is ContainerControl container)
            foreach (Control child in container.OrderedChildren)
                if (ReferenceEquals(child.CommittedParent, container) && ReferenceEquals(child.Parent, container))
                    PrepareTree(child);
    }

    private void Layout()
    {
        Commit();
        Registry.Styles.PollChanges();
        if (_styleRevision != Registry.Styles.Revision)
        {
            _layoutDirty = true;
            _styleRevision = Registry.Styles.Revision;
        }

        if (!_layoutDirty)
            return;
        _layoutDirty = false;
        RebuildControls();
        PrepareTree(Content);
        foreach (var overlay in _overlays)
            PrepareTree(overlay.Control);
        RebuildControls();
        Content.Measure(_layout!, Size);
        Content.Arrange(new Bounds2(0, 0, Size.Width, Size.Height));
        LayoutOverlays();
        RefreshHover();
    }

    private void ValidateInteraction()
    {
        if (FocusedControl is Control focus && (focus.Workspace != this || !focus.IsAvailable() || !focus.IsFocusable || !IsInActiveScope(focus)))
            Focus(null);
        foreach (var capture in _captures)
            if (capture.Value.Workspace != this || !capture.Value.IsAvailable(true) || !IsInActiveScope(capture.Value))
            {
                ReleasePointers(c => c.Workspace != this || !c.IsAvailable(true) || !IsInActiveScope(c));
                break;
            }
    }

    internal bool PointerHits(Control control, Vector2 position) => HitTarget(position)is Control target && Within(target, control);
    private bool CapturedByAnotherPointer(Control? target, ControlPointer pointer)
    {
        if (target is null || GetCapturedControl(pointer)is not null)
            return false;
        foreach (var capture in _captures)
            if (capture.Key != pointer && Within(target, capture.Value))
                return true;
        return false;
    }

    private readonly List<Control> _route = new(16);
    private ControlInputEventArgs? _routedArgs;
    private ControlOverlay? FindOverlay(Control control)
    {
        for (int i = _overlays.Count - 1; i >= 0; i--)
            if (_overlays[i].Control == control)
                return _overlays[i];
        return null;
    }

    private void Dispatch(ControlInputEvent input)
    {
        if (ColorPicking.Handle(input) || AssetDragging.Handle(this, input))
            return;
        _input.Pointer = input.Pointer;
        if (input.Type != ControlInputEventType.PointerMove || input.PointerKind != ControlPointerKind.Mouse)
        {
            _tipSuppressed = true;
            HideToolTip();
            _tipElapsed = 0;
        }
        else
            _tipSuppressed = false;
        if (input.Type == ControlInputEventType.PointerCancel)
        {
            if (GetCapturedControl(input.Pointer)is Control cancelled)
                ReleasePointer(cancelled, input.Pointer);
            if (input.PointerKind == ControlPointerKind.Touch)
                _hasPointer = false;
            return;
        }

        if (input.Type == ControlInputEventType.Deactivate)
        {
            Focus(null);
            ReleasePointers(static _ => true);
            foreach (Control control in EnumerateControls())
                control.CancelInteraction();
            _hasPointer = false;
            RefreshHover();
            return;
        }

        if (input.Type == ControlInputEventType.KeyDown && input.Key == Keys.Escape && ActiveOverlay is Control popup && FindOverlay(popup)is { Popup: true })
        {
            ClosePopup(popup);
            return;
        }

        bool pointer = input.Type is ControlInputEventType.PointerMove or ControlInputEventType.PointerDown or ControlInputEventType.PointerUp or ControlInputEventType.PointerWheel;
        Control? target;
        if (pointer)
        {
            _pointer = input.Position;
            _hasPointer = true;
            RefreshHover();
            if (input.Type == ControlInputEventType.PointerDown && DismissOutsidePopup(_pointer))
                return;
            target = GetCapturedControl(input.Pointer) ?? HitTarget(_pointer);
            // Older controls have one gesture state. Never let another contact steal it.
            if (CapturedByAnotherPointer(target, input.Pointer))
                return;
            if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Left)
            {
                Control? focus = target;
                while (focus is not null && !focus.IsFocusable)
                    focus = focus.Parent;
                Focus(focus);
            }
        }
        else
        {
            if (input.Type == ControlInputEventType.KeyDown && input.Key == Keys.Tab && !input.Control && !(FocusedControl is TextBox { AcceptsTab: true }))
            {
                if (!input.IsRepeat)
                    MoveFocus(input.Shift);
                return;
            }

            target = FocusedControl ?? ActiveOverlay ?? Content;
        }

        if (target is null)
            return;
        var route = _route;
        route.Clear();
        for (Control? node = target; node is not null; node = node.Parent)
            route.Add(node);
        var args = (_routedArgs ??= new ControlInputEventArgs(input, target)).Reset(input, target);
        foreach (Control node in route)
        {
            if (!node.IsAvailable())
                break;
            args.CurrentTarget = node;
            node.Input(_input, args);
            if (args.Handled)
                break;
        }

        if (!args.Handled && input.Type == ControlInputEventType.KeyDown)
            foreach (Control node in EnumerateControls())
                if (node is MenuBar menu && IsInActiveScope(menu) && menu.HandleShortcut(input))
                    break;
        if (input.Type == ControlInputEventType.PointerUp && input.Button == MouseButton.Left)
        {
            if (GetCapturedControl(input.Pointer)is Control owner)
                ReleasePointer(owner, input.Pointer);
            if (input.PointerKind == ControlPointerKind.Touch)
                _hasPointer = false;
        }
    }

    private void MoveFocus(bool reverse)
    {
        Control[] controls = EnumerateControls().Where(c => c.IsAvailable() && c.IsFocusable && IsInActiveScope(c)).OrderBy(c => c.TabIndex).ToArray();
        if (controls.Length == 0)
        {
            Focus(null);
            return;
        }

        int index = Array.IndexOf(controls, FocusedControl);
        int next = index < 0 ? (reverse ? controls.Length - 1 : 0) : (index + (reverse ? controls.Length - 1 : 1)) % controls.Length;
        Focus(controls[next]);
    }

    private void RefreshHover()
    {
        Control? hit = _hasPointer ? HitTarget(_pointer) : null;
        for (int i = 0; i < _controls.Count; i++)
        {
            var c = _controls[i];
            c.IsPointerOver = hit is not null && Within(hit, c);
        }
    }

    private static Control? Hit(Control control, Bounds2 clip, Vector2 point)
    {
        if (!control.IsAvailable(true))
            return null;
        Bounds2 visible = Control.Intersect(clip, control.Bounds);
        if (!Control.Contains(visible, point.X, point.Y))
            return null;
        if (control is ContainerControl container)
        {
            Control[] children = container.OrderedChildren;
            for (int i = children.Length - 1; i >= 0; i--)
            {
                Control? hit = Hit(children[i], Control.Intersect(visible, control.GetChildClip(children[i])), point);
                if (hit is not null)
                    return hit;
            }
        }

        return control;
    }

    private static IEnumerable<Control> Walk(Control control)
    {
        yield return control;
        if (control is ContainerControl container)
            foreach (Control child in container.Children)
                foreach (Control descendant in Walk(child))
                    yield return descendant;
    }

    private static bool Visible(Control control)
    {
        for (Control? node = control; node is not null; node = node.Parent)
            if (!node.IsVisible)
                return false;
        return true;
    }

    private static void DrawTree(Control control, Bounds2 clip, float opacity, ControlDrawContext context)
    {
        if (!control.IsVisible)
            return;
        clip = Control.Intersect(clip, control.Bounds);
        opacity *= control.Opacity;
        if (clip.Width <= 0 || clip.Height <= 0 || opacity <= 0)
            return;
        context.SetControl(control.Bounds, clip, opacity);
        control.Paint(context);
        if (control is ContainerControl container)
            foreach (Control child in container.OrderedChildren)
                DrawTree(child, Control.Intersect(clip, control.GetChildClip(child)), opacity, context);
        context.SetControl(control.Bounds, clip, opacity);
        control.PaintOverlay(context);
    }

    private readonly List<ControlOverlay> _overlays = new();
    private readonly ConcurrentQueue<Action> _posted = new();
    private bool _committing;
    /// <summary>Gets pointer position.</summary>
    public Vector2 PointerPosition => _pointer;
    /// <summary>Gets overlay count.</summary>
    public int OverlayCount => _overlays.Count;
    private Control? ActiveOverlay => _overlays.Count == 0 ? null : _overlays[^1].Control;

    /// <summary>Performs post using the current control state.</summary>
    public void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _posted.Enqueue(action);
    }

    private readonly List<Control> _controls = new();
    internal List<Control> EnumerateControls() => _controls;
    private void RebuildControls()
    {
        _controls.Clear();
        Collect(Content);
        for (int i = 0; i < _overlays.Count; i++)
            Collect(_overlays[i].Control);
    }

    private void Collect(Control control)
    {
        _controls.Add(control);
        if (control is ContainerControl c)
            for (int i = 0; i < c.Children.Count; i++)
                Collect(c.Children[i]);
    }

    private static bool Within(Control control, Control ancestor)
    {
        for (Control? n = control; n is not null; n = n.Parent)
            if (n == ancestor)
                return true;
        return false;
    }

    private bool IsInActiveScope(Control control)
    {
        int modal = _overlays.FindLastIndex(e => e.Modal);
        if (modal < 0)
            return true;
        for (int i = modal; i < _overlays.Count; i++)
            if (Within(control, _overlays[i].Control))
                return true;
        return false;
    }

    /// <summary>Performs show popup using the current control state.</summary>
    public void ShowPopup(Control popup, Bounds2 placement, Control? owner = null, Action? closed = null)
    {
        ArgumentNullException.ThrowIfNull(popup);
        if (owner is not null && owner.Workspace != this)
            throw new InvalidOperationException("Popup owner belongs to another workspace.");
        ReserveRoot(popup);
        Mutate(() => AddOverlay(popup, placement, false, true, owner, closed));
    }

    /// <summary>Performs show window using the current control state.</summary>
    public void ShowWindow(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        ReserveRoot(window);
        Mutate(() => AddOverlay(window, Bounds2.Zero, false, false, null, null));
    }

    /// <summary>Performs show dialog using the current control state.</summary>
    public Task<DialogResult> ShowDialog(Dialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);
        ReserveRoot(dialog);
        Task<DialogResult> task;
        try
        {
            task = dialog.Prepare(this);
        }
        catch
        {
            ReleaseRoot(dialog);
            throw;
        }

        Mutate(() =>
        {
            AddOverlay(dialog, Bounds2.Zero, true, false, null, null);
            try
            {
                dialog.Open();
            }
            catch
            {
                CloseOverlay(dialog);
                throw;
            }
        });
        return task;
    }

    private void AddOverlay(Control control, Bounds2 placement, bool modal, bool popup, Control? owner, Action? closed)
    {
        if (!ReferenceEquals(control.RootOwner, this) || control.Parent is not null)
            throw new InvalidOperationException("Overlay reservation is no longer valid.");
        if (owner is not null && owner.Workspace != this)
        {
            ReleaseRoot(control);
            if (control is Dialog rejected)
                rejected.NotifyClosed();
            throw new InvalidOperationException("Popup owner belongs to another workspace.");
        }

        control.CommittedParent?.DetachCommittedChild(control);
        Control? old = FocusedControl;
        ReleasePointers(static _ => true);
        _overlays.Add(new ControlOverlay(control, placement, modal, popup, owner, old, closed));
        try
        {
            ContainerControl.Attach(control, this);
            PrepareTree(control);
            Focus(null);
            Control? focus = Walk(control).FirstOrDefault(c => c.IsAvailable() && c.IsFocusable);
            if (focus is not null)
                Focus(focus);
        }
        catch
        {
            CloseOverlay(control);
            throw;
        }
    }

    /// <summary>Performs close popup using the current control state.</summary>
    public void ClosePopup(Control popup) => Mutate(() => CloseOverlay(popup));
    internal void CloseOverlay(Control control)
    {
        ControlOverlay? entry = _overlays.FirstOrDefault(e => e.Control == control);
        if (entry is null)
            return;
        foreach (ControlOverlay child in _overlays.Where(e => e.Owner is not null && Within(e.Owner, control)).ToArray())
            CloseOverlay(child.Control);
        _overlays.Remove(entry);
        ContainerControl.Attach(control, null);
        ReleaseRoot(control);
        if (FocusedControl is Control focus && Within(focus, control))
            Focus(null);
        ReleasePointers(c => Within(c, control));
        if (entry.PreviousFocus is Control previous && previous.Workspace == this && previous.IsAvailable() && previous.IsFocusable && IsInActiveScope(previous))
            Focus(previous);
        entry.Closed?.Invoke();
        if (control is Window window)
            window.NotifyClosed();
    }

    private bool DismissOutsidePopup(Vector2 point)
    {
        ControlOverlay? popup = _overlays.LastOrDefault(e => e.Popup);
        if (popup is null || Control.Contains(popup.Control.Bounds, point.X, point.Y))
            return false;
        if (popup.Owner is Control owner && Control.Contains(owner.Bounds, point.X, point.Y))
            return false;
        while (popup.Owner is Control parent)
        {
            ControlOverlay? ancestor = _overlays.LastOrDefault(e => e.Popup && Within(parent, e.Control));
            if (ancestor is null || Control.Contains(ancestor.Control.Bounds, point.X, point.Y))
                break;
            popup = ancestor;
        }

        Control close = popup.Control;
        Mutate(() => CloseOverlay(close));
        return true;
    }

    private Control? HitTarget(Vector2 point)
    {
        var bounds = new Bounds2(0, 0, Size.Width, Size.Height);
        for (int i = _overlays.Count - 1; i >= 0; i--)
        {
            ControlOverlay e = _overlays[i];
            Control? hit = Hit(e.Control, bounds, point);
            if (hit is not null)
                return hit;
            if (e.Modal)
                return null;
        }

        return Hit(Content, bounds, point);
    }

    private void LayoutOverlays()
    {
        foreach (ControlOverlay e in _overlays)
        {
            e.Control.Measure(_layout!, Size);
            Bounds2 slot = e.Placement;
            if (e.Control is Window window)
            {
                float w = MathF.Min(Size.Width, window.DesiredSize.Width), h = MathF.Min(Size.Height, window.DesiredSize.Height);
                Vector2 position = window.Position ?? new Vector2((Size.Width - w) / 2, (Size.Height - h) / 2);
                slot = new Bounds2(Math.Clamp(position.X, 0, MathF.Max(0, Size.Width - w)), Math.Clamp(position.Y, 0, MathF.Max(0, Size.Height - h)), w, h);
            }
            else
            {
                float w = MathF.Min(Size.Width, slot.Width > 0 ? slot.Width : e.Control.DesiredSize.Width), h = MathF.Min(Size.Height, slot.Height > 0 ? slot.Height : e.Control.DesiredSize.Height);
                slot = new Bounds2(Math.Clamp(slot.X, 0, MathF.Max(0, Size.Width - w)), Math.Clamp(slot.Y, 0, MathF.Max(0, Size.Height - h)), w, h);
            }

            e.Control.Arrange(slot, true);
        }
    }

    private void DrawOverlays(ControlDrawContext context)
    {
        var bounds = new Bounds2(0, 0, Size.Width, Size.Height);
        foreach (ControlOverlay entry in _overlays)
        {
            if (ColorPicking.IsActive && ReferenceEquals(entry.Control, ColorPicking.Owner))
                continue;
            context.SetControl(bounds, bounds, 1);
            if (entry.Modal && !context.DrawSkinRegion("modal_overlay", bounds))
                context.FillRectangle(bounds, new Color(0, 0, 0, 160));
            Bounds2 b = entry.Control.Bounds;
            if (entry.Control is not MenuBar { UseSkin: false })
                context.DrawSkinPart(entry.Control, "Shadow", new Bounds2(b.X - 8, b.Y - 8, b.Width + 16, b.Height + 16));
            DrawTree(entry.Control, bounds, 1, context);
        }
    }

    private ToolTip? _visibleToolTip;
    private Control? _tipOwner;
    private string _tipText = string.Empty;
    private Control? _tipContent;
    private double _tipElapsed;
    private bool _tipSuppressed;
    /// <summary>Gets the currently drawn noninteractive hover tooltip.</summary>
    public ToolTip? VisibleToolTip => _visibleToolTip;

    private void HideToolTip()
    {
        if (_visibleToolTip is not ToolTip tip)
            return;
        ContainerControl.Attach(tip, null);
        ReleaseRoot(tip);
        _visibleToolTip = null;
    }

    private void UpdateToolTip(TimeSpan elapsed)
    {
        Control? owner = _hasPointer && !_tipSuppressed && _captures.Count == 0 ? HitTarget(_pointer) : null;
        while (owner is not null && owner.ToolTip.Length == 0 && owner.ToolTipContent is null)
            owner = owner.Parent;
        if (owner is not null && !IsInActiveScope(owner))
            owner = null;
        string text = owner?.ToolTip ?? string.Empty;
        Control? content = owner?.ToolTipContent;
        if (owner != _tipOwner || text != _tipText || !ReferenceEquals(content, _tipContent))
        {
            HideToolTip();
            _tipOwner = owner;
            _tipText = text;
            _tipContent = content;
            _tipElapsed = 0;
        }

        if (owner is null)
        {
            HideToolTip();
            return;
        }

        var d = owner;
        if (!float.IsFinite(d.ConfigToolTipDelay) || d.ConfigToolTipDelay < 0 || !float.IsFinite(d.ConfigToolTipDuration) || d.ConfigToolTipDuration < 0 || !float.IsFinite(d.ConfigToolTipMaxWidth) || d.ConfigToolTipMaxWidth <= 0)
            throw new InvalidOperationException("Invalid tooltip timing or width.");
        _tipElapsed += elapsed.TotalSeconds;
        if (_tipElapsed < d.ConfigToolTipDelay || d.ConfigToolTipDuration > 0 && _tipElapsed >= d.ConfigToolTipDelay + d.ConfigToolTipDuration)
        {
            HideToolTip();
            return;
        }

        if (_visibleToolTip is null)
        {
            ToolTip tip = content is not null ? owner.ToolTipPresenter : new ToolTip
            {
                Content = new Label
                {
                    Text = text,
                    WrapText = true
                }
            };
            ReserveRoot(tip);
            ContainerControl.Attach(tip, this);
            _visibleToolTip = tip;
        }

        PrepareTree(_visibleToolTip);
        foreach (Control node in Walk(_visibleToolTip))
            if (Visible(node))
                node.Update(elapsed);
        _visibleToolTip.Measure(_layout!, new Size2(MathF.Min(Size.Width, d.ConfigToolTipMaxWidth), Size.Height));
        float w = MathF.Min(Size.Width, _visibleToolTip.DesiredSize.Width), h = MathF.Min(Size.Height, _visibleToolTip.DesiredSize.Height);
        float x = Math.Clamp(_pointer.X + 16, 0, MathF.Max(0, Size.Width - w)), y = _pointer.Y + 22;
        if (y + h > Size.Height)
            y = MathF.Max(0, _pointer.Y - h - 10);
        _visibleToolTip.Arrange(new Bounds2(x, y, w, h), true);
    }

    private bool _disposed;
    /// <summary>Detaches the tree and releases binding/source subscriptions. Caller-owned fonts and skins remain borrowed.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        if (_busy)
            throw new InvalidOperationException("Cannot dispose a root during update or draw.");
        ColorPicking.Cancel();
        AssetDragging.Cancel();
        Focus(null);
        ReleasePointers(static _ => true);
        HideToolTip();
        while (_overlays.Count > 0)
        {
            var control = _overlays[^1].Control;
            if (control is Dialog dialog)
                dialog.Complete(DialogResult.Cancel);
            else
                CloseOverlay(control);
        }

        for (int i = 0; i < Bindings.Count; i++)
            if (Bindings[i] is IDisposable binding)
                binding.Dispose();
        Bindings.Clear();
        ContainerControl.Attach(Content, null);
        foreach (Control reserved in _rootReservations.ToArray())
        {
            ContainerControl.Attach(reserved, null);
            ReleaseRoot(reserved);
            if (reserved is Dialog pendingDialog)
                pendingDialog.NotifyClosed();
        }

        _controls.Clear();
        _pending.Clear();
        while (_posted.TryDequeue(out _))
        {
        };
        _disposed = true;
    }
}
