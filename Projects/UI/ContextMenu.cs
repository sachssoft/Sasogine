using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Sachssoft.Engine.Input;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// A popup menu; call Show at a root-logical pointer position.
/// </summary>
public sealed class ContextMenu : MenuBar
{
    /// <summary>
    /// Initializes a new context menu instance.
    /// </summary>
    public ContextMenu() : base()
    {
        Vertical = true;
    }

    /// <inheritdoc/>
    protected override string DefaultStyleName => "ContextMenu";

    /// <summary>
    /// Performs show using the current control state.
    /// </summary>
    public void Show(Workspace root, Vector2 position, Control? owner = null) => root.ShowPopup(this, new Bounds2(position.X, position.Y, 0, 0), owner);
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
