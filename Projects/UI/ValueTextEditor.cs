namespace Sachssoft.Engine.Controls;
/// <summary>
/// Notifies its numeric owner synchronously when text input loses focus.
/// </summary>
internal sealed class ValueTextEditor : TextBox
{
    internal event EventHandler? EditingFocusLost;
    /// <inheritdoc/>
    protected override void OnFocusLost()
    {
        base.OnFocusLost();
        EditingFocusLost?.Invoke(this, EventArgs.Empty);
    }
}
