using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Input;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    public sealed class SelectionToolTransformLayer : SelectionToolLayer
    {
        public SelectionToolTransformLayer(
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

        public override void Cancel()
        {
        }
    }
}