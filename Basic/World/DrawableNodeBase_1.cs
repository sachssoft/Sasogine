namespace Sachssoft.Sasogine.World
{
    public abstract class DrawableEntityBase<TDefinition> : EntityBase<TDefinition>, IDrawableEntity
        where TDefinition : class, IEntityDefinition
    {
        public virtual void Draw(GameContext gameContext)
        {
            // ...
        }
    }
}
