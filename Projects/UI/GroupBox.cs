namespace Sachssoft.Engine.Controls;
/// <summary>A bordered group using shared header and content presentation.</summary>
public class GroupBox : HeaderContentControl
{
    /// <summary>Creates an empty bordered group.</summary>
    public GroupBox()
    {
    }

    /// <summary>Creates a bordered group with a specialized header host.</summary>
    /// <param name = "headerHost">The unowned control presenting header data.</param>
    protected GroupBox(ContentControl headerHost) : base(headerHost)
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "GroupBox";

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        if (!context.DrawSkinPart(this, "Background", LocalBounds))
        {
            context.FillRectangle(LocalBounds, Background);
            context.StrokeRectangle(LocalBounds, BorderColor, MathF.Max(1, BorderThickness));
        }
    }
}
