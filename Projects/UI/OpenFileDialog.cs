using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>OpenFileDialog configuration and behavior.</summary>
public sealed class OpenFileDialog : FileDialog
{
    /// <summary>Initializes a new open file dialog instance.</summary>
    public OpenFileDialog() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "OpenFileDialog";
    /// <inheritdoc/>
    protected override bool SelectsDirectory => false;

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
