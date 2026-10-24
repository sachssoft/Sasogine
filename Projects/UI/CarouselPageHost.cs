namespace Sachssoft.Engine.Controls;

/// <summary>Retains a page while keeping its animation opacity separate from user properties.</summary>
internal sealed class CarouselPageHost : ContentControl
{
    /// <summary>Creates a stretch-aligned page host with page-layer drawing order.</summary>
    internal CarouselPageHost()
    {
        ContentHorizontalAlignment = ControlAlignment.Stretch;
        ContentVerticalAlignment = ControlAlignment.Stretch;
        ZIndex = 1;
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Control";

    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;
}
