using Microsoft.Xna.Framework;
using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Components.Rendering.Animation;

namespace Sachssoft.Sasogine.Inspection.Scene
{
    public class PositionAnimationDefinition : AnimationDefinitionBase, IPositionAnimationDefinition
    {
        /// <summary>
        /// Defines the distance the animation moves in 2D space.
        /// </summary>
        public static readonly IProperty DistanceProperty =
            new StoredProperty<PositionAnimationDefinition, Vector2>(
                nameof(Distance),
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation);

        /// <summary>
        /// Gets or sets the distance vector along which the animation moves.
        /// </summary>
        public virtual Vector2 Distance
        {
            get => GetValue<Vector2>(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }
    }
}
