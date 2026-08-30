using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools;
using Sachssoft.Sasogine.Components.Tools.Selection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    public sealed class SelectionToolLayerDrawContext
    {
        private readonly SelectionTool _selectionTool;

        internal SelectionToolLayerDrawContext(
            SelectionTool selectionTool)
        {
            _selectionTool = selectionTool;
        }

        public void DrawNode(
            Vector2 position,
            SelectionToolNodeShape shape)
        {
            var halfSize = _selectionTool.HandleSize / 2f;

            var bounds = new Bounds(
                position.X - halfSize,
                position.Y - halfSize,
                _selectionTool.HandleSize,
                _selectionTool.HandleSize);

            switch (shape)
            {
                case SelectionToolNodeShape.Quad:
                    _selectionTool._pointBatch.AddFillRectangle(bounds);
                    break;

                case SelectionToolNodeShape.Circle:
                    _selectionTool._pointBatch.AddFillEllipse(bounds);
                    break;
            }
        }

        public void DrawLine(
            Vector2 start,
            Vector2 end)
        {
            _selectionTool._lineBatch.AddLine(
                [
                    start,
                    end
                ],
                _selectionTool.LineThickness);
        }
    }
}
