using Sachssoft.Sasogine.Components.Tools;
using Sachssoft.Sasogine.Components.Tools.Selection;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    public sealed class SelectionToolLayerUpdateContext
    {
        private readonly SelectionTool _selectionTool;

        internal SelectionToolLayerUpdateContext(
            SelectionTool selectionTool)
        {
            _selectionTool = selectionTool;
        }

        public Vector2 CursorPosition => _selectionTool._cursorPosition;

        public bool IsInViewport => _selectionTool._isInViewport;

        public SelectionToolInteractions Interactions => _selectionTool._interactions!;
    }
}
