using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Controls.Examples;
/// <summary>Draws prepared entity geometry without modifying the definition or rebuilding caches.</summary>
internal sealed class LogoCanvas : Control
{
    private readonly LogoEntity _entity;
    private IControlFont? _defaultFont, _measuredTitleFont;
    private string? _measuredTitle, _measuredSubtitle;
    private Size2 _titleSize, _subtitleSize;
    internal int MetricsRevision { get; private set; }
    internal IControlFont? TitleFont { get; set; }

    internal LogoCanvas(LogoEntity entity)
    {
        _entity = entity;
        StyleId = "Logo.Canvas";
    }

    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        if (!ReferenceEquals(_defaultFont, context.DefaultFont))
        {
            _defaultFont = context.DefaultFont;
            _measuredSubtitle = null;
        }

        SynchronizeText();
        return new(MathF.Min(available.Width, 850), MathF.Min(available.Height, 740));
    }

    internal void SynchronizeText()
    {
        if (_defaultFont is null)
            return;
        IControlFont font = TitleFont ?? _defaultFont;
        if (_measuredTitle != _entity.Title || !ReferenceEquals(_measuredTitleFont, font))
        {
            _measuredTitle = _entity.Title;
            _measuredTitleFont = font;
            _titleSize = font.Measure(_entity.Title);
            MetricsRevision++;
        }

        if (_measuredSubtitle != _entity.Subtitle)
        {
            _measuredSubtitle = _entity.Subtitle;
            _subtitleSize = _defaultFont.Measure(_entity.Subtitle);
            MetricsRevision++;
        }
    }

    protected override void OnDraw(ControlDrawContext context)
    {
        context.FillRectangle(new Bounds2(0, 0, Bounds.Width, Bounds.Height), Background);
        var grid = Color.FromNonPremultiplied(255, 255, 255, 8);
        for (float x = 24; x < Bounds.Width; x += 32)
            context.DrawLine(new(x, 0), new(x, Bounds.Height), grid);
        for (float y = 24; y < Bounds.Height; y += 32)
            context.DrawLine(new(0, y), new(Bounds.Width, y), grid);
        context.DrawText(context.DefaultFont, "ENTITY / LOGO", new(24, 22), TextColor);
        if (!_entity.IsVisible)
            return;
        float scale = MathF.Min(Bounds.Width / 760, Bounds.Height / 670);
        Vector2 center = new Vector2(Bounds.Width / 2, Bounds.Height * .43f) + _entity.Offset * scale;
        for (int index = 0; index < _entity.Segments.Count; index++)
        {
            LogoSegment segment = _entity.Segments[index];
            context.DrawLine(center + segment.Start * scale, center + segment.End * scale, Color.FromNonPremultiplied(segment.Color.ToVector4() * .1f), segment.Thickness * scale + 12);
            context.DrawLine(center + segment.Start * scale, center + segment.End * scale, segment.Color, segment.Thickness * scale);
        }

        var font = TitleFont ?? context.DefaultFont;
        context.DrawText(font, _entity.Title, new((Bounds.Width - _titleSize.Width) / 2, Bounds.Height * .72f), TextColor);
        context.DrawText(context.DefaultFont, _entity.Subtitle, new((Bounds.Width - _subtitleSize.Width) / 2, Bounds.Height * .72f + 64), new Color(139, 157, 184));
        context.DrawText(context.DefaultFont, "Definition → Entity → cached geometry", new(24, Bounds.Height - 42), new Color(139, 157, 184));
    }
}
