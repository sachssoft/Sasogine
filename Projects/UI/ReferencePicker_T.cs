using System.ComponentModel;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Edits an engine reference and accepts only objects assignable to the declared target type.
/// </summary>
/// <typeparam name="T">The referenced engine object type.</typeparam>
public sealed class ReferencePicker<T> : ReferencePicker where T : class, IEngineReferenceable
{
    private Reference<T>? _value;
    private static readonly PropertyChangedEventArgs ValueArgs = new(nameof(Value));
    /// <summary>
    /// Gets or sets the reference. User selection replaces it instead of mutating a shared instance.
    /// </summary>
    public Reference<T>? Value
    {
        get => _value;
        set
        {
            if (SetNotify(ref _value, value, ValueArgs)) ReferenceId = value?.Id;
        }
    }
    /// <inheritdoc/>
    protected override bool Accepts(AssetItem asset) => asset.Target is T;
    /// <inheritdoc/>
    protected override void OnReferenceChanged()
    {
        if (_value?.Id != ReferenceId)
            SetNotify(ref _value, ReferenceId is null ? null : new Reference<T>(ReferenceId), ValueArgs);
        base.OnReferenceChanged();
    }
    /// <inheritdoc/>
    protected override void OnUpdate(TimeSpan elapsed)
    {
        base.OnUpdate(elapsed);
        // Reference<T>.Id itself is mutable and does not expose notifications.
        if (_value?.Id != ReferenceId) ReferenceId = _value?.Id;
    }
}
