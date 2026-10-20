using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using Sachssoft.Engine.Performance;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Custom drawing and captured dragging inside an ordinary styled control.</summary>
internal sealed class EditorCanvas : Control
{
    private readonly EditorDocument _document;
    private ControlPointer? _pointer;
    private ControlInputContext? _captureContext;
    private Vector2 _offset;
    private float _scale = 1;
    private Vector2 _origin;
    private StyleProperty<bool> _grid = new(true);
    private StyleProperty<float> _snap = new(20);
    private static readonly System.ComponentModel.PropertyChangedEventArgs GridChangedArgs = new(nameof(ShowGrid));
    private static readonly System.ComponentModel.PropertyChangedEventArgs SnapChangedArgs = new(nameof(Snap));
    internal bool ShowGrid
    {
        get => _grid.Value;
        set
        {
            if (_grid.SetLocal(value))
                NotifyPropertyChanged(GridChangedArgs);
        }
    }

    internal bool Snap
    {
        get => _snap.Value > 0;
        set
        {
            if (_snap.SetLocal(value ? 20 : 0))
                NotifyPropertyChanged(SnapChangedArgs);
        }
    }

    internal EditorCanvas(EditorDocument document)
    {
        _document = document;
        StyleId = "Editor.Canvas";
    }

    /// <inheritdoc/>
    public override void ApplyStyle(Style style)
    {
        base.ApplyStyle(style);
        if (style.Values.TryGetValue("ShowGrid", out var grid) && _grid.Apply(Converters.Convert<bool>("Boolean", grid)))
            NotifyPropertyChanged(GridChangedArgs);
        if (style.Values.TryGetValue("SnapX", out var snap) && _snap.Apply(Converters.Convert<float>("Single", snap)))
            NotifyPropertyChanged(SnapChangedArgs);
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
        _grid.BeginStyle(true);
        _snap.BeginStyle(20);
    }

    /// <summary>Gets default style name.</summary>
    protected override string DefaultStyleName => "Control";

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available) => new(Math.Min(available.Width, 960), Math.Min(available.Height, 540));
    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        _scale = Math.Max(.001f, Math.Min(Math.Max(0, bounds.Width - 32) / 960, Math.Max(0, bounds.Height - 32) / 540));
        _origin = new((Bounds.Width - 960 * _scale) / 2, (Bounds.Height - 540 * _scale) / 2);
    }

    internal Vector2 Screen(float x, float y) => new(Bounds.X + _origin.X + x * _scale, Bounds.Y + _origin.Y + y * _scale);
    private Vector2 World(Vector2 p) => (p - new Vector2(Bounds.X, Bounds.Y) - _origin) / _scale;
    private void Move(Vector2 p)
    {
        var world = World(p) - _offset;
        if (Snap)
        {
            world.X = MathF.Round(world.X / 20) * 20;
            world.Y = MathF.Round(world.Y / 20) * 20;
        }

        _document.Drag(world.X, world.Y);
    }

    /// <inheritdoc/>
    protected override void OnInput(ControlInputContext context, ControlInputEventArgs args)
    {
        var i = args.Input;
        if (i.Type == ControlInputEventType.KeyDown && i.Key == Keys.Escape && _pointer is not null)
        {
            Cancel();
            args.Handled = true;
            return;
        }

        if (i.Type == ControlInputEventType.KeyDown && _pointer is null && _document.Selected is EditorObject selected)
        {
            if (i.Key == Keys.Delete)
            {
                _document.Delete();
                args.Handled = true;
            }
            else if (i.Key is Keys.Left or Keys.Right or Keys.Up or Keys.Down)
            {
                float step = i.Shift ? 20 : 1;
                _document.Update(selected with { X = Math.Clamp(selected.X + (i.Key == Keys.Left ? -step : i.Key == Keys.Right ? step : 0), 0, 960 - selected.Width), Y = Math.Clamp(selected.Y + (i.Key == Keys.Up ? -step : i.Key == Keys.Down ? step : 0), 0, 540 - selected.Height) });
                args.Handled = true;
            }
        }

        if (i.Type == ControlInputEventType.PointerDown && i.Button == MouseButton.Left && _pointer is null)
        {
            context.Focus(this);
            var p = World(i.Position);
            var hit = _document.Objects.LastOrDefault(o => o.Visible && EditorGeometry.Contains(o, p));
            _document.Select(hit?.Id);
            args.Handled = true;
            if (hit is null)
                return;
            _offset = p - new Vector2(hit.X, hit.Y);
            _document.BeginDrag();
            _pointer = i.Pointer;
            _captureContext = context;
            context.CapturePointer(this);
        }
        else if (_pointer == i.Pointer && _pointer is not null)
        {
            if (i.Type == ControlInputEventType.PointerMove)
            {
                Move(i.Position);
                args.Handled = true;
            }
            else if (i.Type == ControlInputEventType.PointerUp && i.Button == MouseButton.Left)
            {
                Move(i.Position);
                _pointer = null;
                _document.EndDrag(false);
                context.ReleasePointer(this);
                _captureContext = null;
                args.Handled = true;
            }
        }
    }

    private void Cancel()
    {
        if (_pointer is not ControlPointer pointer)
            return;
        _pointer = null;
        _document.EndDrag(true);
        _captureContext?.ReleasePointer(this, pointer);
        _captureContext = null;
    }

    /// <inheritdoc/>
    protected override void OnInteractionCancelled() => Cancel();
    /// <inheritdoc/>
    protected override void OnPointerCaptureLost(ControlPointer pointer)
    {
        if (_pointer == pointer)
            Cancel();
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext c)
    {
        base.OnDraw(c);
        var area = new Bounds2(_origin.X, _origin.Y, 960 * _scale, 540 * _scale);
        c.FillRectangle(area, new Color(24, 34, 46));
        using (c.PushClip(area))
        {
            if (ShowGrid)
            {
                for (int x = 0; x <= 960; x += 20)
                    c.DrawLine(_origin + new Vector2(x * _scale, 0), _origin + new Vector2(x * _scale, 540 * _scale), new Color(36, 48, 63));
                for (int y = 0; y <= 540; y += 20)
                    c.DrawLine(_origin + new Vector2(0, y * _scale), _origin + new Vector2(960 * _scale, y * _scale), new Color(36, 48, 63));
            }

            foreach (var o in _document.Objects)
            {
                if (!o.Visible)
                    continue;
                Vector2 center = EditorGeometry.Center(o);
                var left = EditorGeometry.Rotate(center - new Vector2(o.Width / 2, 0), center, o.RotationDegrees);
                var right = EditorGeometry.Rotate(center + new Vector2(o.Width / 2, 0), center, o.RotationDegrees);
                c.DrawLine(_origin + left * _scale, _origin + right * _scale, Color.FromNonPremultiplied(o.Color.ToVector4()), o.Height * _scale);
                var corners = EditorGeometry.Corners(o);
                bool selected = o.Id == _document.Selection;
                for (int j = 0; j < 4; j++)
                {
                    Vector2 a = _origin + corners[j] * _scale, b = _origin + corners[(j + 1) % 4] * _scale;
                    c.DrawLine(a, b, selected ? new Color(87, 222, 236) : new Color(255, 255, 255, 50), selected ? 2 : 1);
                    if (selected)
                        c.FillRectangle(new Bounds2(a.X - 3, a.Y - 3, 6, 6), Color.White);
                }

                // Names are editor overlays; they remain horizontal for readability.
                var textSize = c.DefaultFont.Measure(o.Name);
                if (textSize.Width < o.Width * _scale - 16 && textSize.Height < o.Height * _scale - 8)
                    c.DrawText(c.DefaultFont, o.Name, _origin + center * _scale - new Vector2(textSize.Width, textSize.Height) / 2, Color.White);
            }
        }

        c.StrokeRectangle(area, new Color(65, 83, 105), 1);
    }

    /// <summary>Commits final style values and notifies once per effective property change.</summary>
    protected override void CommitStyledValues()
    {
        base.CommitStyledValues();
        if (_grid.CommitStyle())
            NotifyPropertyChanged(GridChangedArgs);
        if (_snap.CommitStyle())
            NotifyPropertyChanged(SnapChangedArgs);
    }
}
