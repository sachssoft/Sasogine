using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
internal sealed class FileDialogGlyphButton : Button
{
    private readonly string region, fallback;
    internal FileDialogGlyphButton(string region, string fallback)
    {
        this.region = region;
        this.fallback = fallback;
        Padding = Insets2.Zero;
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        base.OnDraw(context);
        var icon = new Bounds2((Bounds.Width - 20) / 2, (Bounds.Height - 20) / 2, 20, 20);
        if (!context.DrawSkinRegion(region, icon, IsAvailable() ? Color.White : new Color(110, 120, 135)))
            context.DrawText(context.DefaultFont, fallback, new Vector2(icon.X + 3, icon.Y), Color.White);
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
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
