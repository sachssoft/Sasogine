using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;

namespace Sachssoft.Engine.Controls;
/// <summary>Transfers an asset entry from a selectable view to a compatible reference field.</summary>
public sealed class AssetDragSession
{
    private AssetView? _owner;
    private ControlPointer _pointer;
    private Vector2 _position;
    /// <summary>Gets the active asset payload, or null when no gesture is active.</summary>
    public AssetItem? Payload { get; private set; }
    internal void Begin(AssetView owner, AssetItem asset, ControlPointer pointer, Vector2 position)
    {
        Cancel(); _owner = owner; Payload = asset; _pointer = pointer; _position = position;
        owner.CancelInteraction();
    }
    /// <summary>Cancels the gesture and releases its capture.</summary>
    public void Cancel()
    {
        if (_owner?.Workspace is Workspace workspace) workspace.ReleasePointer(_owner, _pointer);
        _owner = null; Payload = null;
    }
    internal void Update(Workspace workspace) { if (Payload is not null && _owner?.Workspace != workspace) Cancel(); }
    internal bool Handle(Workspace workspace, ControlInputEvent input)
    {
        if (Payload is not AssetItem asset) return false;
        if (_owner?.Workspace != workspace) { Cancel(); return false; }
        if (input.Type == ControlInputEventType.PointerDown && input.Button == MouseButton.Right) { Cancel(); return true; }
        if (input.Type == ControlInputEventType.Deactivate) { Cancel(); return false; }
        if (input.Type == ControlInputEventType.KeyDown && input.Key == Keys.Escape) { Cancel(); return true; }
        if (input.Type is ControlInputEventType.PointerMove or ControlInputEventType.PointerUp or ControlInputEventType.PointerCancel)
        {
            if (input.Pointer != _pointer) return true;
            _position = input.Position;
            if (input.Type == ControlInputEventType.PointerCancel) Cancel();
            else if (input.Type == ControlInputEventType.PointerUp)
            {
                Cancel();
                if (input.Button == MouseButton.Left) workspace.DropAsset(asset, input.Position);
            }
        }
        return true;
    }
    internal void Draw(ControlDrawContext context, Size2 size)
    {
        if (Payload is not AssetItem asset) return;
        var bounds = new Bounds2(0, 0, size.Width, size.Height);
        context.SetControl(bounds, bounds, 1);
        var at = _position + new Vector2(16, 16);
        context.FillRectangle(new Bounds2(at.X, at.Y, 220, 32), new Color(20, 28, 40));
        context.DrawText(context.DefaultFont, asset.Name, at + new Vector2(6, 4), Color.White);
    }
}
