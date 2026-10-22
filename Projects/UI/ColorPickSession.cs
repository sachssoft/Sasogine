using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls;
/// <summary>Owns a deterministic pipette gesture, with preview, accept and cancel callbacks on the UI thread.</summary>
public sealed class ColorPickSession
{
    private IColorPickSource? _source;
    private Action<Color>? _preview, _accept;
    private Action? _cancel;
    private Color _original;
    private bool _pending, _hasSample, _acceptPending;
    private string _label = string.Empty;
    /// <summary>Gets whether a color-picking gesture currently intercepts workspace input.</summary>
    public bool IsActive => Owner is not null;
    /// <summary>Gets the control owning the gesture.</summary>
    public Control? Owner { get; private set; }
    /// <summary>Gets the current pointer position in workspace coordinates.</summary>
    public Vector2 Position { get; private set; }
    /// <summary>Gets the latest valid straight RGBA preview.</summary>
    public Color SelectedColor { get; private set; }
    /// <summary>Gets or sets whether samples retain the original property's alpha channel.</summary>
    public bool PreserveAlpha { get; set; } = true;

    /// <summary>Starts a gesture. A preview callback may update the model; cancellation must restore it.</summary>
    public void Begin(Control owner, IColorPickSource source, Color original, Action<Color> preview, Action<Color> accept, Action cancel)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(accept);
        ArgumentNullException.ThrowIfNull(cancel);
        if (IsActive)
            throw new InvalidOperationException("A color pick is already active.");
        if (owner.Workspace is null)
            throw new InvalidOperationException("Attach the owner before picking a color.");
        owner.Workspace.AssetDragging.Cancel();
        Owner = owner;
        _source = source;
        _original = original;
        SelectedColor = original;
        _preview = preview;
        _accept = accept;
        _cancel = cancel;
        Position = owner.Workspace.PointerPosition;
        _pending = true;
        _hasSample = _acceptPending = false;
        _label = "Pick a pixel · Click: accept · Escape: cancel";
    }

    /// <summary>Cancels the active gesture and invokes its rollback callback exactly once.</summary>
    public void Cancel()
    {
        if (!IsActive)
            return;
        Action? callback = _cancel;
        Reset();
        callback?.Invoke();
    }

    private void Reset()
    {
        Owner = null;
        _source = null;
        _preview = null;
        _accept = null;
        _cancel = null;
        _pending = false;
        _hasSample = _acceptPending = false;
    }

    private bool Sample()
    {
        _pending = false;
        if (_source is DeferredColorPickSource { Source: null })
        {
            _pending = true;
            _hasSample = false;
            return false;
        }

        if (_source is null || !_source.TryPick(Position, out Color color))
        {
            _hasSample = false;
            return false;
        }

        if (PreserveAlpha)
            color.A = _original.A;
        bool changed = !_hasSample || color != SelectedColor;
        SelectedColor = color;
        _hasSample = true;
        _label = $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2} · Click: accept · Escape: cancel";
        if (changed)
            _preview?.Invoke(color);
        return true;
    }

    internal void Update(Workspace workspace)
    {
        if (IsActive && Owner?.Workspace != workspace)
            Cancel();
        if (_pending && Sample() && _acceptPending)
        {
            Action<Color>? callback = _accept;
            Color color = SelectedColor;
            Reset();
            callback?.Invoke(color);
        }
    }

    internal bool Handle(ControlInputEvent input)
    {
        if (!IsActive)
            return false;
        if (input.Type == ControlInputEventType.Deactivate)
        {
            Cancel();
            return false;
        }

        if (input.Type == ControlInputEventType.PointerCancel || input.Type == ControlInputEventType.KeyDown && input.Key == Keys.Escape)
        {
            Cancel();
            return true;
        }

        if (input.Type is ControlInputEventType.PointerMove or ControlInputEventType.PointerDown)
        {
            Position = input.Position;
            _pending = true;
            if (input.Type == ControlInputEventType.PointerDown)
            {
                if (input.Button == MouseButton.Right)
                    Cancel();
                else if (input.Button == MouseButton.Left)
                {
                    if (Sample())
                    {
                        Action<Color>? callback = _accept;
                        Color color = SelectedColor;
                        Reset();
                        callback?.Invoke(color);
                    }
                    else
                        _acceptPending = _source is DeferredColorPickSource { Source: null };
                }
            }
        }

        return true;
    }

    internal void Draw(ControlDrawContext context, Size2 size)
    {
        if (!IsActive)
            return;
        var bounds = new Bounds2(0, 0, size.Width, size.Height);
        context.SetControl(bounds, bounds, 1);
        context.DrawLine(Position - new Vector2(10, 0), Position + new Vector2(10, 0), Color.White);
        context.DrawLine(Position - new Vector2(0, 10), Position + new Vector2(0, 10), Color.White);
        float x = Math.Clamp(Position.X + 18, 0, MathF.Max(0, size.Width - 380));
        float y = Math.Clamp(Position.Y + 18, 0, MathF.Max(0, size.Height - 120));
        context.FillRectangle(new Bounds2(x, y, 380, 116), new Color(18, 23, 31));
        IColorPickSource? previewSource = _source is DeferredColorPickSource deferred ? deferred.Source : _source;
        if (_hasSample && previewSource is RenderTargetColorSource source)
        {
            ReadOnlySpan<Color> pixels = source.MagnifierPixels;
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                    context.FillRectangle(new Bounds2(x + 4 + col * 8, y + 4 + row * 8, 8, 8), Color.FromNonPremultiplied(pixels[row * 9 + col].ToVector4()));
            context.DrawLine(new Vector2(x + 36, y + 36), new Vector2(x + 44, y + 36), Color.White);
            context.DrawLine(new Vector2(x + 36, y + 44), new Vector2(x + 44, y + 44), Color.White);
        }

        context.FillRectangle(new Bounds2(x + 88, y + 8, 52, 52), Color.FromNonPremultiplied(SelectedColor.ToVector4()));
        context.DrawText(Owner?.GetFont(context) ?? context.DefaultFont, _label, new Vector2(x + 5, y + 84), Color.White);
    }
}
