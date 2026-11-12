namespace Sachssoft.Engine.Controls;

/// <summary>Creates reusable item controls using typed callbacks without reflection.</summary>
/// <typeparam name="TControl">The presentation type produced by the factory.</typeparam>
public sealed class RecyclingItemTemplate<TControl> : IRecyclingItemTemplate where TControl : Control
{
    /// <summary>Creates a fresh presentation.</summary>
    private readonly Func<TControl> _create;

    /// <summary>Optionally updates properties that are not bound to the data context.</summary>
    private readonly Action<TControl, object?>? _rebind;

    /// <summary>Creates a template for controls that can present different items.</summary>
    /// <param name="create">Creates a new unowned control.</param>
    /// <param name="rebind">Optionally updates item-dependent properties after data context assignment.</param>
    public RecyclingItemTemplate(Func<TControl> create, Action<TControl, object?>? rebind = null)
    {
        _create = create ?? throw new ArgumentNullException(nameof(create));
        _rebind = rebind;
    }

    /// <inheritdoc/>
    public Control Create(object? source) => _create();

    /// <inheritdoc/>
    public void Rebind(Control control, object? item) => _rebind?.Invoke((TControl)control, item);
}
