namespace Sachssoft.Engine.Controls.Inspection;

/// <summary>Keeps the typed property operations and its explicit reset snapshot outside frame updates.</summary>
internal sealed class InspectorPropertyActions<T> : IInspectorPropertyActions
{
    private readonly Func<T> _read;
    private readonly Action<T> _write;
    private readonly Func<T, T>? _snapshot;
    private T _default;
    internal InspectorPropertyActions(Func<T> read, Action<T> write, Func<T, T>? snapshot = null)
    {
        _read = read; _write = write; _snapshot = snapshot; _default = Snapshot(read());
    }
    private T Snapshot(T value) => _snapshot is null ? value : _snapshot(value);
    internal void SetDefault(T value) => _default = Snapshot(value);
    public void Reset() => _write(Snapshot(_default));
    public void Copy(InspectorClipboard clipboard) => clipboard.Copy(Snapshot(_read()));
    public bool CanPaste(InspectorClipboard clipboard) => clipboard.Contains<T>();
    public void Paste(InspectorClipboard clipboard) { if (clipboard.TryGet(out T value)) _write(Snapshot(value)); }
}
