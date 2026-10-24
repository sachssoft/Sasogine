namespace Sachssoft.Engine.Controls;
/// <summary>Hosts display content without adding independent visual decoration.</summary>
internal sealed class IconValuePresenter : ContentControl
{
    /// <inheritdoc/>
    protected override string DefaultStyleName => "Control";
    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs notification)
    {
        // Ownership changes and child text metrics already invalidate the affected geometry.
        if (notification.PropertyName != nameof(Content))
            base.OnPropertyChanged(notification);
    }
}
