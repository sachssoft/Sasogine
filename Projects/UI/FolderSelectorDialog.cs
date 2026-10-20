using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sachssoft.Engine.Controls;
/// <summary>FolderSelectorDialog configuration and behavior.</summary>
public sealed class FolderSelectorDialog : FileDialog
{
    /// <summary>Initializes a new folder selector dialog instance.</summary>
    public FolderSelectorDialog() : base()
    {
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "FolderSelectorDialog";
    /// <inheritdoc/>
    protected override bool SelectsDirectory => true;

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
