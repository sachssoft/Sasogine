using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasofly.Inspection.Descriptors;
using Sachssoft.Sasogine.Components.Rendering.Animation;

namespace Sachssoft.Sasogine.Inspection.Scene
{
    public abstract class AnimationDefinitionBase : DataInspection, IAnimationDefinition
    {
        public readonly static IProperty SpeedProperty =
            new StoredProperty<AnimationDefinitionBase, float>(
                nameof(Speed),
                defaultValue: 10f,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation,
                descriptors: [new RoundValueDescriptor(2)]);

        /// <summary>
        /// Gets or sets the speed multiplier of the animation.
        /// Affects all position and rotation calculations.
        /// </summary>
        public virtual float Speed
        {
            get => GetValue<float>(SpeedProperty);
            set => SetValue(SpeedProperty, value);
        }

        public readonly static IProperty DurationProperty =
            new StoredProperty<AnimationDefinitionBase, int>(
                nameof(Duration),
                defaultValue: 100,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation,
                coercion: (o, baseValue) => int.Max(baseValue, 0));

        /// <summary>
        /// Gets or sets the total duration of the animation in milliseconds.
        /// Must be zero or positive. Ignored if <see cref="Infinite"/> is true.
        /// </summary>
        public virtual int Duration
        {
            get => GetValue<int>(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        public readonly static IProperty InfiniteProperty =
            new StoredProperty<AnimationDefinitionBase, bool>(
                nameof(Infinite),
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation);

        /// <summary>
        /// Gets or sets whether the animation runs indefinitely.
        /// Overrides <see cref="Duration"/> if true.
        /// </summary>
        public virtual bool Infinite
        {
            get => GetValue<bool>(InfiniteProperty);
            set => SetValue(InfiniteProperty, value);
        }

        public readonly static IProperty DelayProperty =
            new StoredProperty<AnimationDefinitionBase, int>(
                nameof(Delay),
                defaultValue: 0,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Animation,
                coercion: (o, baseValue) => int.Max(baseValue, 0));

        /// <summary>
        /// Gets or sets the start delay of the animation in milliseconds.
        /// Must be zero or positive.
        /// </summary>
        public virtual int Delay
        {
            get => GetValue<int>(DelayProperty);
            set => SetValue(DelayProperty, value);
        }
    }
}
