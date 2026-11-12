using Sachssoft.Engine;
namespace Sachssoft.Engine.Controls;

/// <summary>Retains a page while keeping its animation opacity separate from user properties.</summary>
internal sealed class CarouselPageHost : ContentControl, IControlTransform
{
    /// <summary>Stores the presentation transform independently of the arranged page bounds.</summary>
    private Matrix _slideTransform = Matrix.Identity;

    /// <summary>Creates a stretch-aligned page host with page-layer drawing order.</summary>
    internal CarouselPageHost()
    {
        ContentHorizontalAlignment = LayoutAlignment.Stretch;
        ContentVerticalAlignment = LayoutAlignment.Stretch;
        ZIndex = 1;
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Control";

    /// <inheritdoc/>
    protected override bool DrawsDecoration => false;

    /// <inheritdoc/>
    Matrix IControlTransform.VisualTransform => _slideTransform;

    /// <summary>Moves the rendered page without running layout on the hosted subtree.</summary>
    /// <param name="x">The horizontal displacement in logical units.</param>
    /// <param name="y">The vertical displacement in logical units.</param>
    internal void SetSlideOffset(float x, float y)
    {
        if (_slideTransform.M41 == x && _slideTransform.M42 == y)
            return;

        _slideTransform.M41 = x;
        _slideTransform.M42 = y;
        InvalidateVisual();
    }
}
