using Sachssoft.Sasofly.Inspection;
using Sachssoft.Sasogine.Components;

namespace Sachssoft.Sasogine.Rendering.Parallax
{
    public abstract class ParallaxLayerDefinitionBase : IParla
    {

        public readonly static IProperty IndexProperty =
            new StoredProperty<ParallaxLayerDefinitionBase, int>(
                nameof(Index),
                defaultValue: 0,
                category: Sachssoft.Sasogine.Observables.PropertyCategories.Layout,
                coercion: (o, baseValue) => baseValue);

        public virtual int Index
        {
            get => GetValue<int>(IndexProperty);
            set => SetValue(IndexProperty, value);
        }
    }
}
