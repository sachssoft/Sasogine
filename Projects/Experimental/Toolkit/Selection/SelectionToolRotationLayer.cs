using Sachssoft.Sasogine.Components.Tools;
using Sachssoft.Sasogine.Components.Tools.Selection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    public sealed class SelectionToolRotationLayer : SelectionToolLayer
    {
        public SelectionToolRotationLayer(
            SelectionTool selectionTool)
            : base(selectionTool)
        {
        }

        public override SelectionToolNode? HitTest(
            Vector2 position)
        {
            return null;
        }

        public override void Update(
            SelectionToolLayerUpdateContext context)
        {
        }

        public override void Draw(
            SelectionToolLayerDrawContext context)
        {
        }
    }
}
