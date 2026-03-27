namespace Sachssoft.Sasogine.World
{
    public abstract class DrawableNodeBase<TDefinition> : NodeBase<TDefinition>, IDrawableNode
        where TDefinition : class, INodeDefinition
    {
        public virtual void Draw(GameContext gameContext)
        {
            // ...
        }
    }
}
