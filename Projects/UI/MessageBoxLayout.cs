using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace Sachssoft.Engine.Controls;
internal sealed class MessageBoxLayout : ContainerControl
{
    private readonly MessageBox _owner;
    private readonly Label _message;
    private readonly WrapPanel _buttons;
    internal MessageBoxLayout(MessageBox owner, Label message, WrapPanel buttons) : base()
    {
        _owner = owner;
        _message = message;
        _buttons = buttons;
        Add(message);
        Add(buttons);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        _message.Measure(context, new Size2(MathF.Max(0, available.Width - 48), MathF.Max(0, available.Height - 48)));
        _buttons.Measure(context, available);
        return new Size2(340, 130);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        _message.Arrange(new Bounds2(b.X + 48, b.Y, MathF.Max(0, b.Width - 48), MathF.Max(0, b.Height - 46)), true);
        _buttons.Arrange(new Bounds2(b.X + MathF.Max(0, b.Width - _buttons.DesiredSize.Width), b.Bottom - 34, b.Width, 34), true);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        string state = _owner.Icon switch
        {
            MessageBoxIcon.Warning => "Warning",
            MessageBoxIcon.Error => "Error",
            MessageBoxIcon.Question => "Question",
            _ => "Information"
        };
        context.DrawSkinPart(_owner, "MessageGlyph", new Bounds2(0, 4, 32, 32), state);
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }
}
