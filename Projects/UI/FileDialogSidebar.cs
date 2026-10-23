namespace Sachssoft.Engine.Controls;
/// <summary>
/// Places favorites above a separately scrolling drive list.
/// </summary>
internal sealed class FileDialogSidebar : ContainerControl
{
    private readonly TextBlock _favoritesTitle = new()
    {
        Text = "Favorites"
    }, _drivesTitle = new()
    {
        Text = "Drives"
    };
    internal readonly ListBox Favorites = new(), Drives = new();
    internal FileDialogSidebar()
    {
        base.Add(_favoritesTitle);
        base.Add(Favorites);
        base.Add(_drivesTitle);
        base.Add(Drives);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        foreach (var child in Children)
            child.Measure(context, available);
        return new(170, 200);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float label = MathF.Min(24, b.Height / 4), list = MathF.Max(0, (b.Height - 2 * label) / 2);
        _favoritesTitle.Arrange(new(b.X, b.Y, b.Width, label), true);
        Favorites.Arrange(new(b.X, b.Y + label, b.Width, list), true);
        _drivesTitle.Arrange(new(b.X, b.Y + label + list, b.Width, label), true);
        Drives.Arrange(new(b.X, b.Y + 2 * label + list, b.Width, list), true);
    }
}
