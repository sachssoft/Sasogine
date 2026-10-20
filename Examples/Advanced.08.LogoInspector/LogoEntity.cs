using Microsoft.Xna.Framework;
using System.Collections.Specialized;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>An actual Sasogine entity whose cached vector geometry is rebuilt only after its definition changes.</summary>
internal sealed class LogoEntity : EntityBase<LogoDefinition>, IDisposable
{
    private static readonly Vector2[] Mark =
    {
        new(55, -60),
        new(-35, -60),
        new(-65, -25),
        new(35, 25),
        new(5, 60),
        new(-65, 60)
    };
    private readonly List<LogoAccentDefinition> _subscribed = new();
    private readonly List<LogoSegment> _segments = new();
    private bool _dirty = true;
    internal LogoEntity(LogoDefinition definition) : base(definition)
    {
        IsVisible = true;
        IsEnabled = true;
        definition.PropertyChanged += Changed;
        definition.Transform.PropertyChanged += Changed;
        definition.Appearance.PropertyChanged += Changed;
        definition.Spatial.PropertyChanged += Changed;
        definition.Accents.CollectionChanged += CollectionChanged;
        SubscribeItems();
    }

    internal IReadOnlyList<LogoSegment> Segments => _segments;
    internal int GeometryRevision { get; private set; }
    internal string Title { get; private set; } = string.Empty;
    internal string Subtitle { get; private set; } = string.Empty;
    internal Vector2 Offset { get; private set; }

    private void Changed(object? sender, PropertyChangedEventArgs args) => _dirty = true;
    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
    {
        SubscribeItems();
        _dirty = true;
    }

    private void SubscribeItems()
    {
        foreach (var item in _subscribed)
            item.PropertyChanged -= Changed;
        _subscribed.Clear();
        foreach (var item in Definition.Accents)
        {
            item.PropertyChanged += Changed;
            _subscribed.Add(item);
        }
    }

    internal void Synchronize()
    {
        if (!_dirty)
            return;
        _dirty = false;
        GeometryRevision++;
        _segments.Clear();
        var d = Definition;
        var a = d.Appearance;
        var t = d.Transform;
        IsVisible = d.Visible;
        Title = d.Title;
        Subtitle = d.Subtitle;
        Offset = t.Position;
        for (int i = 1; i < Mark.Length; i++)
            Add(Mark[i - 1], Mark[i], a.Primary, a.Stroke, t);
        if (a.ShowFrame)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.ToRadians(30 + i * 60);
                float next = angle + MathF.PI / 3;
                Add(new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 125, new Vector2(MathF.Cos(next), MathF.Sin(next)) * 125, i < 3 ? a.Primary : a.Secondary, 3, t);
            }

        foreach (var accent in d.Accents)
        {
            if (!accent.Visible)
                continue;
            float angle = MathHelper.ToRadians(accent.Angle);
            var radial = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            var tangent = new Vector2(-radial.Y, radial.X);
            Add(radial * 153 - tangent * accent.Length / 2, radial * 153 + tangent * accent.Length / 2, a.Secondary, 5, t);
        }
    }

    private void Add(Vector2 start, Vector2 end, Color color, float thickness, LogoTransformDefinition transform)
    {
        float angle = MathHelper.ToRadians(transform.Rotation);
        var planar = Matrix.CreateScale(transform.Scale.X, transform.Scale.Y, 1) * Matrix.CreateRotationZ(angle);
        var spatial = Definition.Spatial;
        var model = Matrix.CreateScale(spatial.Scale) * Matrix.CreateRotationX(MathHelper.ToRadians(spatial.Rotation.X)) * Matrix.CreateRotationY(MathHelper.ToRadians(spatial.Rotation.Y)) * Matrix.CreateRotationZ(MathHelper.ToRadians(spatial.Rotation.Z)) * Matrix.CreateTranslation(spatial.Position);
        Vector2 Project(Vector2 point)
        {
            Vector3 world = Vector3.Transform(new Vector3(point, 0), model);
            float perspective = 800 / MathF.Max(100, 800 + world.Z);
            return Vector2.Transform(new Vector2(world.X, world.Y) * perspective, planar);
        }

        _segments.Add(new LogoSegment(Project(start), Project(end), color, thickness * MathF.Sqrt(transform.Scale.X * transform.Scale.Y)));
    }

    public void Dispose()
    {
        var d = Definition;
        d.PropertyChanged -= Changed;
        d.Transform.PropertyChanged -= Changed;
        d.Appearance.PropertyChanged -= Changed;
        d.Spatial.PropertyChanged -= Changed;
        d.Accents.CollectionChanged -= CollectionChanged;
        foreach (var item in _subscribed)
            item.PropertyChanged -= Changed;
        _subscribed.Clear();
    }
}
