using Sachssoft.Engine.Services.Platform;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>An application-local clipboard adapter. Replace with a platform service for system-wide clipboard access.</summary>
internal sealed class ExampleClipboardService : IClipboardService
{
    private readonly Dictionary<string, object?> _data = new(StringComparer.Ordinal);
    /// <summary>Performs set data using the current control state.</summary>
    public void SetData(string format, object? data) => _data[format] = data;
    /// <summary>Performs get data using the current control state.</summary>
    public object? GetData(string format) => _data.TryGetValue(format, out object? value) ? value : null;
    /// <summary>Performs contains data using the current control state.</summary>
    public bool ContainsData(string format) => _data.ContainsKey(format);
    /// <summary>Performs clear using the current control state.</summary>
    public void Clear() => _data.Clear();
}
