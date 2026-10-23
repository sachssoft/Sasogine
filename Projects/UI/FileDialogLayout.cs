using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
internal sealed class FileDialogLayout : ContainerControl
{
    private readonly Control[] _controls;
    internal FileDialogLayout(params Control[] controls) : base()
    {
        _controls = controls;
        foreach (Control child in controls)
            Add(child);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        foreach (Control child in Children)
            child.Measure(context, available);
        return new Size2(680, 360);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        void Place(int i, float x, float y, float w, float h) => _controls[i].Arrange(new Bounds2(b.X + x, b.Y + y, MathF.Max(0, w), MathF.Max(0, h)), true);
        Place(0, 0, 0, 32, 32);
        Place(1, 38, 0, 32, 32);
        Place(2, 76, 0, 32, 32);
        Place(3, 114, 0, b.Width - 114, 32);
        float sidebar = MathF.Min(180, b.Width * .32f);
        Place(10, 0, 40, sidebar, MathF.Max(0, b.Height - 122));
        Place(4, sidebar + 8, 40, MathF.Max(0, b.Width - sidebar - 8), MathF.Max(0, b.Height - 122));
        Place(5, 0, b.Height - 74, b.Width * .62f, 32);
        Place(6, b.Width * .62f + 8, b.Height - 74, b.Width * .38f - 8, 32);
        Place(7, 0, b.Height - 34, MathF.Max(0, b.Width - 196), 32);
        Place(8, b.Width - 188, b.Height - 34, 90, 32);
        Place(9, b.Width - 90, b.Height - 34, 90, 32);
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
